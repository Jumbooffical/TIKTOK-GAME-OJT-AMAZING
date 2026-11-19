using UnityEngine;
using UnityEngine.InputSystem;

public class FarmTester : MonoBehaviour
{
    public Farm farm;

    string[] commonCrops =
    {
        "WhiteFlower",
        "PurpleFlower",
        "Herb"
    };

    string[] rareCrops =
    {
        "PurpleMushroom",
        "RedMushroom"
    };

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
        if (farm == null || Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            PlantInNextEmptyPlot();

            SfxManager.Instance.PlayPlanting();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            farm.HarvestAllPlants();
            farm.StartNewSeason();

            SfxManager.Instance.PlayHarvestAllPlants();
        }
    }

    public void PlantInNextEmptyPlot()
    {
        for (int i = 0; i < farm.plots.Length; i++)
        {
            if (farm.plots[i] == null)
            {
                int cropIndex = GetRandomCrop();
                string cropName = GetCropName(cropIndex);

                farm.AddPlant(i, cropName, cropIndex);

                return;
            }
        }

        Debug.Log("Your farm is full!");
    }

    int GetRandomCrop()
    {
        if (Random.Range(0f, 100f) < rareChance)
        {
            return Random.Range(0, rareCrops.Length)
                + commonCrops.Length;
        }

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