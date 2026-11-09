
[System.Serializable]
public class Plant
{
    public string plantName;
    public int growthStage;

    // Constructor
    public Plant(string name)
    {
        plantName = name;
        growthStage = 0;
    }
}
