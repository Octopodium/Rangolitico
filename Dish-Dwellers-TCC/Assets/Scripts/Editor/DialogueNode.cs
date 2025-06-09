using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Events;
using System;

[Serializable]
public class DialogueNode : Node
{
    public string GUID;
    public string NodeName;
    public string DialogueText;
    public bool _entryPoint = false;
    public bool _exitPoint = false;

    public string sceneName;
    public bool _loadScene = false;
}
