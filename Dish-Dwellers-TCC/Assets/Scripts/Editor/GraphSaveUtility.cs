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
            dialogueContainer.DialogueNodeData.Add(new DialogueNodeData
            {
                Guid = node.GUID,
                nodeName = node.NodeName,
                dialogueText = node.DialogueText,
                position = node.GetPosition().position
            });
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
            var tempNode = _targetGraphView.CreateDialogueNode(nodeData.nodeName);
            tempNode.GUID = nodeData.Guid;
            tempNode.DialogueText = nodeData.dialogueText;

            var textField = tempNode.mainContainer.Q<TextField>();
            if (textField != null) {
                textField.SetValueWithoutNotify(nodeData.dialogueText);
            }

            _targetGraphView.AddElement(tempNode);

            var nodePorts = _containerCache.NodeLinks.Where(x => x.baseNodeGuid == nodeData.Guid).ToList();
            nodePorts.ForEach(x => _targetGraphView.GeneratePort(tempNode, Direction.Output));
        }
    }

    private void ConnectNodes(){
        for(int i = 0; i < Nodes.Count; i++){
            var connections = _containerCache.NodeLinks.Where(x => x.baseNodeGuid == Nodes[i].GUID).ToList();
            for(int j = 0; j < connections.Count; j++){
                var targetNodeGuid = connections[j].targetNodeGuid;
                var targetNode = Nodes.First(x => x.GUID == targetNodeGuid);
                LinkNodes(Nodes[i].outputContainer[j].Q<Port>(), (Port) targetNode.inputContainer[0]);
            
                targetNode.SetPosition(new Rect(
                    _containerCache.DialogueNodeData.First(x => x.Guid == targetNodeGuid).position,
                    _targetGraphView.defaultNodeSize
                ));
            }
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