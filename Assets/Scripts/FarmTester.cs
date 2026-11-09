
using UnityEngine;
using UnityEngine.InputSystem;

public class FarmTester : MonoBehaviour
{
    public Farm farm;

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
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            farm.RemoveAllPlants();
        }
    }

    void PlantInNextEmptyPlot()
    {
        for (int i = 0; i < farm.plots.Length; i++)
        {
            if (farm.plots[i] == null)
            {
                farm.AddPlant(i, "Carrot");
                return;
            }
        }

        Debug.Log("Your farm is full!");
    }
}
