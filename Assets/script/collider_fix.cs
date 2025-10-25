using UnityEngine;

public class AutoMeshCollider : MonoBehaviour
{
    void Start()
    {
        // Add MeshColliders to all meshes in the scene
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.GetComponent<MeshCollider>() == null && mf.sharedMesh != null)
            {
                MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                mc.convex = false; // Set to true only if needed for physics
            }
        }

        Debug.Log("✅ MeshColliders added to all meshes in: " + gameObject.name);
    }
}
