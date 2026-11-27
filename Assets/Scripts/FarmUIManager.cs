using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Casual Farm Game UI Manager (English Version):
/// - Left: Harvested Crops Inventory Card with crop icons, English labels, and count badges
/// - Bottom: Juicy 3D-style Action Buttons (Plant / Harvest)
/// - Top: Score Badge with animated punch effect
/// </summary>
public class FarmUIManager : MonoBehaviour
{
    public static FarmUIManager Instance { get; private set; }

    [Header("Core References")]
    public Farm farm;
    public FarmTester farmTester;
    public Canvas canvas;

    [Header("UI Sprites")]
    public Sprite panelCardSprite;
    public Sprite scoreBadgeSprite;
    public Sprite starIconSprite;
    public Sprite slotBgSprite;
    public Sprite countPillSprite;
    public Sprite btnGreenNormal;
    public Sprite btnGreenPressed;
    public Sprite btnOrangeNormal;
    public Sprite btnOrangePressed;

    [Header("Display Options")]
    [Tooltip("Toggle the top score badge display")]
    public bool showTopScoreBadge = false;

    [Header("Camera Framing Settings")]
    public bool autoFrameCamera = true;
    public Vector3 targetCameraPosition = new Vector3(9.5f, -1.8f, -10f);
    public float targetCameraSize = 13.5f;

    [Header("Runtime Created UI")]
    private RectTransform scoreBadgeRect;
    private TMP_Text scoreText;
    private RectTransform inventoryCardRect;
    private TMP_Text totalHarvestedText;
    private Transform slotsContainer;
    private Dictionary<string, CropSlotUI> cropSlotMap = new Dictionary<string, CropSlotUI>();

    private Coroutine punchScoreCoroutine;
    private int lastScore = 0;

    private class CropSlotUI
    {
        public GameObject root;
        public Image iconImage;
        public TMP_Text countText;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoInit()
    {
        if (FindAnyObjectByType<FarmUIManager>() == null)
        {
            GameObject uiObj = new GameObject("FarmUIManager");
            uiObj.AddComponent<FarmUIManager>();
            Debug.Log("[FarmUIManager] Automatically initialized FarmUIManager in Scene!");
        }
    }

    private void Awake()
    {
        foreach (var s in Resources.LoadAll<Sprite>("Crops")) Debug.Log("Found sprite: " + s.name);
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        LoadSprites();
        HideOldDebugUI();
        InitUI();
    }

    private void Start()
    {
        if (farm == null) farm = FindAnyObjectByType<Farm>();
        if (farmTester == null) farmTester = FindAnyObjectByType<FarmTester>();

        EnsureCameraFraming();
        HideOldDebugUI();
        RefreshData();
    }

    private void EnsureCameraFraming()
    {
        if (!autoFrameCamera) return;

        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = targetCameraSize;
            cam.transform.position = targetCameraPosition;
        }
    }

    private Sprite LoadSpriteSafe(string path, Vector4 border = default)
    {
        Sprite sp = Resources.Load<Sprite>(path);
        if (sp == null)
        {
            Texture2D tex = Resources.Load<Texture2D>(path);
            if (tex != null)
            {
                sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
            }
        }
        return sp;
    }

    private void LoadSprites()
    {
        if (panelCardSprite == null) panelCardSprite = LoadSpriteSafe("UI/ui_panel_card", new Vector4(20, 20, 20, 20));
        if (scoreBadgeSprite == null) scoreBadgeSprite = LoadSpriteSafe("UI/ui_score_badge", new Vector4(24, 20, 24, 20));
        if (starIconSprite == null) starIconSprite = LoadSpriteSafe("UI/ui_star_icon");
        if (slotBgSprite == null) slotBgSprite = LoadSpriteSafe("UI/ui_slot_bg", new Vector4(16, 16, 16, 16));
        if (countPillSprite == null) countPillSprite = LoadSpriteSafe("UI/ui_badge_count", new Vector4(14, 10, 14, 10));

        if (btnGreenNormal == null) btnGreenNormal = LoadSpriteSafe("UI/ui_btn_green_normal", new Vector4(24, 20, 24, 20));
        if (btnGreenPressed == null) btnGreenPressed = LoadSpriteSafe("UI/ui_btn_green_pressed", new Vector4(24, 20, 24, 20));
        if (btnOrangeNormal == null) btnOrangeNormal = LoadSpriteSafe("UI/ui_btn_orange_normal", new Vector4(24, 20, 24, 20));
        if (btnOrangePressed == null) btnOrangePressed = LoadSpriteSafe("UI/ui_btn_orange_pressed", new Vector4(24, 20, 24, 20));
    }

    private Canvas FindMainScreenCanvas()
    {
        GameObject namedCanvas = GameObject.Find("Canvas");
        if (namedCanvas != null)
        {
            Canvas c = namedCanvas.GetComponent<Canvas>();
            if (c != null && c.renderMode != RenderMode.WorldSpace)
                return c;
        }

        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (Canvas c in allCanvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.gameObject.name != "ProgressBar")
                return c;
        }

        foreach (Canvas c in allCanvases)
        {
            if (c.renderMode != RenderMode.WorldSpace && c.gameObject.name != "ProgressBar")
                return c;
        }

        GameObject newCanvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas createdCanvas = newCanvasObj.GetComponent<Canvas>();
        createdCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = newCanvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return createdCanvas;
    }

    private void HideOldDebugUI()
    {
        Canvas screenCanvas = FindMainScreenCanvas();
        if (screenCanvas != null)
        {
            Transform scoreboard = screenCanvas.transform.Find("Scoreboard");
            if (scoreboard != null) scoreboard.gameObject.SetActive(false);

            Transform currentScore = screenCanvas.transform.Find("CurrentScore");
            if (currentScore != null) currentScore.gameObject.SetActive(false);
        }

        GameObject sb = GameObject.Find("Scoreboard");
        if (sb != null) sb.SetActive(false);

        GameObject cs = GameObject.Find("CurrentScore");
        if (cs != null) cs.SetActive(false);

        Farm f = FindAnyObjectByType<Farm>();
        if (f != null)
        {
            if (f.statsText != null) f.statsText.gameObject.SetActive(false);
            if (f.currentScoreText != null) f.currentScoreText.gameObject.SetActive(false);
        }
    }

    public void InitUI()
    {
        canvas = FindMainScreenCanvas();
        if (canvas == null) return;

        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = canvas.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        Transform oldRoot = canvasRect.Find("CasualFarmUIRoot");
        if (oldRoot != null)
        {
            Destroy(oldRoot.gameObject);
        }

        GameObject uiRoot = new GameObject("CasualFarmUIRoot", typeof(RectTransform));
        uiRoot.transform.SetParent(canvasRect, false);
        RectTransform rootRect = uiRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        if (showTopScoreBadge)
        {
            BuildTopScoreBadge(rootRect);
        }

        BuildInventoryCard(rootRect);
        BuildBottomActionButtons(rootRect);
    }

    // 1. TOP SCORE BADGE
    private void BuildTopScoreBadge(RectTransform parent)
    {
        GameObject badgeObj = new GameObject("ScoreBadge", typeof(RectTransform), typeof(Image));
        badgeObj.transform.SetParent(parent, false);
        scoreBadgeRect = badgeObj.GetComponent<RectTransform>();
        scoreBadgeRect.anchorMin = new Vector2(0.5f, 1f);
        scoreBadgeRect.anchorMax = new Vector2(0.5f, 1f);
        scoreBadgeRect.pivot = new Vector2(0.5f, 1f);
        scoreBadgeRect.anchoredPosition = new Vector2(0f, -16f);
        scoreBadgeRect.sizeDelta = new Vector2(250f, 85f);

        Image badgeImg = badgeObj.GetComponent<Image>();
        badgeImg.sprite = scoreBadgeSprite;
        badgeImg.type = Image.Type.Sliced;

        // Star Icon
        GameObject starObj = new GameObject("StarIcon", typeof(RectTransform), typeof(Image));
        starObj.transform.SetParent(badgeObj.transform, false);
        RectTransform starRect = starObj.GetComponent<RectTransform>();
        starRect.anchorMin = new Vector2(0f, 0.5f);
        starRect.anchorMax = new Vector2(0f, 0.5f);
        starRect.pivot = new Vector2(0.5f, 0.5f);
        starRect.anchoredPosition = new Vector2(38f, 0f);
        starRect.sizeDelta = new Vector2(46f, 46f);

        Image starImg = starObj.GetComponent<Image>();
        starImg.sprite = starIconSprite;

        // Score Text
        GameObject txtObj = new GameObject("ScoreText", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(badgeObj.transform, false);
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = new Vector2(0f, 0f);
        txtRect.anchorMax = new Vector2(1f, 1f);
        txtRect.offsetMin = new Vector2(68f, 6f);
        txtRect.offsetMax = new Vector2(-16f, -6f);

        scoreText = txtObj.GetComponent<TextMeshProUGUI>();
        scoreText.text = "<size=17><color=#603005><b>SCORE</b></color></size>\n<size=34><b>0</b></size>";
        scoreText.alignment = TextAlignmentOptions.Center;
    }

    // 2. LEFT HARVESTED CROPS INVENTORY CARD
    private void BuildInventoryCard(RectTransform parent)
    {
        GameObject cardObj = new GameObject("HarvestedCropsCard", typeof(RectTransform), typeof(Image));
        cardObj.transform.SetParent(parent, false);
        inventoryCardRect = cardObj.GetComponent<RectTransform>();
        inventoryCardRect.anchorMin = new Vector2(0f, 0.5f);
        inventoryCardRect.anchorMax = new Vector2(0f, 0.5f);
        inventoryCardRect.pivot = new Vector2(0f, 0.5f);
        inventoryCardRect.anchoredPosition = new Vector2(18f, 25f);
        inventoryCardRect.sizeDelta = new Vector2(215f, 580f);

        Image cardImg = cardObj.GetComponent<Image>();
        cardImg.sprite = panelCardSprite;
        cardImg.type = Image.Type.Sliced;

        // Header Title (in English)
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(cardObj.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -12f);
        titleRect.sizeDelta = new Vector2(195f, 28f);

        TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        titleTxt.text = "<b>HARVESTED CROPS</b>";
        titleTxt.fontSize = 18f;
        titleTxt.color = new Color(1f, 0.94f, 0.65f);
        titleTxt.alignment = TextAlignmentOptions.Center;

        // Subtitle Total Harvested (in English)
        GameObject totalObj = new GameObject("TotalHarvestedText", typeof(RectTransform), typeof(TextMeshProUGUI));
        totalObj.transform.SetParent(cardObj.transform, false);
        RectTransform totalRect = totalObj.GetComponent<RectTransform>();
        totalRect.anchorMin = new Vector2(0f, 1f);
        totalRect.anchorMax = new Vector2(1f, 1f);
        totalRect.pivot = new Vector2(0.5f, 1f);
        totalRect.anchoredPosition = new Vector2(0f, -40f);
        totalRect.sizeDelta = new Vector2(195f, 22f);

        totalHarvestedText = totalObj.GetComponent<TextMeshProUGUI>();
        totalHarvestedText.text = "Total Harvested: <b>0</b>";
        totalHarvestedText.fontSize = 13f;
        totalHarvestedText.color = new Color(0.85f, 0.95f, 0.85f);
        totalHarvestedText.alignment = TextAlignmentOptions.Center;

        // Scroll View (holds all crop rows)
        GameObject scrollObj = new GameObject("CropScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollObj.transform.SetParent(cardObj.transform, false);
        RectTransform scrollRT = scrollObj.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0f, 0f);
        scrollRT.anchorMax = new Vector2(1f, 1f);
        scrollRT.offsetMin = new Vector2(12f, 16f);
        scrollRT.offsetMax = new Vector2(-12f, -68f);

        // Viewport (clips the rows; leaves 10px on the right for the scrollbar)
        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRT = viewportObj.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = new Vector2(-10f, 0f);
        viewportObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f); // invisible, but lets drag/wheel work

        // Content (the rows go here)
        GameObject contentObj = new GameObject("SlotsContainer", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRT = contentObj.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8f;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentObj.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Scrollbar
        GameObject barObj = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        barObj.transform.SetParent(scrollObj.transform, false);
        RectTransform barRT = barObj.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(1f, 0f);
        barRT.anchorMax = new Vector2(1f, 1f);
        barRT.pivot = new Vector2(1f, 1f);
        barRT.anchoredPosition = Vector2.zero;
        barRT.sizeDelta = new Vector2(8f, 0f);
        barObj.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.35f);

        GameObject slidingObj = new GameObject("SlidingArea", typeof(RectTransform));
        slidingObj.transform.SetParent(barObj.transform, false);
        RectTransform slidingRT = slidingObj.GetComponent<RectTransform>();
        slidingRT.anchorMin = Vector2.zero;
        slidingRT.anchorMax = Vector2.one;
        slidingRT.offsetMin = Vector2.zero;
        slidingRT.offsetMax = Vector2.zero;

        GameObject handleObj = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleObj.transform.SetParent(slidingObj.transform, false);
        RectTransform handleRT = handleObj.GetComponent<RectTransform>();
        handleRT.anchorMin = Vector2.zero;
        handleRT.anchorMax = Vector2.one;
        handleRT.offsetMin = Vector2.zero;
        handleRT.offsetMax = Vector2.zero;
        Image handleImg = handleObj.GetComponent<Image>();
        handleImg.color = new Color(1f, 0.94f, 0.65f, 0.9f);

        Scrollbar scrollbar = barObj.GetComponent<Scrollbar>();
        scrollbar.handleRect = handleRT;
        scrollbar.targetGraphic = handleImg;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;

        // Wire it all together
        ScrollRect sr = scrollObj.GetComponent<ScrollRect>();
        sr.viewport = viewportRT;
        sr.content = contentRT;
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;
        sr.scrollSensitivity = 25f;
        sr.verticalScrollbar = scrollbar;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;

        slotsContainer = contentObj.transform;
    }

    // 3. BOTTOM ACTION BUTTONS
    private void BuildBottomActionButtons(RectTransform parent)
    {
        GameObject buttonsObj = new GameObject("ActionButtonsContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonsObj.transform.SetParent(parent, false);
        RectTransform btnContainerRect = buttonsObj.GetComponent<RectTransform>();
        btnContainerRect.anchorMin = new Vector2(0.5f, 0f);
        btnContainerRect.anchorMax = new Vector2(0.5f, 0f);
        btnContainerRect.pivot = new Vector2(0.5f, 0f);
        btnContainerRect.anchoredPosition = new Vector2(0f, 25f);
        btnContainerRect.sizeDelta = new Vector2(620f, 85f);

        HorizontalLayoutGroup hlg = buttonsObj.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 30f;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // Button 1: Plant (Green)
        CreateJuicyButton(
            buttonsObj.transform,
            "BtnPlant",
            "Plant Crop (Space)",
            btnGreenNormal,
            btnGreenPressed,
            new Color(1f, 1f, 1f),
            () =>
            {
                if (farmTester != null)
                {
                    farmTester.PlantInNextEmptyPlot();
                    if (SfxManager.Instance != null) SfxManager.Instance.PlayPlanting();
                }
            }
        );

        // Button 2: Harvest (Orange)
        CreateJuicyButton(
            buttonsObj.transform,
            "BtnHarvest",
            "Harvest All (R)",
            btnOrangeNormal,
            btnOrangePressed,
            new Color(1f, 1f, 1f),
            () =>
            {
                if (farmTester != null)
                {
                    farmTester.HarvestAll();
                }
            }
        );
    }

    private void CreateJuicyButton(Transform parent, string name, string label, Sprite normal, Sprite pressed, Color textColor, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(270f, 78f);

        Image img = btnObj.GetComponent<Image>();
        img.sprite = normal;
        img.type = Image.Type.Sliced;

        Button btn = btnObj.GetComponent<Button>();
        btn.transition = Selectable.Transition.SpriteSwap;
        SpriteState ss = new SpriteState();
        ss.pressedSprite = pressed;
        ss.highlightedSprite = normal;
        btn.spriteState = ss;
        btn.onClick.AddListener(action);

        GameObject txtObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(10f, 12f);
        txtRect.offsetMax = new Vector2(-10f, -4f);

        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        tmp.text = $"<b>{label}</b>";
        tmp.fontSize = 22f;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    public void UpdateUI(int totalScore, int totalHarvested, int[] harvestedCounts, string[] cropNames)
    {
        if (scoreText != null)
        {
            scoreText.text = $"<size=17><color=#603005><b>SCORE</b></color></size>\n<size=34><b>{totalScore:N0}</b></size>";
            if (totalScore > lastScore)
            {
                PunchScoreBadge();
            }
            lastScore = totalScore;
        }

        if (totalHarvestedText != null)
        {
            totalHarvestedText.text = $"Total Harvested: <b>{totalHarvested}</b>";
        }

        if (slotsContainer != null && cropNames != null && harvestedCounts != null)
        {
            for (int i = 0; i < cropNames.Length; i++)
            {
                string cropName = cropNames[i];
                int count = (i < harvestedCounts.Length) ? harvestedCounts[i] : 0;

                CropSlotUI slot;
                if (!cropSlotMap.TryGetValue(cropName, out slot))
                {
                    slot = CreateCropSlot(cropName);
                    cropSlotMap[cropName] = slot;
                }

                if (slot != null && slot.countText != null)
                {
                    if (slot.iconImage.sprite == null) slot.iconImage.sprite = LoadCropIcon(cropName);
                    slot.countText.text = $"x{count}";
                    slot.iconImage.color = (count > 0) ? Color.white : new Color(0.6f, 0.6f, 0.6f, 0.45f);
                }
            }
        }
    }

    private string GetCropDisplayName(string cropName)
    {
        switch (cropName)
        {
            case "WhiteFlower": return "White Flower";
            case "PurpleFlower": return "Purple Flower";
            case "Herb": return "Herb";
            case "PurpleMushroom": return "Purple Mushroom";
            case "RedMushroom": return "Red Mushroom";
            case "Carrot": return "Carrot";
            case "Tomato": return "Tomato";
            case "Strawberry": return "Strawberry";
            case "Corn": return "Corn";
            case "Wheat": return "Wheat";
            case "Sunflower": return "Sunflower";
            case "Pumpkin": return "Pumpkin";
            case "Watermelon": return "Watermelon";
            case "Eggplant": return "Eggplant";
            case "Radish": return "Radish";
            default: return cropName;
        }
    }

    private CropSlotUI CreateCropSlot(string cropName)
    {
        if (slotsContainer == null) return null;

        GameObject slotObj = new GameObject($"Slot_{cropName}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        slotObj.transform.SetParent(slotsContainer, false);
        RectTransform rt = slotObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(188f, 76f);

        LayoutElement le = slotObj.GetComponent<LayoutElement>();
        le.preferredHeight = 76f;
        le.minHeight = 76f;

        Image slotBg = slotObj.GetComponent<Image>();
        slotBg.sprite = slotBgSprite;
        slotBg.type = Image.Type.Sliced;

        // Crop Icon (52x52 on the left)
        GameObject iconObj = new GameObject("CropIcon", typeof(RectTransform), typeof(Image));
        iconObj.transform.SetParent(slotObj.transform, false);
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(34f, 0f);
        iconRect.sizeDelta = new Vector2(52f, 52f);

        Image iconImg = iconObj.GetComponent<Image>();
        iconImg.sprite = LoadCropIcon(cropName);
        Debug.Log("[FarmUIManager] Slot " + cropName + " sprite = " + (iconImg.sprite != null ? iconImg.sprite.name : "NULL"));
        iconImg.preserveAspect = true;

        // Crop Name Label (in English)
        GameObject nameObj = new GameObject("NameText", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameObj.transform.SetParent(slotObj.transform, false);
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0f, 0.5f);
        nameRect.anchorMax = new Vector2(1f, 1f);
        nameRect.pivot = new Vector2(0f, 0.5f);
        nameRect.offsetMin = new Vector2(66f, 2f);
        nameRect.offsetMax = new Vector2(-8f, -6f);

        TextMeshProUGUI nameTxt = nameObj.GetComponent<TextMeshProUGUI>();
        nameTxt.text = $"<b>{GetCropDisplayName(cropName)}</b>";
        nameTxt.fontSize = 15f;
        nameTxt.color = new Color(0.96f, 0.94f, 0.88f);
        nameTxt.alignment = TextAlignmentOptions.Left;

        // Count Badge Pill
        GameObject pillObj = new GameObject("CountBadge", typeof(RectTransform), typeof(Image));
        pillObj.transform.SetParent(slotObj.transform, false);
        RectTransform pillRect = pillObj.GetComponent<RectTransform>();
        pillRect.anchorMin = new Vector2(0f, 0f);
        pillRect.anchorMax = new Vector2(0f, 0f);
        pillRect.pivot = new Vector2(0f, 0f);
        pillRect.anchoredPosition = new Vector2(66f, 8f);
        pillRect.sizeDelta = new Vector2(82f, 26f);

        Image pillImg = pillObj.GetComponent<Image>();
        pillImg.sprite = countPillSprite;
        pillImg.type = Image.Type.Sliced;

        GameObject countTxtObj = new GameObject("CountText", typeof(RectTransform), typeof(TextMeshProUGUI));
        countTxtObj.transform.SetParent(pillObj.transform, false);
        RectTransform countTxtRect = countTxtObj.GetComponent<RectTransform>();
        countTxtRect.anchorMin = Vector2.zero;
        countTxtRect.anchorMax = Vector2.one;
        countTxtRect.offsetMin = new Vector2(4f, 0f);
        countTxtRect.offsetMax = new Vector2(-4f, 0f);

        TextMeshProUGUI countTxt = countTxtObj.GetComponent<TextMeshProUGUI>();
        countTxt.text = "x0";
        countTxt.fontSize = 16f;
        countTxt.color = Color.white;
        countTxt.alignment = TextAlignmentOptions.Center;

        CropSlotUI slotUI = new CropSlotUI
        {
            root = slotObj,
            iconImage = iconImg,
            countText = countTxt
        };

        return slotUI;
    }

    private Sprite LoadCropIcon(string cropName)
    {
        string file;
        switch (cropName)
        {
            case "WhiteFlower": file = "Nature_Flowers_White_03"; break;
            case "PurpleFlower": file = "Nature_Flowers_Purple_03"; break;
            case "Herb": file = "Nature_Herbs"; break;
            case "PurpleMushroom": file = "Nature_Mushrooms_Purple_02"; break;
            case "RedMushroom": file = "Nature_Mushrooms_Red_02"; break;
            default: file = "Crop_" + cropName; break;
        }

        foreach (Sprite sp in Resources.LoadAll<Sprite>("Crops"))
        {
            if (sp.name == file || sp.name == file + "_0")
                return sp;
        }

        Debug.LogWarning("[FarmUIManager] Icon missing for: " + cropName + " (looked for " + file + ")");
        return null;
    }

    private void PunchScoreBadge()
    {
        if (scoreBadgeRect == null) return;
        if (punchScoreCoroutine != null) StopCoroutine(punchScoreCoroutine);
        punchScoreCoroutine = StartCoroutine(DoPunchScale(scoreBadgeRect, 1.25f, 0.22f));
    }

    private IEnumerator DoPunchScale(RectTransform rt, float maxScale, float duration)
    {
        Vector3 orig = Vector3.one;
        Vector3 target = Vector3.one * maxScale;
        float half = duration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            rt.localScale = Vector3.Lerp(orig, target, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            rt.localScale = Vector3.Lerp(target, orig, elapsed / half);
            yield return null;
        }

        rt.localScale = orig;
    }

    public void RefreshData()
    {
        if (farm == null) farm = FindAnyObjectByType<Farm>();
        if (farm != null && farm.cropNames != null && farm.harvestedCropCounts != null)
        {
            UpdateUI(farm.totalScore, farm.totalHarvested, farm.harvestedCropCounts, farm.cropNames);
        }
    }
}
