using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] Sprite empty;
    [SerializeField] Sprite filled;

    public void SetStep(int step)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
            if (i < step)
            {
                transform.GetChild(i).GetComponent<Image>().sprite = filled;
            }
            else
            {
                transform.GetChild(i).GetComponent<Image>().sprite = empty;
            }
		}
	}
}
