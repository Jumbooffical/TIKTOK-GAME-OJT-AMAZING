using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DayNightBackgroundManager : MonoBehaviour
{
    public enum TimeOfDay
    {
        Morning,    // 06:00 - 11:59:59
        Afternoon,  // 12:00 - 17:59:59
        Night       // 18:00 - 05:59:59
    }

    public enum FitMode
    {
        StretchToScreen,   // Stretches to fill camera: keeps sky/moon at top and grass at bottom
        AspectFill         // Uniform aspect ratio scaling
    }

    [Header("Time Mode")]
    [Tooltip("If checked, uses the system's real-time clock. If unchecked, uses debugTimeOfDay below.")]
    public bool useRealTime = true;

    [Tooltip("Manual override for testing in the Inspector (when useRealTime is unchecked).")]
    public TimeOfDay debugTimeOfDay = TimeOfDay.Morning;

    [Header("Background Sprites (Optional - auto-loads from Resources if empty)")]
    public Sprite morningSprite;
    public Sprite afternoonSprite;
    public Sprite nightSprite;

    [Header("Visual & Camera Fit")]
    [Tooltip("How the background fits the camera view.")]
    public FitMode fitMode = FitMode.StretchToScreen;

    [Tooltip("Slight scale buffer to avoid any black border at screen edges.")]
    [Range(1.0f, 1.2f)]
    public float autoFitMargin = 1.05f;

    [Tooltip("Nudge the background up or down to align the horizon line with your farm.")]
    [Range(-15f, 15f)]
    public float verticalOffset = 0f;

    [Tooltip("Sorting order for background (negative number keeps it behind all game objects).")]
    public int sortingOrder = -100;

    [Header("Transition Settings")]
    [Tooltip("Duration in seconds for the smooth crossfade transition between backgrounds.")]
    [Range(0.1f, 5.0f)]
    public float crossfadeDuration = 1.5f;

    // Dual SpriteRenderers for smooth crossfading
    private SpriteRenderer rendererA;
    private SpriteRenderer rendererB;
    private bool isRendererAActive = true;

    private TimeOfDay currentTimeOfDay;
    private Coroutine transitionCoroutine;
    private Camera targetCamera;
    private Material unlitMaterial;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (FindAnyObjectByType<DayNightBackgroundManager>() == null)
        {
            GameObject bgObj = new GameObject("BackgroundManager");
            bgObj.AddComponent<DayNightBackgroundManager>();
            Debug.Log("[DayNightBackground] Auto-spawned BackgroundManager in scene.");
        }
    }

    private void Awake()
    {
        InitializeCamera();
        InitializeMaterial();
        InitializeSprites();
        InitializeRenderers();

        // Determine starting time
        currentTimeOfDay = CalculateCurrentTimeOfDay();
        Sprite initialSprite = GetSpriteForTime(currentTimeOfDay);

        if (rendererA != null)
        {
            rendererA.sprite = initialSprite;
            rendererA.color = Color.white;
        }

        if (rendererB != null)
        {
            rendererB.color = new Color(1f, 1f, 1f, 0f);
        }

        UpdateScaleAndPosition();

        Debug.Log($"[DayNightBackground] Initialized. Current Time: {System.DateTime.Now:HH:mm:ss} -> TimeOfDay: {currentTimeOfDay}");
    }

    private void Start()
    {
        UpdateScaleAndPosition();
    }

    private void Update()
    {
        TimeOfDay requiredTime = CalculateCurrentTimeOfDay();

        if (requiredTime != currentTimeOfDay)
        {
            SetTimeOfDay(requiredTime);
        }

        UpdateScaleAndPosition();
    }

    public TimeOfDay CalculateCurrentTimeOfDay()
    {
        if (!useRealTime)
        {
            return debugTimeOfDay;
        }

        int hour = System.DateTime.Now.Hour;

        if (hour >= 6 && hour < 12)
        {
            return TimeOfDay.Morning;
        }
        else if (hour >= 12 && hour < 18)
        {
            return TimeOfDay.Afternoon;
        }
        else
        {
            return TimeOfDay.Night;
        }
    }

    public void SetTimeOfDay(TimeOfDay newTimeOfDay, bool immediate = false)
    {
        currentTimeOfDay = newTimeOfDay;
        debugTimeOfDay = newTimeOfDay;

        Sprite targetSprite = GetSpriteForTime(newTimeOfDay);

        if (targetSprite == null)
        {
            Debug.LogWarning($"[DayNightBackground] Missing sprite for {newTimeOfDay}!");
            return;
        }

        if (immediate)
        {
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

            SpriteRenderer active = isRendererAActive ? rendererA : rendererB;
            SpriteRenderer inactive = isRendererAActive ? rendererB : rendererA;

            active.sprite = targetSprite;
            active.color = Color.white;
            inactive.color = new Color(1f, 1f, 1f, 0f);
            return;
        }

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(CrossfadeRoutine(targetSprite));
    }

    private IEnumerator CrossfadeRoutine(Sprite newSprite)
    {
        SpriteRenderer currentRenderer = isRendererAActive ? rendererA : rendererB;
        SpriteRenderer nextRenderer = isRendererAActive ? rendererB : rendererA;

        nextRenderer.sprite = newSprite;
        nextRenderer.color = new Color(1f, 1f, 1f, 0f);

        float elapsed = 0f;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentRenderer.color = new Color(1f, 1f, 1f, 1f - smoothT);
            nextRenderer.color = new Color(1f, 1f, 1f, smoothT);

            yield return null;
        }

        currentRenderer.color = new Color(1f, 1f, 1f, 0f);
        nextRenderer.color = Color.white;

        isRendererAActive = !isRendererAActive;
        transitionCoroutine = null;

        Debug.Log($"[DayNightBackground] Crossfade completed. Active background is now {currentTimeOfDay}.");
    }

    private Sprite GetSpriteForTime(TimeOfDay timeOfDay)
    {
        return timeOfDay switch
        {
            TimeOfDay.Morning => morningSprite,
            TimeOfDay.Afternoon => afternoonSprite,
            TimeOfDay.Night => nightSprite,
            _ => morningSprite
        };
    }

    private void InitializeCamera()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void InitializeMaterial()
    {
        if (unlitMaterial == null || unlitMaterial.shader == null || unlitMaterial.shader.name.Contains("InternalErrorShader"))
        {
            // 1. Try loading dedicated Unlit material from Resources
            unlitMaterial = Resources.Load<Material>("M_BackgroundUnlit");

            // 2. Validate shader and fallback to built-in or URP unlit sprite shader
            if (unlitMaterial == null || unlitMaterial.shader == null || unlitMaterial.shader.name.Contains("InternalErrorShader"))
            {
                Shader unlitShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");

                if (unlitShader != null)
                {
                    unlitMaterial = new Material(unlitShader);
                }
            }
        }
    }

    private void InitializeSprites()
    {
        if (morningSprite == null) morningSprite = LoadSpriteWithFallback("Backgrounds/morning");
        if (afternoonSprite == null) afternoonSprite = LoadSpriteWithFallback("Backgrounds/afternoon");
        if (nightSprite == null) nightSprite = LoadSpriteWithFallback("Backgrounds/night");
    }

    private Sprite LoadSpriteWithFallback(string path)
    {
        // Direct Sprite load
        Sprite s = Resources.Load<Sprite>(path);
        if (s != null) return s;

        // Fallback: Texture2D load and wrap into Sprite
        Texture2D tex = Resources.Load<Texture2D>(path);
        if (tex != null)
        {
            return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        Debug.LogError($"[DayNightBackground] Failed to load background sprite at Resources/{path}");
        return null;
    }

    private void InitializeRenderers()
    {
        Transform childA = transform.Find("BG_LayerA");
        if (childA == null)
        {
            GameObject objA = new GameObject("BG_LayerA");
            objA.transform.SetParent(transform, false);
            childA = objA.transform;
        }
        rendererA = childA.GetComponent<SpriteRenderer>();
        if (rendererA == null) rendererA = childA.gameObject.AddComponent<SpriteRenderer>();
        rendererA.sortingOrder = sortingOrder;
        if (unlitMaterial != null) rendererA.sharedMaterial = unlitMaterial;

        Transform childB = transform.Find("BG_LayerB");
        if (childB == null)
        {
            GameObject objB = new GameObject("BG_LayerB");
            objB.transform.SetParent(transform, false);
            childB = objB.transform;
        }
        rendererB = childB.GetComponent<SpriteRenderer>();
        if (rendererB == null) rendererB = childB.gameObject.AddComponent<SpriteRenderer>();
        rendererB.sortingOrder = sortingOrder;
        if (unlitMaterial != null) rendererB.sharedMaterial = unlitMaterial;
    }

    private void UpdateScaleAndPosition()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        Vector3 camPos = targetCamera.transform.position;
        transform.position = new Vector3(camPos.x, camPos.y + verticalOffset, 5f);

        Sprite currentSprite = GetSpriteForTime(currentTimeOfDay);
        if (currentSprite == null) return;

        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth = camHeight * targetCamera.aspect;

        Vector2 spriteSize = currentSprite.rect.size / currentSprite.pixelsPerUnit;
        if (spriteSize.x <= 0 || spriteSize.y <= 0) return;

        if (fitMode == FitMode.StretchToScreen)
        {
            float scaleX = (camWidth / spriteSize.x) * autoFitMargin;
            float scaleY = (camHeight / spriteSize.y) * autoFitMargin;
            transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
        else // AspectFill
        {
            float scaleX = camWidth / spriteSize.x;
            float scaleY = camHeight / spriteSize.y;
            float uniformScale = Mathf.Max(scaleX, scaleY) * autoFitMargin;
            transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
        }
    }

    private void OnValidate()
    {
        if (unlitMaterial == null) InitializeMaterial();
        if (rendererA != null)
        {
            rendererA.sortingOrder = sortingOrder;
            if (unlitMaterial != null) rendererA.sharedMaterial = unlitMaterial;
        }
        if (rendererB != null)
        {
            rendererB.sortingOrder = sortingOrder;
            if (unlitMaterial != null) rendererB.sharedMaterial = unlitMaterial;
        }
    }
}
