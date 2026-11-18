using UnityEngine;

public class FakeEvents : MonoBehaviour
{
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.G)) Fire(new LiveEvent { type = "gift", user = "tester", gift = "Rose", count = 3, diamonds = 1 });
		if (Input.GetKeyDown(KeyCode.L)) Fire(new LiveEvent { type = "like", user = "tester", count = 20 });
		if (Input.GetKeyDown(KeyCode.C)) Fire(new LiveEvent { type = "chat", user = "tester", comment = "!jump" });
	}

	void Fire(LiveEvent e)
	{
		typeof(LiveConnection)
			.GetField("OnEvent", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
			?.GetValue(null);
		// simplest: add a public static Raise(LiveEvent) method to LiveConnection and call it here
	}
}