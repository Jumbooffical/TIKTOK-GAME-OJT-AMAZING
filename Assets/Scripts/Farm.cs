using UnityEngine;
using TMPro;

public class Farm : MonoBehaviour
{
    public Plant[] plots;
    public FieldPlot[] fieldViews;

    [Header("Field Settings")]
    public FieldPlot fieldPrefab;
    public int totalPlots = 50;
    public int columns = 10;
    public float spacing = 1.2f;

    [Header("Crop Scores")]
    public int[] cropScores = { 10, 20, 15, 30, 50 };

    [Header("Scoreboard")]
    public TMP_Text statsText;
    public TMP_Text currentScoreText;

    public int totalHarvested = 0;
    public int totalScore = 0;

    private int[] harvestedCropCounts;

    private string[] cropNames =
    {
        "WhiteFlower",
        "PurpleFlower",
        "Herb",
        "PurpleMushroom",
        "RedMushroom"
    };

    private void Awake()
    {
        if (fieldPrefab == null)
        {
            Debug.LogError("Field Prefab is missing!");
            return;
        }

        plots = new Plant[totalPlots];
        fieldViews = new FieldPlot[totalPlots];

        harvestedCropCounts = new int[cropScores.Length];

        CreateFields();
        UpdateStatsUI();
    }

    void CreateFields()
    {
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

    int CalculateCurrentScore()
    {
        int score = 0;

        foreach (Plant plant in plots)
        {
            if (plant != null)
            {
                score += plant.scoreValue;
            }
        }

        return score;
    }

    public void AddPlant(int plotIndex, string plantName, int cropIndex)
    {
        if (plotIndex < 0 || plotIndex >= plots.Length)
            return;

        if (cropIndex < 0 || cropIndex >= cropScores.Length)
        {
            Debug.LogError("Invalid crop index: " + cropIndex);
            return;
        }

        if (plots[plotIndex] != null)
        {
            Debug.Log("This plot is already occupied!");
            return;
        }

        plots[plotIndex] = new Plant(
            plantName,
            cropIndex,
            cropScores[cropIndex]
        );

        fieldViews[plotIndex].PlantCrop(cropIndex);

        Debug.Log("Planted " + plantName + " in plot " + plotIndex);

        UpdateStatsUI();
    }

    public void HarvestAllPlants()
    {
        int harvestedThisRound = 0;
        int scoreThisRound = 0;

        for (int i = 0; i < plots.Length; i++)
        {
            Plant plant = plots[i];

            if (plant != null)
            {
                harvestedThisRound++;

                totalHarvested++;
                totalScore += plant.scoreValue;

                scoreThisRound += plant.scoreValue;

                harvestedCropCounts[plant.cropIndex]++;

                Debug.Log(
                    "Harvested " + plant.plantName +
                    " | Earned " + plant.scoreValue + " points"
                );

                plots[i] = null;
                fieldViews[i].RemoveCrop();
            }
        }

        Debug.Log(
            "Harvest complete! Crops: " + harvestedThisRound +
            " | Score earned: " + scoreThisRound
        );

        UpdateStatsUI();
    }

    void UpdateStatsUI()
    {
        // Update harvested statistics
        if (statsText != null)
        {
            string display = "HARVEST STATISTICS\n\n";

            for (int i = 0; i < cropNames.Length; i++)
            {
                display += cropNames[i] + ": ";

                if (i < harvestedCropCounts.Length)
                    display += harvestedCropCounts[i];
                else
                    display += "0";

                display += "\n";
            }

            display += "\n--------------------\n";
            display += "Total Harvested: " + totalHarvested + "\n";
            display += "Total Score: " + totalScore;

            statsText.text = display;
        }

        // Update current score
        if (currentScoreText != null)
        {
            currentScoreText.text =
                "CURRENT SCORE: " + CalculateCurrentScore();
        }
    }
}