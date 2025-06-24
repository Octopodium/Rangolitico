using UnityEngine;
using UnityEngine.UI;

public class EmoteButton : MonoBehaviour {
    public Image emoteImage;
    public Image buttonImage;

    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;

    public string emoteName;
    

    void Start() {
        Unselected();
    }


    public void Selected() {
        buttonImage.color = selectedColor;
    }

    public void Unselected() {
        buttonImage.color = normalColor;
    }
}
