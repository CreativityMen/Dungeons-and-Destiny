/*
    Title: InteractData.cs
    Author: Afonso Marques
    
    Description: This script allows any Game Object with
    InteractHandler.cs to show/hide properties based on
    the chosen enum type. (e.g. If NPC, then show NPC properties)
 */

using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static InteractHandler;

// Makes a custom editor in the hierarchy that depends on InteractHandler.cs
[CustomEditor(typeof(InteractHandler))]
public class InteractData : Editor
{
    // Properties (We're just fetching the same variables as InteractHandler.cs)
    SerializedProperty interactableType;
    SerializedProperty isInteracting;

    SerializedProperty threshold;

    SerializedProperty ClassName;

    SerializedProperty NPCName;
    SerializedProperty canTalk;
    SerializedProperty Messages;

    private void OnEnable()
    {
        interactableType = serializedObject.FindProperty("interactableType");
        isInteracting = serializedObject.FindProperty("isInteracting");

        threshold = serializedObject.FindProperty("threshold");

        ClassName = serializedObject.FindProperty("ClassName");

        NPCName = serializedObject.FindProperty("NPCName");
        canTalk = serializedObject.FindProperty("CanTalk");
        Messages = serializedObject.FindProperty("Messages");
    }

    public override void OnInspectorGUI()
    {
        // Updates each property
        serializedObject.Update();

        // Shows the threshold float value and everything else below the way Unity normally would
        EditorGUILayout.PropertyField(threshold);
        EditorGUILayout.LabelField("Is Interacting", InteractHandler.isInteracting.ToString());

        EditorGUILayout.PropertyField(ClassName);
        EditorGUILayout.PropertyField(interactableType);

        // Creates a variable for the type, and only uses index values for each enum since it can only be read that way
        var type = (InteractableType)interactableType.enumValueIndex;

        // If the type is equal to... then it will show its respective properties
        switch (type)
        {
            case InteractableType.NPC:
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(NPCName);
                EditorGUILayout.PropertyField(canTalk, false);


                if (canTalk.boolValue)
                {
                    EditorGUILayout.PropertyField(Messages, true);
                    ClassName.stringValue = "DialogueHandler";
                }
                else
                {
                    EditorGUILayout.PropertyField(Messages, false);
                    ClassName.stringValue = "DialogueHandler";
                }
                
                break;

            case InteractableType.Item:
                EditorGUILayout.Space();
                ClassName.stringValue = "[ObjectName = ClassName]";
                break;
        }

        // Applies the modified properties
        serializedObject.ApplyModifiedProperties();
    }
}
