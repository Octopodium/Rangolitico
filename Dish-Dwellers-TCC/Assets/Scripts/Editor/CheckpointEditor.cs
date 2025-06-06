using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Checkpoint)), CanEditMultipleObjects]
public class CheckpointEditor : Editor {
    #region SerializedProperties

    SerializedProperty spawnPoints;

    #endregion

    Checkpoint checkpoint;
    bool sizeHandle = false;


    private void OnEnable() {
        checkpoint = target as Checkpoint;

        spawnPoints = serializedObject.FindProperty("spawnPoints");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        sizeHandle = EditorGUILayout.Toggle("Enable Size Editing",sizeHandle);

        EditorGUI.BeginChangeCheck();

        Vector3 size = EditorGUILayout.Vector3Field("Checkpoint Size", checkpoint.col.size);

        if (EditorGUI.EndChangeCheck()) {
            Undo.RecordObject(checkpoint.col, "Changed collider size");
            checkpoint.col.size = size;
            checkpoint.col.center = new Vector3(0, size.y / 2, 0);
            EditorUtility.SetDirty(target);
        }

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(spawnPoints);

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI() {
        Handles.color = Color.green;

        if (sizeHandle) {
            EditorGUI.BeginChangeCheck();

            Vector3 size = Handles.ScaleHandle(checkpoint.col.size, checkpoint.transform.position + new Vector3(0, checkpoint.col.size.y / 2, 0), checkpoint.transform.rotation);

            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(checkpoint.col, "Changed collider size");
                checkpoint.col.size = size;
                checkpoint.col.center = new Vector3(0, size.y / 2, 0);
                EditorUtility.SetDirty(target);
            }
        }

        foreach (var point in checkpoint.spawnPoints) {
            Handles.DrawDottedLine(checkpoint.transform.position + checkpoint.col.center, point.position, 7f);
            Handles.DrawWireDisc(point.position, Vector3.up, 0.75f);
            point.position = Handles.DoPositionHandle(point.position, point.rotation);
        }
    }

}
