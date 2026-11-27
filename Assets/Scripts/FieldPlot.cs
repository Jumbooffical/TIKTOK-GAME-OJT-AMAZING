using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldPlot : MonoBehaviour
{
    [Header("Manual GameObjects (existing crops)")]
    public GameObject[] crops;

    [Header("Dynamic Crop Display (auto-generated)")]
    public SpriteRenderer dynamicCropRenderer;
    [Header("Silhouette Reveal")]
    public float revealDelay = 5f;   // use 60 for the final version
    public float fadeTime = 0.4f;
    public bool IsRevealed { get; private set; }
    public System.Action OnRevealed;
    private static Dictionary<string, string> cropSpriteMap = new Dictionary<string, string>()
    {
        { "WhiteFlower", "Nature_Flowers_White_03" },
        { "PurpleFlower", "Nature_Flowers_Purple_03" },
        { "Herb", "Nature_Herbs" },
        { "Carrot", "Crop_Carrot" },
        { "Corn", "Crop_Corn" },
        { "Eggplant", "Crop_Eggplant" },
        { "Pumpkin", "Crop_Pumpkin" },
        { "Radish", "Crop_Radish" },
        { "Strawberry", "Crop_Strawberry" },
        { "Sunflower", "Crop_Sunflower" },
        { "Tomato", "Crop_Tomato" },
        { "Watermelon", "Crop_Watermelon" },
        { "Wheat", "Crop_Wheat" },
        { "PurpleMushroom", "Nature_Mushrooms_Purple_02" },
        { "RedMushroom", "Nature_Mushrooms_Red_02" }
    };

    private void Awake()
    {
        HideAllCrops();
    }

    public void HideAllCrops()
    {
        StopAllCoroutines();
        IsRevealed = false;
        if (dynamicCropRenderer != null) dynamicCropRenderer.color = Color.white;
        if (crops != null)
        {
            foreach (GameObject crop in crops)
            {
                if (crop != null)
                    crop.SetActive(false);
            }
        }

        if (dynamicCropRenderer != null)
        {
            dynamicCropRenderer.gameObject.SetActive(false);
        }
    }

    public void PlantCrop(int cropIndex)
    {
        PlantCrop(cropIndex, null);
    }

    public void PlantCrop(int cropIndex, string cropName)
    {
        HideAllCrops();

        // 1. Try the named sprite from Resources/Crops first
        Sprite targetSprite = null;
        if (!string.IsNullOrEmpty(cropName))
        {
            if (cropSpriteMap.ContainsKey(cropName))
            {
                targetSprite = Resources.Load<Sprite>("Crops/" + cropSpriteMap[cropName]);
            }
            if (targetSprite == null)
            {
                targetSprite = Resources.Load<Sprite>("Crops/" + cropName);
            }
            if (targetSprite == null)
            {
                targetSprite = Resources.Load<Sprite>("Crops/Crop_" + cropName);
            }
        }

        if (targetSprite != null)
        {
            ShowDynamicCrop(targetSprite, cropName);
            return;
        }

        // 2. Fall back to the manual placeholder GameObjects
        if (crops != null && cropIndex >= 0 && cropIndex < crops.Length && crops[cropIndex] != null)
        {
            ShowDynamicCrop(crops[cropIndex].GetComponent<SpriteRenderer>().sprite, crops[cropIndex].name);
            return;
        }

        Debug.LogError("Invalid crop index or sprite missing: " + cropIndex + " (" + cropName + ")");
    }

    void ShowDynamicCrop(Sprite sprite, string cropName)
    {
        if (dynamicCropRenderer == null)
        {
            GameObject cropObj = new GameObject(string.IsNullOrEmpty(cropName) ? "Crop" : cropName);
            cropObj.transform.SetParent(transform, false);
            cropObj.transform.localPosition = Vector3.zero;

            dynamicCropRenderer = cropObj.AddComponent<SpriteRenderer>();
            dynamicCropRenderer.sortingOrder = 1;

            // Tu dong them WindSway de cay lac lu theo gio
            WindSway sway = cropObj.AddComponent<WindSway>();
            sway.swayAngle = 8f;
            sway.swaySpeed = 2f;
            sway.pivotOffsetY = -0.3f;
        }

        dynamicCropRenderer.gameObject.name = string.IsNullOrEmpty(cropName) ? "Crop" : cropName;
        dynamicCropRenderer.sprite = sprite;

        // Can chinh scale dua theo do phan giai cua sprite de bang dung voi cay cu
        if (sprite != null)
        {
            // Base the plant's scale on its sprite size vs the plot's size
            float spritePixelMax = Mathf.Max(sprite.rect.width, sprite.rect.height);
            Sprite fieldPlotSprite = transform.GetComponent<SpriteRenderer>().sprite;
            float fieldPlotSpritePixelMax = Mathf.Max(fieldPlotSprite.rect.width, fieldPlotSprite.rect.height);
            print(fieldPlotSpritePixelMax);
            float calculatedScale = fieldPlotSpritePixelMax / spritePixelMax;
            dynamicCropRenderer.transform.localScale = Vector3.one * calculatedScale;
        }
        else
        {
            dynamicCropRenderer.transform.localScale = Vector3.one * 1.75f;
        }

        dynamicCropRenderer.gameObject.SetActive(true);
        StartCoroutine(RevealRoutine());

        Debug.Log("Displaying crop (Dynamic Sprite): " + (sprite != null ? sprite.name : "NULL for " + cropName));

    }
    IEnumerator RevealRoutine()
    {
        IsRevealed = false;
        dynamicCropRenderer.color = Color.black;

        yield return new WaitForSeconds(revealDelay);

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            dynamicCropRenderer.color = Color.Lerp(Color.black, Color.white, t / fadeTime);
            yield return null;
        }

        dynamicCropRenderer.color = Color.white;
        IsRevealed = true;
        if (OnRevealed != null) OnRevealed();
    }


    public void RemoveCrop()
    {
        HideAllCrops();
    }
}
