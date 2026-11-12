using UnityEngine;

public class FieldPlot : MonoBehaviour
{
    public GameObject[] crops;

    [Header("Optional Override")]
    [Tooltip("Leave empty to automatically load M_PlantWind from Resources or Shaders")]
    public Material customWindMaterial;

    private static Material s_sharedWindMaterial;

    private void Awake()
    {
        ApplyWindMaterialToCrops();
        HideAllCrops();
    }

    void ApplyWindMaterialToCrops()
    {
        Material matToUse = customWindMaterial;

        if (matToUse == null)
        {
            if (s_sharedWindMaterial == null)
            {
                // 1. Try loading pre-configured material from Resources
                s_sharedWindMaterial = Resources.Load<Material>("M_PlantWind");

                // 2. Fallback: dynamically create material from shader if material asset not loaded
                if (s_sharedWindMaterial == null)
                {
                    Shader windShader = Shader.Find("Custom/PlantWindSway");
                    if (windShader != null)
                    {
                        s_sharedWindMaterial = new Material(windShader);
                    }
                    else
                    {
                        Debug.LogWarning("[FieldPlot] Could not find 'Custom/PlantWindSway' shader or 'M_PlantWind' material in Resources!");
                    }
                }
            }
            matToUse = s_sharedWindMaterial;
        }

        if (matToUse != null && crops != null)
        {
            foreach (GameObject crop in crops)
            {
                if (crop != null)
                {
                    // Find all SpriteRenderers on the crop (including children and inactive ones)
                    SpriteRenderer[] renderers = crop.GetComponentsInChildren<SpriteRenderer>(true);
                    foreach (SpriteRenderer sr in renderers)
                    {
                        sr.sharedMaterial = matToUse;
                    }
                }
            }
        }
    }

    void HideAllCrops()
    {
        foreach (GameObject crop in crops)
        {
            if (crop != null)
                crop.SetActive(false);
        }
    }

    public void PlantCrop(int cropIndex)
    {
        HideAllCrops();

        if (cropIndex >= 0 && cropIndex < crops.Length)
        {
            crops[cropIndex].SetActive(true);

            Debug.Log("Displaying crop: " + crops[cropIndex].name);
        }
        else
        {
            Debug.LogError("Invalid crop index: " + cropIndex);
        }
    }

    public void RemoveCrop()
    {
        HideAllCrops();
    }
}
