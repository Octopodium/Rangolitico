using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Sincronizavel))]
public class EditorSincronizavel : Editor {
    Sincronizavel sincronizavel;

    private void OnEnable() {
        sincronizavel = target as Sincronizavel;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        serializedObject.Update();

        if (!sincronizavel.IsPrefab() || sincronizavel.isSingleton) {
            EditorGUILayout.LabelField("ID: " + sincronizavel.identificador);

            if (GUILayout.Button("Regerar ID")) {
                if (sincronizavel.naoUsarIDAuto) {
                    Debug.LogWarning("ID não será regenerado, pois a opção 'não usar ID automático' está ativada.");
                    return;
                }

                sincronizavel.GeraID();
                EditorUtility.SetDirty(target);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
