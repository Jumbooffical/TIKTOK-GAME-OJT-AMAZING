
using UnityEngine;

public class FieldPlot : MonoBehaviour
{
    public GameObject[] crops;

    private void Awake()
    {
        HideAllCrops();
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
