using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerLegAnimator : MonoBehaviour {

    public Vector3 joint;
    public Vector3 raycastDirectionIdle;
    public float maxIdleFootDistance;
    [Header ("Walking Parameters")]
    public AnimationCurve stepLegLiftAnimation;
    public float stepSpeedSmol;
    public float stepSpeedMedium;
    public float stepSpeedBeeg;
    [Range (0, 1)]
    public float stepPhase;
    public Vector3 raycastDirectionWalk;
    public float maxStepDistance;
    [Header ("Aerial Parameters")]
    public Vector3 aerialStepPosition;
    public Vector3 groundSlamStepPosition;
    public float aerialStepSwaySpeed;
    public float aerialStepSwayMagnitude;
    public float aerialStepLerpSpeed;

    private Vector3 _position;
    private Vector3 stepFrom;
    private Vector3 stepPosition;
    //The scaling factor for vectors, which should face animations face the correct way, as well as ensure that big mode's legs can reach farther without being considered aerial
    private Vector3 scaleFactor { get { return transform.parent.lossyScale; } }
    private Vector3 jointWorldSpace { get { return Vector3.Scale (joint, scaleFactor) + transform.parent.position; } }
    private Vector3 raycastDirectionWalkWorldSpace { get { return Vector3.Scale (raycastDirectionWalk, scaleFactor) + transform.parent.position; } }
    private Vector3 raycastDirectionIdleWorldSpace { get { return Vector3.Scale (raycastDirectionIdle, scaleFactor) + transform.parent.position; } }
    private float maxStepDistanceScaled { get { return maxStepDistance * transform.parent.lossyScale.x; } }
    private float rayDistance { get { return Vector2.Distance (Vector3.Scale (joint, scaleFactor), Vector3.Scale (raycastDirectionWalk, scaleFactor)); } }
    private float stepTime;
    private bool updatedStep;
    private bool isAerial;

    // Start is called before the first frame update
    void Start () {
    }

    // Update is called once per frame
    void Update () {

#if UNITY_EDITOR
        PlayerController.DrawX (aerialStepPosition + transform.parent.position, 0.1f, Color.cyan);
        PlayerController.DrawX (groundSlamStepPosition + transform.parent.position, 0.1f, Color.blue);
        Debug.DrawLine (jointWorldSpace, raycastDirectionWalkWorldSpace, Color.red);
        Debug.DrawLine (jointWorldSpace, raycastDirectionIdleWorldSpace, Color.red * 0.8f);
        Debug.DrawLine (raycastDirectionWalkWorldSpace, raycastDirectionWalkWorldSpace + (Vector3.left * maxStepDistanceScaled), Color.black);
        Debug.DrawLine (raycastDirectionIdleWorldSpace, raycastDirectionIdleWorldSpace + (Vector3.left * maxIdleFootDistance), Color.black);
#endif

        if (!Application.isPlaying) return;

        float stepSpeed = stepSpeedMedium;
        if (PlayerController.instance.sizeMode == SizeModes.SMOL)
            stepSpeed = stepSpeedSmol;
        if (PlayerController.instance.sizeMode == SizeModes.BEEG)
            stepSpeed = stepSpeedBeeg;
        stepTime = Time.time * stepSpeed + stepPhase;
        //This leg is idle if the player isn't moving, or if they're being held
        bool idle = Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.5f || Resizer.instance.holdingPlayer;

        RaycastHit2D rayHit = Physics2D.Raycast (jointWorldSpace, Vector3.Scale (idle ? raycastDirectionIdle : raycastDirectionWalk, scaleFactor), rayDistance, PlayerController.instance.groundLayers);
        Vector3 rayHitPos = rayHit.point;
        Vector3 dPos = jointWorldSpace - transform.position;
        Debug.DrawRay (stepPosition, dPos);
        Debug.DrawRay (rayHitPos, Vector3.up * 0.1f, Color.green);
        if (rayHit && rayHit.collider) {
            if (stepTime % 2 < 1) {
                transform.position = stepPosition;
                updatedStep = false;
            } else {
                float fac = Mathf.Max ((stepTime % 2) - 1, 0);
                if (isAerial) {
                    stepPosition = rayHitPos;
                    isAerial = false;
                }
                if (!updatedStep && Vector3.Distance (stepPosition, rayHitPos) > (idle ? maxIdleFootDistance : maxStepDistanceScaled)) {
                    stepFrom = stepPosition;
                    stepPosition = rayHitPos;
                    updatedStep = true;
                }
                transform.position = Vector3.Lerp (stepFrom, stepPosition, updatedStep ? fac : 1);
                transform.position += Vector3.up * stepLegLiftAnimation.Evaluate (updatedStep ? fac : 1);
            }
        } else {
            isAerial = true;
            transform.position = Vector3.Lerp (transform.position, transform.parent.position + Vector3.Scale (PlayerController.instance.groundSlamming ? groundSlamStepPosition : aerialStepPosition, scaleFactor) + new Vector3 (
                (Mathf.PerlinNoise (Time.time * aerialStepSwaySpeed, 0) - 0.5f) * aerialStepSwayMagnitude,
                (Mathf.PerlinNoise (0, Time.time * aerialStepSwaySpeed) - 0.5f) * aerialStepSwayMagnitude
                ), Time.deltaTime * aerialStepLerpSpeed);
            stepFrom = transform.position;
            stepPosition = transform.position;
        }
        transform.rotation = Quaternion.Euler (0, 0, Mathf.Rad2Deg * -Mathf.Atan2 (dPos.x, dPos.y));
        _position = transform.position;
    }

}