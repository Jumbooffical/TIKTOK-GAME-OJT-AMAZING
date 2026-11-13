using UnityEngine;

public class createinventory : MonoBehaviour
{
    public GameObject obj_slot;

    public float x = 0;
    public float y = 0;

    void Start()
    {
        int columns = 10;
        int rows = 5;

        float offset = 1;

        for (int r = 0; r < rows; r++)
        {
            for (int i = 0; i < columns; i++)
            {
                Instantiate(
                    obj_slot,
                    new Vector3(
                        x + (i * offset),
                        y - (r * offset),
                        0
                    ),
                    Quaternion.identity
                );
            }
        }
    }
}