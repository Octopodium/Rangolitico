using UnityEngine;

public class TutorialSettings : MonoBehaviour
{
    public GameObject tutorialTroca;
    public void Start(){
        if(GameManager.instance.modoDeJogo == ModoDeJogo.SINGLEPLAYER){
            tutorialTroca.SetActive(true);
        }
    }

}
