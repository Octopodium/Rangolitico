using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor;

public class DialogueGraph : EditorWindow
{
    private DialogueGraphView dialogueGraphView;
    private string _fileName;

    [MenuItem("Window/Dialogue Graph")]
    public static void OpenDialogueGraphWindow(){
        var window = GetWindow<DialogueGraph>();
        window.titleContent = new GUIContent("Dialogue Graph");
    }

    private void OnEnable(){
        ConstructGraph();
        GenerateToolbar();
    }

    private void ConstructGraph(){
        dialogueGraphView = new DialogueGraphView{
            name = "Dialogue Graph"
        };

        dialogueGraphView.StretchToParentSize();
        rootVisualElement.Add(dialogueGraphView);
    }

    private void GenerateToolbar(){
        var toolbar = new Toolbar();

        var fileName = new TextField("File Name: ");
        fileName.SetValueWithoutNotify("New Narrative");
        fileName.MarkDirtyRepaint();
        fileName.RegisterValueChangedCallback(x => _fileName = x.newValue);
        toolbar.Add(fileName);

        toolbar.Add(new Button(() => RequestDataOperations(true)){text = "Save"});
        toolbar.Add(new Button(() => RequestDataOperations(false)){text = "Load"});

        var nodeCreateButton =  new Button(() => {
            dialogueGraphView.CreateNode("Text Node");
        });
        nodeCreateButton.text = "CREATE NODE";
        toolbar.Add(nodeCreateButton);

        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperations(bool save){
        if(string.IsNullOrEmpty(_fileName)){
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valid file name.", "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(dialogueGraphView);
        if(save){
            saveUtility.SaveGraph(_fileName);
        }else{
            saveUtility.LoadGraph(_fileName);
        }
    }

    private void OnDisable(){
        rootVisualElement.Remove(dialogueGraphView);
    }
}
