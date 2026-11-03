using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillPassiveSO))]
public class PassiveSkillDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Update representasi serialized object
        serializedObject.Update();

        // 1. TAMPILKAN INFO UTAMA
        EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillId"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("displayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("tier"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillType"));

        EditorGUILayout.Space(10);

        // 2. TAMPILKAN KHUSUS LOGIKA PASIF
        EditorGUILayout.LabelField("Passive Configurations", EditorStyles.boldLabel);
        SerializedProperty triggerTypeProp = serializedObject.FindProperty("triggerType");
        EditorGUILayout.PropertyField(triggerTypeProp);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("triggerChance"));

        // Kondisional: Tampilkan Duration & Effect Value hanya jika butuh (misal Heal Over Time)
        PassiveTriggerType triggerType = (PassiveTriggerType)triggerTypeProp.enumValueIndex;
        if (triggerType == PassiveTriggerType.OnKillEnemy)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("duration"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("effectValue"));
        }

        EditorGUILayout.Space(10);

        // 3. TAMPILKAN VISUAL UI
        EditorGUILayout.LabelField("UI & Visual", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("locked"));

        // Terapkan perubahan ke asset
        serializedObject.ApplyModifiedProperties();
    }
}