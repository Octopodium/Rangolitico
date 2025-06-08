using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;

public class GraphSaveUtility
{
    private DialogueGraphView _targetGraphView;
    private DialogueContainer _containerCache;

    private List<Edge> Edges => _targetGraphView.edges.ToList();
    private List<DialogueNode> Nodes => _targetGraphView.nodes.ToList().Cast<DialogueNode>().ToList();

    public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName){
        var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
        var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
        for(var i = 0; i < connectedPorts.Length; i++){
            var outputNode = connectedPorts[i].output.node as DialogueNode;
            var inputNode = connectedPorts[i].input.node as DialogueNode;

            dialogueContainer.NodeLinks.Add(new NodeLinkData
            {
                baseNodeGuid = outputNode.GUID,
                portName = connectedPorts[i].output.portName,
                targetNodeGuid = inputNode.GUID
            });
        }

        foreach (var node in Nodes.Where(node => !node._entryPoint))
        {
            var nodeData = new DialogueNodeData{
                Guid = node.GUID,
                nodeName = node.NodeName,
                dialogueText = node.DialogueText,
                position = node.GetPosition().position,
                _loadScene = node._exitPoint ? node._loadScene : false,
                sceneName = node._exitPoint ? node.sceneName : ""
            };

            dialogueContainer.DialogueNodeData.Add(nodeData);
        }
        
        if(!AssetDatabase.IsValidFolder("Assets/Resources")){
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();

    }

    public void LoadGraph(string fileName){
        _containerCache = Resources.Load<DialogueContainer>(fileName);
        if(_containerCache == null){
            EditorUtility.DisplayDialog("File Not Found", "This dialogue graph does not exists.", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ClearGraph(){
        Nodes.Find(x => x._entryPoint).GUID = _containerCache.NodeLinks[0].baseNodeGuid;

        foreach(var node in Nodes){
            if(node._entryPoint) continue;
            Edges.Where(x => x.input.node == node).ToList().ForEach(edge => _targetGraphView.RemoveElement(edge));
            _targetGraphView.RemoveElement(node);
        }
    }

    private void CreateNodes(){
        foreach(var nodeData in _containerCache.DialogueNodeData){
            var isExitNode = nodeData.nodeName == "End";
            var tempNode = isExitNode 
            ? _targetGraphView.GenerateExitNode()
            : _targetGraphView.CreateDialogueNode(nodeData.nodeName);

            tempNode.GUID = nodeData.Guid;
            tempNode.SetPosition(new Rect(nodeData.position, _targetGraphView.defaultNodeSize));

            if(isExitNode){
                tempNode._loadScene = nodeData._loadScene;
                tempNode.sceneName = nodeData.sceneName ?? "";
            }else{
                tempNode.DialogueText = nodeData.dialogueText;    
                var textField = tempNode.mainContainer.Q<TextField>();
                if (textField != null) {
                    textField.SetValueWithoutNotify(nodeData.dialogueText);
                }
            }
            var nodePorts = _containerCache.NodeLinks.Where(x => x.baseNodeGuid == nodeData.Guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.GeneratePort(tempNode, Direction.Output));
            _targetGraphView.AddElement(tempNode);
        }
    }

    private void ConnectNodes(){
        foreach(var link in _containerCache.NodeLinks){
            var outputNode = Nodes.FirstOrDefault(n => n.GUID == link.baseNodeGuid);
            var inputNode = Nodes.FirstOrDefault(n => n.GUID == link.targetNodeGuid);
        
            if(outputNode == null || inputNode == null) continue;

            var outputPort = outputNode.outputContainer.Query<Port>().Where(p => p.portName == link.portName).First();

            var inputPort = (Port)inputNode.inputContainer[0];
        
            LinkNodes(outputPort, inputPort);
        }
    }

    private void LinkNodes(Port outputSocket, Port inputSocket){
        var tempEdge = new Edge{
            output = outputSocket,
            input = inputSocket
        };
        tempEdge?.input.Connect(tempEdge);
        tempEdge?.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }

}