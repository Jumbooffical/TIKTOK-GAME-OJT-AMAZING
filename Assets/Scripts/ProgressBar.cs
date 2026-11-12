using UnityEngine;

public class ProgressBar : MonoBehaviour
{
    public void SetStep(int step)
	{
		for (int i = 0; i < transform.childCount; i++)
		{
            if (i < step)
            {
                transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(0, 1, 0.4f);
            }
            else
            {
                transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.2f, 0.2f);
            }
		}
	}
}
