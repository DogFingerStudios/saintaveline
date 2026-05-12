using UnityEngine;
using UnityEditor;

// AI: Custom Inspector for DialogNodeSO - provides better UI organization
[CustomEditor(typeof(DialogNodeSO))]
public class DialogNodeSOEditor : Editor
{
    private SerializedProperty _isPlayerSpeaking;
    private SerializedProperty _title;
    private SerializedProperty _line;
    private SerializedProperty _autoAdvance;
    private SerializedProperty _options;
    private SerializedProperty _note;
    private SerializedProperty _onSelectedActions;
    private SerializedProperty _onSelectedActionsSO;
    private SerializedProperty _commandString;

    private bool _showActions = true;

    private void OnEnable()
    {
        _isPlayerSpeaking = serializedObject.FindProperty("IsPlayerSpeaking");
        _title = serializedObject.FindProperty("Title");
        _line = serializedObject.FindProperty("Line");
        _autoAdvance = serializedObject.FindProperty("AutoAdvance");
        _options = serializedObject.FindProperty("Options");
        _note = serializedObject.FindProperty("Note");
        _onSelectedActions = serializedObject.FindProperty("OnSelectedActions");
        _onSelectedActionsSO = serializedObject.FindProperty("OnSelectedActionsSO");
        _commandString = serializedObject.FindProperty("CommandString");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // AI: Dialogue Content Section
        EditorGUILayout.LabelField("Dialogue Content", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_isPlayerSpeaking);
        EditorGUILayout.PropertyField(_title);
        EditorGUILayout.PropertyField(_line);
        EditorGUILayout.PropertyField(_autoAdvance);
        EditorGUILayout.PropertyField(_options);

        EditorGUILayout.Space(10);

        // AI: Actions Section (Collapsible)
        _showActions = EditorGUILayout.BeginFoldoutHeaderGroup(_showActions, "Actions (Execute When Selected)");

        if (_showActions)
        {
            EditorGUI.indentLevel++;

            // AI: Show warning if multiple action types are used
            bool hasEnumActions = _onSelectedActions.arraySize > 0;
            bool hasSOActions = _onSelectedActionsSO.arraySize > 0;
            bool hasCommands = !string.IsNullOrWhiteSpace(_commandString.stringValue);
            int activeSystemCount = (hasEnumActions ? 1 : 0) + (hasSOActions ? 1 : 0) + (hasCommands ? 1 : 0);

            if (activeSystemCount > 1)
            {
                EditorGUILayout.HelpBox(
                    $"This node uses {activeSystemCount} action systems. Actions execute in order:\n" +
                    "1. Enum Actions\n" +
                    "2. ScriptableObject Actions\n" +
                    "3. Command Strings",
                    UnityEditor.MessageType.Info
                );
            }
            else if (activeSystemCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "No actions configured. Choose one or more systems:\n" +
                    "• Enum Actions (simple)\n" +
                    "• ScriptableObject Actions (reusable)\n" +
                    "• Command Strings (flexible)",
                    UnityEditor.MessageType.Warning
                );
            }

            EditorGUILayout.Space(5);

            // AI: Enum Actions
            EditorGUILayout.LabelField("Enum Actions (Simple)", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_onSelectedActions, true);

            EditorGUILayout.Space(5);

            // AI: ScriptableObject Actions
            EditorGUILayout.LabelField("ScriptableObject Actions (Reusable)", EditorStyles.miniLabel);
            EditorGUILayout.PropertyField(_onSelectedActionsSO, true);

            EditorGUILayout.Space(5);

            // AI: Command Strings
            EditorGUILayout.LabelField("Command Strings (Flexible)", EditorStyles.miniLabel);
            EditorGUILayout.HelpBox(
                "Format: CommandName param1 param2\n" +
                "Multiple: Command1 p1; Command2 p2\n" +
                "Example: SetFlag quest_started; GiveItem sword 1",
                UnityEditor.MessageType.Info
            );
            EditorGUILayout.PropertyField(_commandString);

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(10);

        // AI: Notes Section
        EditorGUILayout.LabelField("Developer Notes", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_note);

        serializedObject.ApplyModifiedProperties();
    }
}
