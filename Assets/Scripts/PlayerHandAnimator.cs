using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerHandAnimator : MonoBehaviour {

    public Vector3 joint;
    public Vector3 aerialHandPosition;
    public Vector3 groundSlamHandPosition;
    public float lerpSpeed;

    private Transform estrangedParent;
    private Vector3 restPos;
    private Vector3 scaleFactor { get { return estrangedParent.lossyScale; } }
    private Vector3 jointWorldSpace { get { return Vector3.Scale (joint, scaleFactor) + estrangedParent.position; } }

    // Start is called before the first frame update
    void Start () {

        estrangedParent = transform.parent;

        if (!Application.isPlaying) return;

        restPos = transform.localPosition;
        transform.parent = null;

    }

    // Update is called once per frame
    void LateUpdate () {

#if UNITY_EDITOR
        Debug.DrawLine (transform.position, jointWorldSpace);
        PlayerController.DrawX (aerialHandPosition + estrangedParent.position, 0.1f, Color.magenta);
        PlayerController.DrawX (groundSlamHandPosition + estrangedParent.position, 0.1f, Color.magenta * 0.8f);
#endif

        if (!Application.isPlaying) return;

        transform.localScale = estrangedParent.lossyScale;

        Vector3 dPos = jointWorldSpace - transform.position;
        transform.rotation = Quaternion.Euler (0, 0, Mathf.Rad2Deg * -Mathf.Atan2 (dPos.x, dPos.y));

        transform.position = Vector3.Lerp (transform.position, estrangedParent.position + Vector3.Scale (PlayerController.instance.groundSlamming ? groundSlamHandPosition : (PlayerController.instance.grounded ? restPos : aerialHandPosition), scaleFactor), Time.deltaTime * lerpSpeed);

    }

}