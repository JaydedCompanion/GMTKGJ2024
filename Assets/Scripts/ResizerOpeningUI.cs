using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResizeMode {
    SHRINK,
    SAME,
    GROW,
    IDLE,
    BLOCKED
}
public class ResizerOpeningUI : MonoBehaviour {

    public ResizeMode status;
    public SpriteRenderer renderer;
    public TMPro.TextMeshPro text;
    public Color colIdle;
    public Color colOpen;
    public Color colBlocked;

    private float alpha;

    // Start is called before the first frame update
    void Start () {

        renderer = GetComponent<SpriteRenderer> ();
        text = GetComponentInChildren<TMPro.TextMeshPro> ();
        alpha = renderer.color.a;

    }

    // Update is called once per frame
    void Update () {

        Color col = renderer.color;
        switch (status) {
        case ResizeMode.IDLE:
            col = colIdle;
            text.color = colIdle;
            text.text = "";
            break;
        case ResizeMode.BLOCKED:
            col = colBlocked;
            col.a = alpha;
            text.color = colBlocked;
            text.text = "×";
            break;
        case ResizeMode.SHRINK:
            text.text = "--";
            break;
        case ResizeMode.GROW:
            text.text = "++";
            break;
        case ResizeMode.SAME:
            text.text = "==";
            break;
        }
        if (status == ResizeMode.SHRINK || status == ResizeMode.GROW || status == ResizeMode.SAME) {
            col = colOpen;
            col.a = alpha;
            text.color = colOpen;
        }
        renderer.color = col;

    }

}