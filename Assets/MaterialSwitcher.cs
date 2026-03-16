using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class MaterialSwitcher : MonoBehaviour
{
    public Material originalMaterial;
    public Material newMaterial;

    private SkinnedMeshRenderer smr;
    private bool isOriginal = true;

    private void Awake()
    {
        smr = GetComponent<SkinnedMeshRenderer>();
    }

    public void ToggleMaterial()
    {
        isOriginal = !isOriginal;

        // 1. Get a copy of the current array
        Material[] currentMaterials = smr.materials;

        // 2. Modify the copy
        if (currentMaterials.Length > 0)
        {
            currentMaterials[0] = isOriginal ? originalMaterial : newMaterial;

            // 3. Assign the copy back to the renderer to force an update
            smr.materials = currentMaterials;
        }
    }
    public void RestoreMaterial()
    {
        Material[] currentMaterials = smr.materials;
        currentMaterials[0] = originalMaterial;
        smr.materials = currentMaterials;

        // Force the renderer to acknowledge the change
        smr.enabled = false;
        smr.enabled = true;
    }
}
