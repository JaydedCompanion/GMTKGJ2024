using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialText : MonoBehaviour {

    public float appearSpeed;
    public float disappearSpeed;
    public float startDelay;
    public float zOffset;

    private Transform track;
    private TMPro.TextMeshPro text;
    private Color showCol = new Color (1, 1, 1, 1);
    private Color hideCol = new Color (1, 1, 1, 0);
    private bool startDelayDone;

    // Start is called before the first frame update
    void Start () {
        track = GameObject.Find ("ResizerPiece.Tut").transform;
        text = GetComponent<TMPro.TextMeshPro> ();
        text.color = hideCol;
        StartCoroutine ("AppearDelay");
    }

    // Update is called once per frame
    void Update () {

        bool show = false;

        if (!startDelayDone)
            return;
        if (name == "Tut.Done")
            show = !Resizer.instance.dormant;
        if (Resizer.instance.dormant) {
            switch (name) {
            case ("Tut.TouchToGrab"):
                transform.position = track.position - (Vector3.forward * zOffset);
                show = !PlayerController.instance.holdingObject;
                break;
            case ("Tut.Throw"):
            case ("Tut.Drop"):
                transform.position = PlayerController.instance.transform.position - (Vector3.forward * zOffset);
                goto case "Tut.CombinePieces";
            case ("Tut.CombinePieces"):
                show = PlayerController.instance.holdingObject;
                break;
            }
        }
        text.color = Color.Lerp (text.color, show ? showCol : hideCol, Time.deltaTime * (show ? appearSpeed : disappearSpeed));

    }

    private IEnumerator AppearDelay () {
        yield return new WaitForSecondsRealtime (startDelay);
        startDelayDone = true;
	}

}