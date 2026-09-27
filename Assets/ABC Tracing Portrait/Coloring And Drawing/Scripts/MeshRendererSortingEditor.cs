#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshRenderer))]
public class MeshRendererSortingEditor : Editor
{
    private SerializedProperty sortingLayerProperty;
    private SerializedProperty sortingOrderProperty;

    private void OnEnable()
    {
        sortingLayerProperty = serializedObject.FindProperty("m_SortingLayerID");
        sortingOrderProperty = serializedObject.FindProperty("m_SortingOrder");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        // Display sorting layer dropdown
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(sortingLayerProperty);
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            SceneView.RepaintAll();
        }

        // Display sorting order field
        EditorGUILayout.PropertyField(sortingOrderProperty);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif       