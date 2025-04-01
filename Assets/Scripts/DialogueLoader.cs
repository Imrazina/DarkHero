using UnityEngine;
using System.IO;

public static class DialogueLoader
{
    public static DialogueData LoadDialogue(string fileName)
    {
        TextAsset file = Resources.Load<TextAsset>(fileName);
        if (file == null)
        {
            Debug.LogError($"Dialogue file not found: {fileName}");
            return null;
        }

        DialogueData data = JsonUtility.FromJson<DialogueData>(file.text);
        if (data == null || data.dialogues == null || data.dialogues.Count == 0)
        {
            Debug.LogError($"Invalid dialogue data in file: {fileName}");
            return null;
        }
        
        return data;
    }
}