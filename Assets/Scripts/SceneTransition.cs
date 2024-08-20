using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour {

    public static SceneTransition instance;
    public SpriteRenderer blocker;
    public Transform[] icons;
    public float speed;

    private Color hiddenCol;
    private Color showingCol;
    private int chosenIcon;
    private bool showing;

    // Start is called before the first frame update
    void Start () {

        if (instance && instance != this) {
            Destroy (gameObject);
            Debug.LogWarning ("Tried spawning a second SceneTransition.");
            return;
        }
        DontDestroyOnLoad (gameObject);
        instance = this;
        SceneManager.sceneLoaded += EndTransition;

        showingCol = blocker.color;
        showingCol.a = 1;
        hiddenCol = blocker.color;
        hiddenCol.a = 0;

    }

    // Update is called once per frame
    void Update () {

        for (int i = 0; i < icons.Length; i++)
            if (i == chosenIcon)
                icons [i].localScale = Vector3.Lerp (icons [i].localScale, showing ? Vector3.one * 2 : Vector3.zero, Time.deltaTime * speed);
            else
                icons [i].localScale = Vector3.zero;
        blocker.color = Color.Lerp (blocker.color, showing ? showingCol : hiddenCol, Time.deltaTime * speed);

    }

    public void StartTransition () {
        chosenIcon = Random.Range (0, icons.Length);
        showing = true;
        transform.position = Camera.main.transform.position + (Vector3.forward * 10);
	}
    public void EndTransition (Scene scene, LoadSceneMode mode) {
        showing = false;
	}

    public static SceneTransition SpawnInstance () {
        if (instance) {
            Debug.LogWarning ("Tried calling SpawnInstance when a SceneTransition was already present in the scene.");
            return instance;
        }
        instance = Instantiate (Resources.Load ("Prefabs/SceneTransition") as GameObject).GetComponent<SceneTransition> ();
        return instance;
	}

	private void OnDestroy () {
        instance = null;
	}

}