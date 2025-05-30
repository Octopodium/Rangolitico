using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System;

[Serializable]
public class DialogueNode : Node
{
    public string GUID;
    public string NodeName;
    public string DialogueText;
    public bool _entryPoint = false;
}
