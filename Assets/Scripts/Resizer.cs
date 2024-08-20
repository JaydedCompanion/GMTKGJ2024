using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Directions {
	NONE,
	LEFT,
	RIGHT,
	TOP
}

[ExecuteInEditMode]
[RequireComponent (typeof (Collider2D))]
public class Resizer : MonoBehaviour {

	public static Resizer instance;

	public bool dormant = true;
	[Header ("Player Grab Parameters")]
	public bool holdingPlayer;
	public Directions directionEntered;
	public float lowestTouchY;
	public float highestTouchY;
	public float grabLerpSpeed;
	public float yeetForceUp;
	public float yeetForceHorizontal;
	public float exitClearanceDistance;
	[Header ("Break/Repair Parameters")]
	public ParticleSystem appearParticles;
	public List<Rigidbody2D> pieceInstances = new List<Rigidbody2D> ();
	public float pieceDistanceToRepair;
	public float pieceMinDistanceAfterDestruction;
	public float explodeForce;
	public int appearParticleCount = 20;
	[Header ("Prompt Visualizer Parameters")]
	public LayerMask openingBlockers;
	public ResizerOpeningUI openingTop;
	public ResizerOpeningUI openingLeft;
	public ResizerOpeningUI openingRight;

	private Rigidbody2D rb;
	public Collider2D coll { get; private set; }
	private Vector2 averagePiecePos;
	private bool piecesSeparating;
	private bool waitingForKeyRelease; //Used to prevent the resizer from automatically spitting the player out if they're holding down a direction when they enter the resizer

	private void Start () {
		instance = this;
		rb = GetComponent<Rigidbody2D> ();
		coll = GetComponent<Collider2D> ();
		if (!Application.isPlaying) return;
		foreach (GameObject g in GameObject.FindGameObjectsWithTag ("ResizerPiece"))
			pieceInstances.Add (g.GetComponent<Rigidbody2D> ());
	}

	// Update is called once per frame
	void Update () {

#if UNITY_EDITOR
		Debug.DrawLine (
			transform.position + Vector3.left + (Vector3.up * lowestTouchY),
			transform.position + Vector3.right + (Vector3.up * lowestTouchY),
			Color.cyan
			);
		Debug.DrawLine (
			transform.position + Vector3.left + (Vector3.up * highestTouchY),
			transform.position + Vector3.right + (Vector3.up * highestTouchY),
			Color.cyan
			);
		Debug.DrawLine (
			transform.position + (Vector3.left * exitClearanceDistance),
			transform.position + (Vector3.right * exitClearanceDistance),
			Color.red
			);
		Debug.DrawLine (
			transform.position,
			transform.position + (Vector3.up * exitClearanceDistance),
			Color.red
			);
		pieceMinDistanceAfterDestruction = Mathf.Max (pieceMinDistanceAfterDestruction, pieceDistanceToRepair);
#endif

		if (!Application.isPlaying) return;

		averagePiecePos = Vector3.zero;
		foreach (Rigidbody2D instance in pieceInstances)
			averagePiecePos += instance.position;
		averagePiecePos /= pieceInstances.Count;

		transform.GetChild(0).gameObject.SetActive (!dormant);

		UpdateOpeningClearance ();

		if (dormant) {
			//Move self to the average position of all pieces, which will facilitate the calculation of whether all pieces are near one another
			transform.position = averagePiecePos;
			//Calculate whether all pieces of this Resizer are near eachother
			bool allInRange = true;
			foreach (Rigidbody2D instance in pieceInstances)
				if (Vector3.Distance (transform.position, instance.position) > pieceDistanceToRepair) {
					allInRange = false;
					break;
				}
			//Spawn self into the world if all pieces are in range
			if (allInRange && !piecesSeparating) {
				dormant = false;
				foreach (Rigidbody2D instance in pieceInstances)
					instance.gameObject.SetActive (false);
				rb.velocity = Vector3.zero;
				PlayerController.instance.holdingObject = null;
				if (PlayerController.instance.holdingObjectCollider)
					PlayerController.instance.holdingObjectCollider.enabled = true;
				PlayerController.instance.holdingObjectCollider = null;
				appearParticles.Emit (appearParticleCount);
			}
			bool someOutsideRange = false;
			foreach (Rigidbody2D instance in pieceInstances)
				if (Vector3.Distance (transform.position, instance.position) > pieceMinDistanceAfterDestruction) {
					someOutsideRange = true;
					break;
				}
			if (someOutsideRange)
				piecesSeparating = false;
			//Disable collision while dormant
			coll.enabled = false;
		} else if (!PlayerController.instance.holdingObject || PlayerController.instance.holdingObject != rb) {
			//If holding the player, freeze the box in place and disable the collider
			rb.isKinematic = holdingPlayer;
			coll.enabled = !holdingPlayer;
			if (holdingPlayer) {
				rb.velocity = Vector3.zero;
				PlayerController inst = PlayerController.instance;
				//If holding player, disable their physics and pull them into the center of the box
				inst.rb.isKinematic = true;
				inst.rb.velocity = Vector3.zero;
				inst.transform.position = Vector3.Lerp (inst.transform.position, transform.position, grabLerpSpeed);
				Physics2D.IgnoreCollision (coll, inst.coll, true);
				//Ignore input until directional keys and jump button have been released, which will prevent the player from accidentally exiting by holding down a directional key while entering
				if (Mathf.Abs (Input.GetAxisRaw ("Horizontal")) < 0.5f && Input.GetAxisRaw ("Vertical") < 0.5f)
					waitingForKeyRelease = false;
				Directions releasePlayer = Directions.NONE;
				//If keys have been released since entering the box, register input and store the direction in a variable
				if (!waitingForKeyRelease)
					if (Input.GetAxisRaw ("Horizontal") < -0.5f)
						releasePlayer = Directions.LEFT;
					else if (Input.GetAxisRaw ("Horizontal") > 0.5f)
						releasePlayer = Directions.RIGHT;
					else if (Input.GetAxisRaw ("Vertical") > 0.5f)
						releasePlayer = Directions.TOP;
				//Set the status of the openings
				switch (directionEntered) {
				case Directions.LEFT:
					openingLeft.status = ResizeMode.SAME;
					openingTop.status = ResizeMode.GROW;
					openingRight.status = ResizeMode.GROW;
					break;
				case Directions.RIGHT:
					openingLeft.status = ResizeMode.SHRINK;
					openingTop.status = ResizeMode.SHRINK;
					openingRight.status = ResizeMode.SAME;
					break;
				}
				if (Physics2D.Raycast (transform.position, Vector3.left, exitClearanceDistance, openingBlockers))
					openingLeft.status = ResizeMode.BLOCKED;
				if (Physics2D.Raycast (transform.position, Vector3.right, exitClearanceDistance, openingBlockers))
					openingRight.status = ResizeMode.BLOCKED;
				if (Physics2D.Raycast (transform.position, Vector3.up, exitClearanceDistance, openingBlockers))
					openingTop.status = ResizeMode.BLOCKED;
				//Handle player exiting via different directions
				switch (releasePlayer) {
				//Handle player exiting through the left
				case Directions.LEFT:
					//Stop the player from exiting if the exit is blocked;
					if (openingLeft.status == ResizeMode.BLOCKED) {
						releasePlayer = Directions.NONE;
						break;
					}
					inst.rb.velocity = Vector3.left * yeetForceHorizontal;
					//If the player didn't enter from the left, then shrink them
					if (directionEntered != Directions.LEFT)
						inst.sizeMode--;
					break;
				//Handle player exiting through the right
				case Directions.RIGHT:
					//Stop the player from exiting if the exit is blocked;
					if (openingRight.status == ResizeMode.BLOCKED) {
						releasePlayer = Directions.NONE;
						break;
					}
					inst.rb.velocity = Vector3.right * yeetForceHorizontal;
					//If the player didn't enter from the right, then enlarge them
					if (directionEntered != Directions.RIGHT)
						inst.sizeMode++;
					break;
				//Handle player exiting through the top
				case Directions.TOP:
					//Stop the player from exiting if the exit is blocked;
					if (openingTop.status == ResizeMode.BLOCKED) {
						releasePlayer = Directions.NONE;
						break;
					}
					inst.rb.velocity = Vector3.up * yeetForceUp;
					//If the player entered from the left, then enlarge them
					if (directionEntered == Directions.LEFT)
						inst.sizeMode++;
					//If the player entered from the right, then shrink them
					if (directionEntered == Directions.RIGHT)
						inst.sizeMode--;
					break;
				}
				//If any direction was pressed, causing the player to exit, release the player
				if (releasePlayer != Directions.NONE) {
					Debug.Log ("Direction Exited: " + releasePlayer);
					holdingPlayer = false;
					inst.rb.isKinematic = false;
				}
			}
		}
	}

	private void OnCollisionEnter2D (Collision2D collision) {
		if (dormant)
			return;
		float collY = collision.GetContact (0).point.y;
		if (collision.collider.tag == "Player" && collY > transform.position.y + lowestTouchY && collY < transform.position.y + highestTouchY) {
			//If the player approaches from the left (which will enlarge them), hold them if they're not at max size
			if (PlayerController.instance.transform.position.x < transform.position.x && PlayerController.instance.sizeMode < SizeModes.BEEG) {
				holdingPlayer = true;
				directionEntered = Directions.LEFT;
			}
			//If the player approaches from the right (which will shrink them), hold them if they're not at min size
			if (PlayerController.instance.transform.position.x > transform.position.x && PlayerController.instance.sizeMode > SizeModes.SMOL) {
				holdingPlayer = true;
				directionEntered = Directions.RIGHT;
			}
			waitingForKeyRelease = true;
		}
	}

	private void OnTriggerExit2D (Collider2D collision) {
		Physics2D.IgnoreCollision (coll, collision, false);
	}

	public void Detonate () {
		foreach (Rigidbody2D instance in pieceInstances) {
			Vector3 rand = new Vector2 (Random.Range (-1f, 1f), Random.Range (-1f, 1f)).normalized;
			instance.transform.position = transform.position + (rand * 0.25f);
			instance.velocity = rand * explodeForce;
			instance.gameObject.SetActive (enabled);
		}
		dormant = true;
		piecesSeparating = true;
	}

	private void UpdateOpeningClearance () {
		openingLeft.status = ResizeMode.IDLE;
		openingRight.status = ResizeMode.IDLE;
		openingTop.status = ResizeMode.IDLE;
		if (Physics2D.Raycast (transform.position, Vector3.left, exitClearanceDistance, openingBlockers))
			openingLeft.status = ResizeMode.BLOCKED;
		if (Physics2D.Raycast (transform.position, Vector3.right, exitClearanceDistance, openingBlockers))
			openingRight.status = ResizeMode.BLOCKED;
		if (Physics2D.Raycast (transform.position, Vector3.up, exitClearanceDistance, openingBlockers))
			openingTop.status = ResizeMode.BLOCKED;
		openingLeft.gameObject.SetActive (PlayerController.instance.sizeMode != SizeModes.BEEG);
		openingRight.gameObject.SetActive (PlayerController.instance.sizeMode != SizeModes.SMOL);

	}

}