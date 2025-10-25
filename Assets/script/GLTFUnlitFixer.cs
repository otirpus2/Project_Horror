using UnityEngine;
using UnityEditor;

public class GLTFUnlitFixer : AssetPostprocessor
{
    void OnPostprocessModel(GameObject g)
    {
        if (assetPath.EndsWith(".glb"))
        {
            foreach (var renderer in g.GetComponentsInChildren<Renderer>())
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null && mat.shader.name.ToLower().Contains("unlit"))
                    {
                        mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                    }
                }
            }
            Debug.Log($"✅ Converted Unlit materials to URP Lit for: {assetPath}");
        }
    }
}
