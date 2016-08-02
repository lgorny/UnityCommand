using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(CommandTest))]
public class CommandTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CommandTest commandTest = (CommandTest)target;
        EditorGUILayout.TextArea(commandTest.commandsQueue);
    }
}
