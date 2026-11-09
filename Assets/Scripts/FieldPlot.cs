using UnityEngine;

public class FieldPlot : MonoBehaviour
{
    public GameObject crop;

    private void Awake()
    {
        if (crop != null)
            crop.SetActive(false);
    }

    public void PlantCarrot()
    {
        if (crop != null)
            crop.SetActive(true);
    }

    public void RemoveCrop()
    {
        if (crop != null)
            crop.SetActive(false);
    }
}