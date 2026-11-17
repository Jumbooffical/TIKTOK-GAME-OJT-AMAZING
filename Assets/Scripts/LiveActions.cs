using UnityEngine;

public class LiveActions : MonoBehaviour
{
    [SerializeField] private FarmTester farmTester;

	[SerializeField] private float likesPerBoost = 50f;

	private int likeCounter;

	void OnEnable()  { LiveConnection.OnEvent += Handle; }
	void OnDisable() { LiveConnection.OnEvent -= Handle; }

	void Handle(LiveEvent e)
	{
		switch (e.type)
		{
			case "chat":   HandleChat(e); break;
			case "gift":   HandleGift(e); break;
			case "like":   HandleLike(e); break;
			case "follow": HandleFollow(e); break;
		}
	}

	void HandleChat(LiveEvent e)
	{
		string text = e.comment.Trim().ToLowerInvariant();

		switch (text)
		{
			case "plant":
                Debug.Log("Planted plant!");
                farmTester.PlantInNextEmptyPlot();
				SfxManager.Instance.PlayPlanting();
                break;
		}
	}

	void HandleGift(LiveEvent e)
	{
		// by gift name; names depend on the language and can change
		switch (e.gift)
		{
			case "Rose":     Debug.Log("Rose gift"); break;
			case "TikTok":   Debug.Log("TikTok gift"); break;
			case "Universe": Debug.Log("Universe gift"); break;
			default:
				// or fall back to value: bigger gifts, bigger effect
				Debug.Log("Gift worth " + (e.diamonds * e.count) + " diamonds");
				break;
		}
	}

	void HandleLike(LiveEvent e)
	{
		// likes arrive in bursts, so accumulate and trigger per threshold
		likeCounter += Mathf.Max(1, e.count);

		while (likeCounter >= likesPerBoost)
		{
			likeCounter -= (int)likesPerBoost;
			Debug.Log("Like meter full: power-up");
		}
	}

	void HandleFollow(LiveEvent e)
	{
		Debug.Log("New follower: " + e.user);
	}
}