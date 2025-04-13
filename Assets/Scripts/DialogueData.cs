using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public List<DialogueLine> dialogues;
}

[System.Serializable]
public class DialogueLine
{
    public string id;
    public string name;
    public string text;
    public string avatar;
    public List<DialogueChoice> choices;
}

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public string nextId; 
}