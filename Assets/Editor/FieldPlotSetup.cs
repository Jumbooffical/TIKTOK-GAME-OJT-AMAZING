#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[InitializeOnLoad]
public static class FieldPlotSetup
{
    static FieldPlotSetup()
    {
        EditorApplication.delayCall += () =>
        {
            SetupCrops(false);
        };
    }

    [MenuItem("Tools/Thêm 10 Cây Vào FieldPlot")]
    public static void ManualSetup()
    {
        SetupCrops(true);
    }

    public static void SetupCrops(bool showDialog)
    {
        FieldPlot fieldPlot = Object.FindAnyObjectByType<FieldPlot>();
        if (fieldPlot == null)
        {
            if (showDialog)
                EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy GameObject FieldPlot trong Scene!", "OK");
            return;
        }

        string[] orderedNames = {
            "WhiteFlower",
            "PurpleFlower",
            "Herb",
            "PurpleMushroom",
            "RedMushroom",
            "Carrot",
            "Tomato",
            "Strawberry",
            "Corn",
            "Wheat",
            "Sunflower",
            "Pumpkin",
            "Watermelon",
            "Eggplant",
            "Radish"
        };

        string[] spriteFiles = {
            "Nature_Flowers_White_03",
            "Nature_Flowers_Purple_03",
            "Nature_Herbs",
            "Nature_Mushrooms_Purple_02",
            "Nature_Mushrooms_Red_02",
            "Crop_Carrot",
            "Crop_Tomato",
            "Crop_Strawberry",
            "Crop_Corn",
            "Crop_Wheat",
            "Crop_Sunflower",
            "Crop_Pumpkin",
            "Crop_Watermelon",
            "Crop_Eggplant",
            "Crop_Radish"
        };

        GameObject[] finalCrops = new GameObject[orderedNames.Length];
        bool changed = false;

        for (int i = 0; i < orderedNames.Length; i++)
        {
            string cropName = orderedNames[i];
            string spriteName = spriteFiles[i];

            // Tim child co san trong FieldPlot
            Transform child = fieldPlot.transform.Find(cropName);

            if (child == null)
            {
                // Neu chua co thi tao moi lam child cua FieldPlot
                GameObject newObj = new GameObject(cropName);
                newObj.transform.SetParent(fieldPlot.transform, false);
                newObj.transform.localPosition = Vector3.zero;

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprite/" + spriteName + ".png");
                if (sprite == null)
                {
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Crops/" + cropName + ".png");
                }

                // Tinh scale chuan de bang dung cay cu (~2.24 world units)
                float targetScale = 1.75f;
                if (sprite != null)
                {
                    float spritePixelMax = Mathf.Max(sprite.rect.width, sprite.rect.height);
                    float baseSize = spritePixelMax / sprite.pixelsPerUnit;
                    targetScale = 2.24f / baseSize;
                }
                newObj.transform.localScale = Vector3.one * targetScale;

                SpriteRenderer sr = newObj.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = 1;

                // Gan WindSway de lac lu theo gio
                WindSway sway = newObj.AddComponent<WindSway>();
                sway.swayAngle = 8f;
                sway.swaySpeed = 2f;
                sway.pivotOffsetY = -0.3f;

                newObj.SetActive(false);

                finalCrops[i] = newObj;
                changed = true;
            }
            else
            {
                finalCrops[i] = child.gameObject;
            }
        }

        // Cap nhat mang crops trong FieldPlot
        if (changed || fieldPlot.crops == null || fieldPlot.crops.Length != finalCrops.Length)
        {
            Undo.RecordObject(fieldPlot, "Update FieldPlot Crops");
            fieldPlot.crops = finalCrops;
            EditorUtility.SetDirty(fieldPlot);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(fieldPlot.gameObject.scene);

            Debug.Log("[FieldPlotSetup] Da bo sung va xep thu tu 15 cay vao FieldPlot thanh cong!");

            if (showDialog)
            {
                EditorUtility.DisplayDialog("Thành công", "Đã thêm đầy đủ 15 cây con vào dưới FieldPlot trong Hierarchy!", "OK");
            }
        }
        else if (showDialog)
        {
            EditorUtility.DisplayDialog("Thông báo", "Tất cả 15 cây đã có sẵn trong FieldPlot!", "OK");
        }
    }
}
#endif
