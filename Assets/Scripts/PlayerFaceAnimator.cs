using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (SpriteRenderer))]
public class PlayerFaceAnimator : MonoBehaviour {

    public Sprite idle;
    public Sprite walking;
    public Sprite blink;
    public Sprite smile;
    public Sprite grimace;
    public Sprite [] easterEggs;
    public float minBlinkTime;
    public float maxBlinkTime;
    public float blinkDuration;
    public float blinkTimer;
    public float idleDuration;
    public float idleTimer;

    private Transform estrangedParent;
    private SpriteRenderer renderer;
    private bool wasWalking;

    // Start is called before the first frame update
    void Start () {

        estrangedParent = transform.parent;
        transform.parent = null;
        renderer = GetComponent<SpriteRenderer> ();
        idleTimer = idleDuration;
    }

    // Update is called once per frame
    void Update () {

        blinkTimer -= Time.deltaTime;
        idleTimer -= Time.deltaTime;

        if (
            Mathf.Abs (Input.GetAxisRaw ("Horizontal")) > 0.5f ||
            Mathf.Abs (Input.GetAxisRaw ("Vertical")) > 0.5f ||
            Input.GetButton ("Submit") ||
            Input.GetButton ("Jump")
            )
            idleTimer = idleDuration;

        if (idleTimer < 0 && idleTimer > -1) {
            blinkTimer = blinkDuration;
            idleTimer = -1;
        }

        if (blinkTimer <= blinkDuration) {
            renderer.sprite = blink;
            if (blinkTimer < 0) {
                blinkTimer = Random.Range (minBlinkTime, maxBlinkTime);
                renderer.sprite = idle;
                if (idleTimer < 0)
                    renderer.sprite = easterEggs [Random.Range (0, easterEggs.Length)];
            }
        } else {
            if (Mathf.Abs (Input.GetAxisRaw ("Horizontal")) > 0.5f && !Resizer.instance.holdingPlayer) {
                renderer.sprite = walking;
                if (!wasWalking) {
                    blinkTimer = blinkDuration;
                    wasWalking = true;
                }
            } else if (wasWalking) {
                blinkTimer = blinkDuration;
                wasWalking = false;
            }
		}

        if (PlayerController.instance.groundSlamming)
            renderer.sprite = grimace;

        transform.position = estrangedParent.position - Vector3.forward * 0.2f;
        transform.localScale = Vector3.Lerp (transform.localScale, PlayerController.instance.facingDirection, Time.deltaTime * PlayerController.instance.scaleLerpSpeed);

    }

}