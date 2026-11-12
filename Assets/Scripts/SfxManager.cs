using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{
	public static SfxManager Instance { get; private set; }

	[SerializeField] private AudioClip planting;
	[SerializeField] private AudioClip harvest_all_plants;

	private AudioSource source;

	void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
		source = GetComponent<AudioSource>();
		source.spatialBlend = 0f;
	}

	public void PlayPlanting() => source.PlayOneShot(planting);
	public void PlayHarvestAllPlants() => source.PlayOneShot(harvest_all_plants);
}