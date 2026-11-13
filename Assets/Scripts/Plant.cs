[System.Serializable]
public class Plant
{
    public string plantName;
    public int growthStage;
    public int cropIndex;
    public int scoreValue;

    public Plant(string name, int index, int score)
    {
        plantName = name;
        cropIndex = index;
        scoreValue = score;
        growthStage = 0;
    }
}