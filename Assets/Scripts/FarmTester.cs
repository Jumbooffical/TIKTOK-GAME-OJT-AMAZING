using UnityEngine;
using UnityEngine.InputSystem;

public class FarmTester : MonoBehaviour
{
    public Farm farm;

    [Header("Crop List (15 Crops Total)")]
    public string[] allCrops =
    {
        // 5 cay ban dau
        "WhiteFlower",
        "PurpleFlower",
        "Herb",
        "PurpleMushroom",
        "RedMushroom",

        // 10 cay moi bo sung
        "Carrot",
        "Tomato",
        "Strawberry",
        "Corn",
        "Wheat",
        "Sunflower",
        "Pumpkin",
        "Watermelon",
        "Eggplant",
        "Radish"
    };

    private void Awake()
    {
        if (farm == null)
        {
            farm = FindAnyObjectByType<Farm>();
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("SPACE PRESSED!");

            if (farm == null)
            {
                Debug.LogError("Farm reference is missing!");
                return;
            }

            // Play planting sound
            SfxManager.Instance.PlayPlanting();

            PlantInNextEmptyPlot();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("R PRESSED!");

            // Play planting sound
            SfxManager.Instance.PlayHarvestAllPlants();

            farm.RemoveAllPlants();
        }
    }


    void PlantInNextEmptyPlot()
    {
        for (int i = 0; i < farm.plots.Length; i++)
        {
            if (farm.plots[i] == null)
            {
                int cropIndex = GetRandomCrop();
                string cropName = GetCropName(cropIndex);

                farm.AddPlant(i, cropName, cropIndex);

                Debug.Log("You got: " + cropName);

                return;
            }
        }

        Debug.Log("Your farm is full!");
    }

    int GetRandomCrop()
    {
        // Ti le xuat hien bang nhau ngau nhien cho tat ca cac loai cay
        return Random.Range(0, allCrops.Length);
    }

    string GetCropName(int cropIndex)
    {
        if (cropIndex >= 0 && cropIndex < allCrops.Length)
        {
            return allCrops[cropIndex];
        }

        return "UnknownCrop";
    }
}
