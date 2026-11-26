using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SeasonManager — cycles Spring → Summer → Autumn → Winter every 20 seconds.
/// Applies a subtle color tint overlay on the background and spawns matching
/// weather particle effects (petals, shimmer, leaves, snow).
/// Works alongside DayNightBackgroundManager (does NOT replace it).
/// </summary>
[DisallowMultipleComponent]
public class SeasonManager : MonoBehaviour
{
    // ─────────────────────────── Enums & Data ───────────────────────────
    public enum Season { Spring, Summer, Autumn, Winter }

    private struct SeasonData
    {
        public string  displayName;
        public Color   tintColor;      // Additive overlay color on background
        public Color   particleColor1; // Primary particle color
        public Color   particleColor2; // Secondary particle color
        public float   particleSize;
        public float   particleSpeed;
        public float   emissionRate;
        public bool    useGravity;     // Falls with gravity (petals/leaves/snow)
        public float   startLifetime;
    }

    // ─────────────────────────── Config ─────────────────────────────────
    public float seasonDuration = 20f;    // Seconds per season
    public float transitionDuration = 2f; // Fade duration when switching season

    // ─────────────────────────── State ──────────────────────────────────
    private Season         currentSeason;
    private float          timer;
    private ParticleSystem activeParticles;
    private SpriteRenderer overlayRenderer;
    private Canvas         hudCanvas;
    private Text           seasonLabel;
    private Coroutine      labelFadeRoutine;
    private Coroutine      tintRoutine;
    private Color          currentTint = new Color(1f, 1f, 1f, 0f);

    private static readonly SeasonData[] seasons = new SeasonData[]
    {
        // Spring — soft pink/green tint, flower petals
        new SeasonData {
            displayName  = "🌸 Spring",
            tintColor    = new Color(1f, 0.92f, 0.95f, 0.18f),
            particleColor1 = new Color(1f, 0.72f, 0.84f, 0.9f),  // pink
            particleColor2 = new Color(1f, 1f,   1f,   0.7f),    // white
            particleSize   = 0.18f,
            particleSpeed  = 0.6f,
            emissionRate   = 30f,
            useGravity     = true,
            startLifetime  = 7f,
        },
        // Summer — warm golden tint, shimmer dust
        new SeasonData {
            displayName  = "☀️ Summer",
            tintColor    = new Color(1f, 0.98f, 0.80f, 0.12f),
            particleColor1 = new Color(1f, 0.95f, 0.5f, 0.6f),   // golden
            particleColor2 = new Color(1f, 1f,   0.8f, 0.3f),    // pale yellow
            particleSize   = 0.08f,
            particleSpeed  = 0.3f,
            emissionRate   = 20f,
            useGravity     = false,
            startLifetime  = 5f,
        },
        // Autumn — orange/warm tint, falling leaves
        new SeasonData {
            displayName  = "🍂 Autumn",
            tintColor    = new Color(1f, 0.85f, 0.60f, 0.20f),
            particleColor1 = new Color(0.90f, 0.45f, 0.10f, 0.9f), // deep orange
            particleColor2 = new Color(0.80f, 0.65f, 0.10f, 0.8f), // golden-brown
            particleSize   = 0.22f,
            particleSpeed  = 0.9f,
            emissionRate   = 25f,
            useGravity     = true,
            startLifetime  = 8f,
        },
        // Winter — cool blue tint, snowflakes
        new SeasonData {
            displayName  = "❄️ Winter",
            tintColor    = new Color(0.82f, 0.90f, 1.00f, 0.22f),
            particleColor1 = new Color(0.90f, 0.95f, 1f, 0.95f),  // ice white
            particleColor2 = new Color(0.75f, 0.85f, 1f, 0.70f),  // pale blue
            particleSize   = 0.14f,
            particleSpeed  = 0.5f,
            emissionRate   = 45f,
            useGravity     = true,
            startLifetime  = 9f,
        },
    };

    // ─────────────────────── Auto Bootstrap ─────────────────────────────
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInitialize()
    {
        if (FindAnyObjectByType<SeasonManager>() == null)
        {
            GameObject go = new GameObject("SeasonManager");
            go.AddComponent<SeasonManager>();
            Debug.Log("[SeasonManager] Auto-spawned.");
        }
    }

    // ─────────────────────────── Unity ──────────────────────────────────
    private void Awake()
    {
        currentSeason = Season.Spring;
        timer         = 0f;
    }

    private void Start()
    {
        CreateOverlay();
        CreateHUD();
        ApplySeason(currentSeason, immediate: true);
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= seasonDuration)
        {
            timer = 0f;
            Season next = (Season)(((int)currentSeason + 1) % 4);
            ApplySeason(next, immediate: false);
        }
    }

    // ─────────────────────── Season Switch ──────────────────────────────
    private void ApplySeason(Season season, bool immediate)
    {
        currentSeason = season;
        SeasonData data = seasons[(int)season];

        // ── Tint overlay ──────────────────────────────────────────────
        if (tintRoutine != null) StopCoroutine(tintRoutine);
        if (immediate)
        {
            if (overlayRenderer != null) overlayRenderer.color = data.tintColor;
            currentTint = data.tintColor;
        }
        else
        {
            tintRoutine = StartCoroutine(FadeTint(data.tintColor, transitionDuration));
        }

        // ── Particles ─────────────────────────────────────────────────
        if (activeParticles != null) Destroy(activeParticles.gameObject);
        activeParticles = CreateParticles(data);

        // ── HUD label ─────────────────────────────────────────────────
        ShowSeasonLabel(data.displayName);

        Debug.Log($"[SeasonManager] Season → {data.displayName}");
    }

    // ─────────────────────── Overlay Sprite ─────────────────────────────
    private void CreateOverlay()
    {
        // Fullscreen color overlay sits just in front of the background
        GameObject obj = new GameObject("SeasonColorOverlay");
        obj.transform.SetParent(transform, false);

        overlayRenderer = obj.AddComponent<SpriteRenderer>();
        overlayRenderer.sortingOrder = -99; // just above background (-100)

        // 1×1 white texture → scale to cover screen
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        overlayRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        overlayRenderer.color  = new Color(1f, 1f, 1f, 0f); // start transparent

        StartCoroutine(ScaleOverlayToCamera(overlayRenderer));
    }

    private IEnumerator ScaleOverlayToCamera(SpriteRenderer sr)
    {
        while (true)
        {
            Camera cam = Camera.main;
            if (cam != null && sr != null)
            {
                float h = cam.orthographicSize * 2f;
                float w = h * cam.aspect;
                Vector3 camPos = cam.transform.position;
                sr.transform.position = new Vector3(camPos.x, camPos.y, 4f);
                sr.transform.localScale = new Vector3(w, h, 1f);
            }
            yield return null;
        }
    }

    private IEnumerator FadeTint(Color target, float duration)
    {
        Color start = currentTint;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentTint = Color.Lerp(start, target, elapsed / duration);
            if (overlayRenderer != null) overlayRenderer.color = currentTint;
            yield return null;
        }
        currentTint = target;
        if (overlayRenderer != null) overlayRenderer.color = target;
    }

    // ─────────────────────── Particle System ─────────────────────────────
    private ParticleSystem CreateParticles(SeasonData data)
    {
        Camera cam = Camera.main;
        if (cam == null) return null;

        float camH  = cam.orthographicSize * 2f;
        float camW  = camH * cam.aspect;
        float spawnY = cam.transform.position.y + cam.orthographicSize + 1f; // just above screen
        float spawnX = cam.transform.position.x;

        GameObject obj = new GameObject("SeasonParticles_" + currentSeason.ToString());
        obj.transform.position = new Vector3(spawnX, spawnY, 1f);

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();

        // ─ Main module ───────────────────────────────────────────────────
        var main              = ps.main;
        main.loop             = true;
        main.startLifetime    = new ParticleSystem.MinMaxCurve(data.startLifetime * 0.8f, data.startLifetime);
        main.startSpeed       = new ParticleSystem.MinMaxCurve(data.particleSpeed * 0.6f, data.particleSpeed);
        main.startSize        = new ParticleSystem.MinMaxCurve(data.particleSize * 0.6f, data.particleSize * 1.4f);
        main.startColor       = new ParticleSystem.MinMaxGradient(data.particleColor1, data.particleColor2);
        main.maxParticles     = 300;
        main.simulationSpace  = ParticleSystemSimulationSpace.World;
        main.gravityModifier  = data.useGravity ? new ParticleSystem.MinMaxCurve(0.05f, 0.12f)
                                                 : new ParticleSystem.MinMaxCurve(0f);
        main.startRotation    = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

        // ─ Emission ───────────────────────────────────────────────────────
        var emission     = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = data.emissionRate;

        // ─ Shape: wide horizontal band above camera ───────────────────────
        var shape        = ps.shape;
        shape.enabled    = true;
        shape.shapeType  = ParticleSystemShapeType.Box;
        shape.scale      = new Vector3(camW * 1.2f, 0.5f, 0.1f);

        // ─ Velocity over lifetime (gentle horizontal drift) ───────────────
        var vel     = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space   = ParticleSystemSimulationSpace.World;

        // Autumn leaves: extra horizontal wobble
        float xRange = (currentSeason == Season.Autumn) ? 0.9f : 0.4f;

        // Summer dust: float upward slightly
        float yMin = 0f;
        float yMax = 0f;
        if (currentSeason == Season.Summer)
        {
            yMin = 0.05f;
            yMax = 0.25f;
        }

        // All three axes must use the same mode, so set every one of them
        vel.x = new ParticleSystem.MinMaxCurve(-xRange, xRange);
        vel.y = new ParticleSystem.MinMaxCurve(yMin, yMax);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        // ─ Rotation over lifetime (spin for leaves/petals) ────────────────
        var rot             = ps.rotationOverLifetime;
        rot.enabled         = true;
        rot.z               = new ParticleSystem.MinMaxCurve(-90f * Mathf.Deg2Rad, 90f * Mathf.Deg2Rad);

        // ─ Size over lifetime (fade out at end) ───────────────────────────
        var sizeOL          = ps.sizeOverLifetime;
        sizeOL.enabled      = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 1f);
        sizeCurve.AddKey(0.8f, 1f);
        sizeCurve.AddKey(1f, 0f);
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ─ Color over lifetime (alpha fade) ───────────────────────────────
        var colorOL         = ps.colorOverLifetime;
        colorOL.enabled     = true;
        Gradient grad       = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]  { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[]  { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(1f, 0.85f), new GradientAlphaKey(0f, 1f) }
        );
        colorOL.color = grad;

        // ─ Renderer ───────────────────────────────────────────────────────
        var renderer         = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 10; // in front of everything

        // Use default sprite material
        Shader spriteShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (spriteShader == null) spriteShader = Shader.Find("Particles/Standard Unlit");
        if (spriteShader == null) spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null) renderer.material = new Material(spriteShader);

        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        ps.Play();
        return ps;
    }

    // ─────────────────────── HUD Label ───────────────────────────────────
    private void CreateHUD()
    {
        // Build a lightweight world-space canvas for the season name
        GameObject canvasGO  = new GameObject("SeasonHUD");
        hudCanvas            = canvasGO.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 50;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();

        // Season label text
        GameObject textGO    = new GameObject("SeasonLabel");
        textGO.transform.SetParent(canvasGO.transform, false);

        seasonLabel          = textGO.AddComponent<Text>();
        seasonLabel.font     = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        seasonLabel.fontSize = 36;
        seasonLabel.fontStyle = FontStyle.Bold;
        seasonLabel.color    = new Color(1f, 1f, 1f, 0f); // start transparent
        seasonLabel.alignment = TextAnchor.MiddleCenter;

        // Add outline
        Outline outline = textGO.AddComponent<Outline>();
        outline.effectColor    = new Color(0f, 0f, 0f, 0.8f);
        outline.effectDistance = new Vector2(2f, -2f);

        // Position: top-center, slight down from top
        RectTransform rt     = textGO.GetComponent<RectTransform>();
        rt.anchorMin         = new Vector2(0.5f, 1f);
        rt.anchorMax         = new Vector2(0.5f, 1f);
        rt.pivot             = new Vector2(0.5f, 1f);
        rt.anchoredPosition  = new Vector2(0f, -60f);
        rt.sizeDelta         = new Vector2(300f, 60f);
    }

    private void ShowSeasonLabel(string name)
    {
        if (seasonLabel == null) return;
        seasonLabel.text = name;
        if (labelFadeRoutine != null) StopCoroutine(labelFadeRoutine);
        labelFadeRoutine = StartCoroutine(LabelFadeRoutine());
    }

    private IEnumerator LabelFadeRoutine()
    {
        // Fade in
        float t = 0f;
        while (t < 0.6f) { t += Time.deltaTime; seasonLabel.color = new Color(1f, 1f, 1f, t / 0.6f); yield return null; }
        seasonLabel.color = Color.white;

        // Hold for 3 seconds
        yield return new WaitForSeconds(3f);

        // Fade out
        t = 0f;
        while (t < 0.6f) { t += Time.deltaTime; seasonLabel.color = new Color(1f, 1f, 1f, 1f - t / 0.6f); yield return null; }
        seasonLabel.color = new Color(1f, 1f, 1f, 0f);
    }

    private void OnDestroy()
    {
        if (activeParticles != null) Destroy(activeParticles.gameObject);
    }
}
