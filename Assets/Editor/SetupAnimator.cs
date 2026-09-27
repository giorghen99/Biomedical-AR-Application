using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class SetupAnimator : MonoBehaviour
{
    [MenuItem("Tools/Setup Heart Animation")]
    static void Setup()
    {
        var selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("Seleziona il modello nella scena prima di eseguire.");
            return;
        }

        string path = "Assets/Animazione.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        // Cerca la clip Take 001 nel modello selezionato
        AnimationClip clip = null;
        var importer = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(selected))) as ModelImporter;
        if (importer != null)
        {
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(importer.assetPath))
            {
                if (o is AnimationClip ac && ac.name == "Take 001")
                    clip = ac;
            }
        }

        if (clip == null)
        {
            Debug.LogError("Clip 'Take 001' non trovata.");
            return;
        }

        var state = controller.layers[0].stateMachine.AddState("Take 001");
        state.motion = clip;

        var animator = selected.GetComponent<Animator>();
        if (animator == null) animator = selected.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        Debug.Log("Animator creato e assegnato con la clip.");
    }
}
