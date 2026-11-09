
using UnityEngine;

public class Farm : MonoBehaviour
{
    public Plant[] plots;
    public FieldPlot[] fieldViews;

    [Header("Field Settings")]
    public FieldPlot fieldPrefab;
    public int totalPlots = 50;
    public int columns = 10;
    public float spacing = 20;

    private void Awake()
    {
        plots = new Plant[totalPlots];
        fieldViews = new FieldPlot[totalPlots];

        CreateFields();
    }

    void CreateFields()
    {
        for (int i = 0; i < totalPlots; i++)
        {
            // Calculate position
            int row = i / columns;
            int column = i % columns;

            Vector3 position = new Vector3(
                column * spacing,
                -row * spacing,
                0
            );

            // Create field object
            FieldPlot field = Instantiate(
                fieldPrefab,
                position,
                Quaternion.identity,
                transform
            );

            fieldViews[i] = field;
        }

        Debug.Log("Created " + totalPlots + " fields!");
    }

    public void AddPlant(int plotIndex, string plantName)
    {
        if (plotIndex < 0 || plotIndex >= plots.Length)
            return;

        if (plots[plotIndex] != null)
        {
            Debug.Log("This plot is already occupied!");
            return;
        }

        plots[plotIndex] = new Plant(plantName);

        fieldViews[plotIndex].PlantCarrot();

        Debug.Log("Planted " + plantName + " in plot " + plotIndex);
    }

    public void RemovePlant(int plotIndex)
    {
        if (plotIndex < 0 || plotIndex >= plots.Length)
            return;

        plots[plotIndex] = null;
        fieldViews[plotIndex].RemoveCrop();
    }

    public void RemoveAllPlants()
    {
        for (int i = 0; i < plots.Length; i++)
        {
            plots[i] = null;
            fieldViews[i].RemoveCrop();
        }

        Debug.Log("All plants removed!");
    }
}
