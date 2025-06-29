using UnityEngine;
using UnityEngine.UI;

public class EmoteButton : MonoBehaviour {
    public Image emoteImage;
    public Image buttonImage;

    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    public float scaleNormal = 1f;
    public float scaleSelected = 1.2f;

    public string emoteName;
    

    void Start() {
        Unselected();
    }


    public void Selected() {
        buttonImage.color = selectedColor;
        transform.localScale = Vector3.one * scaleSelected;
    }

    public void Unselected() {
        buttonImage.color = normalColor;
        transform.localScale = Vector3.one * scaleNormal;
    }
}
