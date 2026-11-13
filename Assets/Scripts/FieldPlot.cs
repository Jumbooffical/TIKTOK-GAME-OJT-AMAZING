using UnityEngine;

public class FieldPlot : MonoBehaviour
{
    [Header("Manual GameObjects (existing crops)")]
    public GameObject[] crops;

    [Header("Dynamic Crop Display (auto-generated)")]
    public SpriteRenderer dynamicCropRenderer;

    [Header("Crop Size Settings")]
    [Tooltip("Kich thuoc dong nhat cua cay (2.24 tuong duong cay cu 32x32 scale 7)")]
    public float targetCropSize = 2.24f;

    private void Awake()
    {
        HideAllCrops();
    }

    public void HideAllCrops()
    {
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

        // 1. Neu co san GameObject trong mang crops (5 loai cay goc)
        if (crops != null && cropIndex >= 0 && cropIndex < crops.Length && crops[cropIndex] != null)
        {
            crops[cropIndex].SetActive(true);
            Debug.Log("Displaying crop (GameObject): " + crops[cropIndex].name);
            return;
        }

        // 2. Tu dong hien thi Sprite tu Resources/Crops
        Sprite targetSprite = null;
        if (!string.IsNullOrEmpty(cropName))
        {
            targetSprite = Resources.Load<Sprite>("Crops/" + cropName);
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
            float spritePixelMax = Mathf.Max(sprite.rect.width, sprite.rect.height);
            float baseSize = spritePixelMax / sprite.pixelsPerUnit;
            float calculatedScale = targetCropSize / baseSize;
            dynamicCropRenderer.transform.localScale = Vector3.one * calculatedScale;
        }
        else
        {
            dynamicCropRenderer.transform.localScale = Vector3.one * 1.75f;
        }

        dynamicCropRenderer.gameObject.SetActive(true);

        Debug.Log("Displaying crop (Dynamic Sprite): " + sprite.name);
    }

    public void RemoveCrop()
    {
        HideAllCrops();
    }
}
