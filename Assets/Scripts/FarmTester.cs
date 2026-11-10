
using UnityEngine;
using UnityEngine.InputSystem;

public class FarmTester : MonoBehaviour
{
    public Farm farm;

    // Common crops
    string[] commonCrops =
    {
        "WhiteFlower",
        "PurpleFlower",
        "Herb"
    };

    // Rare crops
    string[] rareCrops =
    {
        "PurpleMushroom",
        "RedMushroom",
    };

    // 10% chance to enter the rare pool
    const float rareChance = 10f;

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

            PlantInNextEmptyPlot();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("R PRESSED!");
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
        // Roll for rare crop
        if (Random.Range(0f, 100f) < rareChance)
        {
            // Choose a random rare crop
            return Random.Range(0, rareCrops.Length)
                + commonCrops.Length;
        }

        // Choose a random common crop
        return Random.Range(0, commonCrops.Length);
    }

    string GetCropName(int cropIndex)
    {
        if (cropIndex < commonCrops.Length)
        {
            return commonCrops[cropIndex];
        }

        return rareCrops[cropIndex - commonCrops.Length];
    }
}
