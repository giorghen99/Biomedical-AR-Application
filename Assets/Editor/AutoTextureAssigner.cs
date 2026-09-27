using UnityEngine;
using UnityEditor;
using System.IO;

public class AutoTextureAssigner : EditorWindow
{
    [MenuItem("Tools/Auto Texture Assigner")]
    static void Init()
    {
        AutoTextureAssigner window = (AutoTextureAssigner)EditorWindow.GetWindow(typeof(AutoTextureAssigner));
        window.Show();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Assegna Texture ai Materiali"))
        {
            string materialsPath = "Assets/Model/Materials";
            string texturesPath = "Assets/Textures";

            string[] materialFiles = Directory.GetFiles(materialsPath, "*.mat", SearchOption.AllDirectories);
            string[] textureFiles = Directory.GetFiles(texturesPath, "*.*", SearchOption.AllDirectories);

            foreach (var matFile in materialFiles)
            {
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matFile);
                foreach (var texFile in textureFiles)
                {
                    if (texFile.EndsWith(".png") || texFile.EndsWith(".jpg"))
                    {
                        Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(texFile);
                        if (tex != null && tex.name.ToLower().Contains(mat.name.ToLower()))
                        {
                            mat.mainTexture = tex;
                            Debug.Log($"🟢 Assegnata texture '{tex.name}' al materiale '{mat.name}'");
                            break;
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
