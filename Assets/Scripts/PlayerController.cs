using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SizeModes {
    SMOL,
    MEDIUM,
    BEEG
}

[ExecuteInEditMode, RequireComponent (typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {

    public static PlayerController instance;

    [Header ("Sizing Parameters")]
    public SizeModes sizeMode;
    public Vector3 scaleSmol;
    public Vector3 scaleMedium;
    public Vector3 scaleBeeg;
    public float scaleLerpSpeed;
    [Header ("Movement Parameters")]
    [Range (0, 1)]
    public float horizontalDrag;
    public AnimationCurve horizontalRunForce;
    [Range (0, 1)]
    public float horizontalAirDrag;
    public AnimationCurve horizontalAirForce;
    [Header ("Jump Parameters")]
    public float jumpForce;
    public float coyoteTime = 0.1f;
    public bool doubleJumpAvailable;
    public Vector3 groundedRaycastOriginA;
    public Vector3 groundedRaycastOriginB;
    public float groundedRaycastDistance;
    public LayerMask groundLayers;
    [Header ("Ground Slam Parameters")]
    public float groundSlamMinijumpForce;
    [Header ("Object Toss Parameters")]
    public Transform holdPivot;
    public float holdPivotPullForce;
    public Vector2 holdPivotInitialJump;
    [Range (0, 1)]
    public float holdPivotDrag;
    public Rigidbody2D holdingObject;
    public Collider2D holdingObjectCollider;
    public Vector2 throwForce;

    public static UnityEngine.Events.UnityEvent groundSlamShockwave = new UnityEngine.Events.UnityEvent ();
	public bool grounded { get { return Time.time - timeSinceLastGrounded < coyoteTime; } }
    private bool groundSlamming;
    public Rigidbody2D rb { get; private set; }
    public Collider2D coll { get; private set; }
    private Collider2D groundColliderA;
    private Collider2D groundColliderB;
    private Vector3 currentSize;
    private Vector3 facingDirection = Vector3.one;
    private static readonly Vector3 facingRight = new Vector3 (  1, 1, 1 );
    private static readonly Vector3 facingLeft  = new Vector3 ( -1, 1, 1 );
    private float timeSinceLastGrounded;

    // Start is called before the first frame update
    void Start () {

        instance = this;

        if (!Application.isPlaying) return;

        facingDirection = Vector3.one;

        rb = GetComponent<Rigidbody2D> ();
        coll = GetComponent<Collider2D> ();

        holdPivot.parent = null;


    }

    private void Update () {

        Debug.DrawRay (transform.position + Vector3.Scale (groundedRaycastOriginA, transform.lossyScale), Vector2.down * groundedRaycastDistance * transform.lossyScale.y, Color.blue);
        Debug.DrawRay (transform.position + Vector3.Scale (groundedRaycastOriginB, transform.lossyScale), Vector2.down * groundedRaycastDistance * transform.lossyScale.y, Color.blue);

        holdPivot.position = transform.position;

        if (!Application.isPlaying) return;

        switch (sizeMode) {
        case SizeModes.SMOL:
            currentSize = scaleSmol;
            break;
        case SizeModes.MEDIUM:
            currentSize = scaleMedium;
            break;
        case SizeModes.BEEG:
            currentSize = scaleBeeg;
            break;
        }

        if (Input.GetAxisRaw ("Horizontal") < -0.5f)
            facingDirection = facingLeft;
        if (Input.GetAxisRaw ("Horizontal") > 0.5f)
            facingDirection = facingRight;

        currentSize = Vector3.Scale (currentSize, facingDirection);
        transform.localScale = Vector3.Lerp (transform.localScale, currentSize, Time.deltaTime * scaleLerpSpeed);
        holdPivot.localScale = facingDirection;

        if (Input.GetButtonDown ("Jump")) {
            if (grounded) {
                Jump ();
                timeSinceLastGrounded = 0;
                //Only allow double jump if smol
            } else if (doubleJumpAvailable && sizeMode == SizeModes.SMOL) {
                Jump ();
                doubleJumpAvailable = false;
                //Only allow ground slam if beeg
            } else if (!groundSlamming && sizeMode == SizeModes.BEEG) {
                GroundSlam ();
                groundSlamming = true;
            }
        }

        if (holdingObject) {
            if (Input.GetButtonDown ("Submit")) {
                holdingObject.velocity = Vector2.Scale(throwForce, facingDirection) + rb.velocity;
                holdingObject = null;
                holdingObjectCollider.enabled = true;
                holdingObjectCollider = null;
            }
        }

    }

	// Update is called once per frame
	void FixedUpdate () {

        if (!Application.isPlaying) return;

        //If the raycast is hitting the ground, set the last grounded time to the current time.
        RaycastHit2D hitA;
        RaycastHit2D hitB;
        hitA = Physics2D.Raycast (transform.position + Vector3.Scale (groundedRaycastOriginA, transform.lossyScale), Vector2.down, groundedRaycastDistance * transform.lossyScale.y, groundLayers);
        hitB = Physics2D.Raycast (transform.position + Vector3.Scale (groundedRaycastOriginB, transform.lossyScale), Vector2.down, groundedRaycastDistance * transform.lossyScale.y, groundLayers);
        groundColliderA = hitA.collider;
        groundColliderB = hitB.collider;
        if (hitA || hitB) {
            timeSinceLastGrounded = Time.time;
            doubleJumpAvailable = true;
            if (groundSlamming) {
                groundSlamming = false;
                groundSlamShockwave.Invoke ();
                if (hitA.collider == Resizer.instance.coll || hitB.collider == Resizer.instance.coll)
                    Resizer.instance.Detonate ();
            }
        }

        if (holdingObject) {
            holdingObject.AddForce (((Vector2)holdPivot.GetChild(0).position - holdingObject.position) * holdPivotPullForce * Time.fixedDeltaTime);
            holdingObject.velocity *= 1 - holdPivotDrag;
        }

        if (!groundSlamming) {
            Vector3 vel = rb.velocity;
            vel.x += (grounded ? horizontalRunForce : horizontalAirForce).Evaluate ((float)sizeMode / (float)SizeModes.BEEG) * Input.GetAxisRaw ("Horizontal");
            vel.x = vel.x * (1 - (grounded ? horizontalDrag : horizontalAirDrag));
            rb.velocity = vel;
        }

    }

    private void Jump () {
        Debug.Log ("Jumping!");
        Vector3 vel = rb.velocity;
        vel.y = jumpForce;
        rb.velocity = vel;
	}

    private void GroundSlam () {
        groundSlamming = true;
        Vector3 vel = Vector3.zero;
        vel.y = groundSlamMinijumpForce;
        rb.velocity = vel;
    }

	private void OnCollisionStay2D (Collision2D collision) {
        foreach (ContactPoint2D point in collision.contacts)
            DrawCross (point.point, 1, Color.red);
        if (!holdingObject) {
            //Any resizer piece can be grabbed by just touching the object while not holding another object
            if (collision.collider.tag == "ResizerPiece")
                GrabObject (collision);
            //Only allow grabbing the resizer if the player is standing on it (which can be determined by whether the object that the player is standing on is the resizer collider) and they press the interact key
            if (collision.collider.name == "Resizer" && Input.GetButton ("Submit") && (groundColliderA == collision.collider || groundColliderB == collision.collider))
                GrabObject (collision);
        }
    }

    private void GrabObject (Collision2D objectToGrab) {
        holdingObject = objectToGrab.rigidbody;
        holdingObject.velocity = holdPivotInitialJump;
        holdingObjectCollider = objectToGrab.collider;
        holdingObjectCollider.enabled = false;
    }

    private void DrawCross (Vector3 pos, float size, Color col) {
        Debug.DrawLine (pos - (Vector3.one * size), pos + (Vector3.one * size), col);
        Debug.DrawLine (pos - (Vector3.right * size), pos + (Vector3.right * size), col);
    }

}