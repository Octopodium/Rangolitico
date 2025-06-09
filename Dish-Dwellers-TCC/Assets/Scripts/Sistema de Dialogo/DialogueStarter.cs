using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    public DialogueContainer currentDialogue;

    private void Start(){
        DialogueSystem.instance.StartDialogue(currentDialogue);
    }

}
