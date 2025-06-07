using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

public class DialogueGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(200,300);

    public DialogueGraphView(){
        styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        var gridBackgorund = new GridBackground();
        Insert(0, gridBackgorund);
        gridBackgorund.StretchToParentSize();

        AddElement(GenerateEntryNode());
        AddElement(GenerateExitNode());
    }

    private DialogueNode GenerateEntryNode(){
        var node = new DialogueNode{
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = "ENTRY DIALOGUE",
            _entryPoint = true
        };

        node.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        node.AddToClassList("entry-node");

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        node.capabilities &= ~Capabilities.Deletable;

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100,200,100,200));
        return node;
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter){
        var compatiblePorts = new List<Port>();
        ports.ForEach((port) => {
            if(startPort != port && startPort.node != port.node && startPort.direction != port.direction){
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }

    public Port GeneratePort(DialogueNode node, Direction direction, Port.Capacity capacity = Port.Capacity.Single){
        return node.InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(float));
    }

    public void CreateNode(string nodeName){
        AddElement(CreateDialogueNode(nodeName));
    }

    public DialogueNode CreateDialogueNode(string nodeName){
        var dialogueNode = new DialogueNode{
            title = nodeName,
            NodeName = nodeName,
            DialogueText = "Write your conversation here",
            GUID = Guid.NewGuid().ToString()
        };

        var dialogueConversation = CreateTextField();
        dialogueConversation.RegisterValueChangedCallback(evt =>
        {
            dialogueNode.DialogueText = evt.newValue;
        });
        dialogueConversation.SetValueWithoutNotify(dialogueNode.DialogueText);
        dialogueNode.mainContainer.Add(dialogueConversation);

        var inputPort = GeneratePort(dialogueNode, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "input";
        dialogueNode.inputContainer.Add(inputPort);

        var outputPort = GeneratePort(dialogueNode, Direction.Output);
        outputPort.portName = "output";
        dialogueNode.outputContainer.Add(outputPort);

        dialogueNode.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

        dialogueNode.RefreshExpandedState();
        dialogueNode.RefreshPorts();
        dialogueNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

        return dialogueNode;
    }

    public TextField CreateTextField(){
        var dialogueConversation = new TextField("");
        dialogueConversation.multiline = true;
        dialogueConversation.style.height = 100;
        dialogueConversation.style.width = 200;

        dialogueConversation.style.whiteSpace = WhiteSpace.Normal;
        dialogueConversation.style.unityOverflowClipBox = OverflowClipBox.ContentBox;

        foreach (VisualElement child in dialogueConversation.Children()){
            child.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.UpperLeft);
        }

        return dialogueConversation;
    }

    public DialogueNode GenerateExitNode(){ //criando um exit node pra gente chamar a cena no final do dialogo
        var node = new DialogueNode{
            NodeName = "End",
            title = "END",
            GUID = Guid.NewGuid().ToString(),
            DialogueText = "Exit Node",
            _exitPoint = true,
            sceneName = "1-1",
            _loadScene = false
        };
        var toggle = new Toggle("Load Scene?") { value = node._loadScene };
        toggle.RegisterValueChangedCallback(evt => node._loadScene = evt.newValue);

        var sceneField = new TextField("Scene Name:") { value = node.sceneName };
        sceneField.RegisterValueChangedCallback(evt => node.sceneName = evt.newValue);
        sceneField.SetEnabled(node._loadScene);
    
        toggle.RegisterValueChangedCallback(evt => {
            sceneField.SetEnabled(evt.newValue);
        });

        node.mainContainer.Add(toggle);
        node.mainContainer.Add(sceneField);

        node.capabilities &= ~Capabilities.Deletable;

        node.styleSheets.Add(Resources.Load<StyleSheet>("Node"));
        node.AddToClassList("exit-node");

        var generatedPort = GeneratePort(node, Direction.Input);
        generatedPort.portName = "End";
        node.inputContainer.Add(generatedPort);

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(500,200,500,200));
        return node;
    }
}
