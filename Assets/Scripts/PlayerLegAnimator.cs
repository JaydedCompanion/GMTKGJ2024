using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerLegAnimator : MonoBehaviour {

    public PlayerLegAnimator counterpart;
    public float stepSpeed;
    [Range (0, 1)]
    public float stepPhase;
    public AnimationCurve stepLegLiftAnimation;
    public Vector3 joint;
    public Vector3 raycastDirection;
    public float maxStepDistance;

    private Vector3 stepFrom;
    private Vector3 stepPosition;
    private bool updatedStep;
    private float rayDistance;
    private float stepTime;

    // Start is called before the first frame update
    void Start () {
        rayDistance = Vector2.Distance (joint, raycastDirection);
    }

    // Update is called once per frame
    void Update () {

        Debug.DrawLine (transform.parent.position + joint, transform.parent.position + raycastDirection, Color.red);
        Debug.DrawLine (transform.parent.position + raycastDirection, transform.parent.position + raycastDirection + (Vector3.left * maxStepDistance), Color.black);

        //Ensure both legs don't move at the same time
        //if (counterpart) {
        //    if (myTurn == counterpart.myTurn)
        //        counterpart.myTurn = !myTurn;
        //} else
        //    myTurn = true;


        if (!Application.isPlaying) return;

        stepTime = Time.time * stepSpeed + stepPhase;

        RaycastHit2D rayHit = Physics2D.Raycast (transform.parent.position + joint, transform.parent.position + raycastDirection, rayDistance);
        Vector3 rayHitPos = rayHit.point;

        Vector3 dPos = (transform.parent.position + joint) - stepPosition;
        Debug.DrawRay (transform.parent.position, dPos);
        transform.position = stepPosition;
        transform.rotation = Quaternion.Euler (0, 0, Mathf.Rad2Deg * -Mathf.Atan2 (dPos.x, dPos.y));
        if (stepTime % 2 < 1)
            updatedStep = false;
        if (stepTime % 2 > 1 && !updatedStep) {
            stepFrom = stepPosition;
            stepPosition = rayHitPos;
            updatedStep = true;
        }
        //TODO: Fix the fucking step animation. Basically none of it works, might as well start from scratch...
        transform.position = Vector3.Lerp (stepFrom, stepPosition, Mathf.Max (stepTime % 2, 1) - 1);
        transform.position += Vector3.up * stepLegLiftAnimation.Evaluate (Mathf.Max (stepTime % 2, 1) - 1);
    }
}