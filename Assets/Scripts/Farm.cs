
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
        Debug.Log("Farm is starting: " + gameObject.name);

        if (fieldPrefab == null)
        {
            Debug.LogError(
                "FIELD PREFAB IS NULL on: " + gameObject.name
            );
            return;
        }

        Debug.Log("Field Prefab assigned: " + fieldPrefab.name);

        plots = new Plant[totalPlots];
        fieldViews = new FieldPlot[totalPlots];

        CreateFields();
    }

   void CreateFields()
   {
        if (fieldPrefab == null)
        {
            Debug.LogError("Cannot create fields: fieldPrefab is missing!");
            return;
        }

        for (int i = 0; i < totalPlots; i++)
        {
            int row = i / columns;
            int column = i % columns;

            Vector3 position = new Vector3(
                column * spacing,
                -row * spacing,
                0
            );

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

    public void AddPlant(int plotIndex, string plantName, int cropIndex)
    {
        if (plotIndex < 0 || plotIndex >= plots.Length)
            return;

        if (plots[plotIndex] != null)
        {
            Debug.Log("This plot is already occupied!");
            return;
        }

        plots[plotIndex] = new Plant(plantName);

        // Display the selected crop
        fieldViews[plotIndex].PlantCrop(cropIndex, plantName);
    
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
