using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem instance {get; private set;}

    [Header("Ui Elements")]
    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    private float textSpeed = 0.05f;

    private DialogueContainer currentDialogue;
    private DialogueNodeData currentNode;
    private Queue<string> currentSentences;
    private bool isTyping = false;
    private string fullText;

    private Dictionary<string, DialogueNodeData> nodeLookup;

    private void Awake(){
        if (instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }else{
            Destroy(gameObject);
        }

        dialoguePanel.SetActive(false);
    }

    public void StartDialogue(DialogueContainer dialogue){
        currentDialogue = dialogue;

        BuildNodeLookup();
        var entryNode = dialogue.NodeLinks.Find(x => x.portName == "Next");
        currentNode = nodeLookup[entryNode.targetNodeGuid];

        dialoguePanel.SetActive(true);
        DisplayNextSentence();
    }

    private void BuildNodeLookup(){
        nodeLookup = new Dictionary<string, DialogueNodeData>();
        foreach(var node in currentDialogue.DialogueNodeData){
            nodeLookup[node.Guid] = node;
        }
    }

    private void DisplayNextSentence(){
        if(isTyping){
            CompleteSentence();
        }

        fullText = currentNode.dialogueText;
        StartCoroutine("TypeSentence", fullText);
    }

    IEnumerator TypeSentence(string sentence){
        isTyping = true;
        dialogueText.text = "";

        foreach(char letter in sentence.ToCharArray()){
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
        isTyping = false;
    }

    private void CompleteSentence(){
        StopAllCoroutines();
        dialogueText.text = fullText;
        isTyping = false;
    }

    public void AdvanceToNextNode(){
        var link = currentDialogue.NodeLinks.Find(x => x.baseNodeGuid == currentNode.Guid);
        if(link != null && nodeLookup.ContainsKey(link.targetNodeGuid)){
            currentNode = nodeLookup[link.targetNodeGuid];
            DisplayNextSentence();
        }else{
            EndDialogue();
        }
    }

    private void EndDialogue(){
        //dialoguePanel.SetActive(false);
        //proxima cena temporario para build3 e museu
        SceneManager.LoadScene("1-1");
    }
}
