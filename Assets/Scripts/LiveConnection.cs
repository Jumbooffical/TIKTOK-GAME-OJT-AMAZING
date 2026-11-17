using System;
using UnityEngine;
using NativeWebSocket;

[Serializable]
public class LiveEvent
{
	public string type;
	public string user;
	public string comment;
	public string gift;
	public int count;
	public int diamonds;
}

public class LiveConnection : MonoBehaviour
{
	[SerializeField] private string url = "ws://localhost:8080";

	public static event Action<LiveEvent> OnEvent;

	private WebSocket socket;

	async void Start()
	{
		socket = new WebSocket(url);

		socket.OnMessage += bytes =>
		{
			string json = System.Text.Encoding.UTF8.GetString(bytes);
			var e = JsonUtility.FromJson<LiveEvent>(json);
			if (e != null) OnEvent?.Invoke(e);
		};

		socket.OnError += err => Debug.LogWarning("Socket error: " + err);

		await socket.Connect();
	}

	void Update()
	{
#if !UNITY_WEBGL || UNITY_EDITOR
		socket?.DispatchMessageQueue(); // delivers messages on the main thread
#endif
	}

	async void OnApplicationQuit()
	{
		if (socket != null) await socket.Close();
	}
}