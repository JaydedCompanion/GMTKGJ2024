using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoorway : MonoBehaviour {

    public static ExitDoorway instance;

    public bool open;
    public Transform top;
    public Transform bottom;
    public float openDistance;
    public float lerpSpeed;
    public float delayBeforeNextLevel = 1;

    public bool levelDone { get; private set; }
    private Vector3 startPosTop;
    private Vector3 startPosBottom;

	private void Start () {
        instance = this;
        startPosTop = top.localPosition;
        startPosBottom = bottom.localPosition;
	}

	// Update is called once per frame
	void Update () {
        //Determine whether this gate should open by verifying that no Resizers remain to be built
        open = true;
        if (FindObjectsOfType<Resizer>().Length > 0)
            foreach (Resizer resizer in FindObjectsOfType<Resizer>())
                if (resizer.dormant) {
                    open = false;
                    break;
                }
        top.transform.localPosition = Vector3.Lerp (
            top.transform.localPosition,
            startPosTop + (open ? Vector3.up * openDistance : Vector3.zero),
            Time.deltaTime * lerpSpeed
            );
        bottom.transform.localPosition = Vector3.Lerp (
            bottom.transform.localPosition,
            startPosBottom+ (open ? Vector3.down * openDistance : Vector3.zero),
            Time.deltaTime * lerpSpeed
            );
    }

	private void OnCollisionEnter2D (Collision2D collision) {
		if (collision.collider.tag == "Player" && open) {
            levelDone = true;
            GetComponent<Collider2D> ().enabled = false;
            enabled = false;
            StartCoroutine ("LoadNextScene");
		}
	}

    private IEnumerator LoadNextScene () {
        yield return new WaitForSecondsRealtime (delayBeforeNextLevel);
        SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex + 1);
	}

}