using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HorPipe))]
public class HorPipeEditor : Editor
{
    private SerializedProperty point1;
    private SerializedProperty point2;

    private void OnEnable()
    {
        point1 = serializedObject.FindProperty("point1");
        point2 = serializedObject.FindProperty("point2");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(point1);
        EditorGUILayout.PropertyField(point2);
        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        HorPipe t = (target as HorPipe);

        EditorGUI.BeginChangeCheck();
        Vector3 pos = Handles.PositionHandle(t.point1, Quaternion.identity);
        Vector3 pos2 = Handles.PositionHandle(t.point2, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Move point");
            t.point1 = pos;
            t.point2 = pos2;
        }
    }
}
