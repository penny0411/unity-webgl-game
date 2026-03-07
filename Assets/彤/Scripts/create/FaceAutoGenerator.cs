using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class FaceAutoGenerator : MonoBehaviour
{
    [Header("目標設定")]
    public SkinnedMeshRenderer targetMesh;

    [Header("UI 模板與容器")]
    public GameObject sliderPrefab;
    public GameObject categoryBtnPrefab;
    public Transform contentParent;
    public Transform categoryParent;

    [Header("歷史管理")]
    public FaceHistoryManager historyManager;

    private string[] allFeatures = {
        "eyebrowPosX", "eyebrowPosZ", "eyebrowRotate", "eyebrowSizeX",
        "eyePosX", "eyePosZ", "eyeSizeX", "eyeSizeZ", "eyeRotate",
        "eyeballPosX", "eyeballPosZ", "eyeballSize",
        "nosePosY", "nosePosZ", "noseSizeX", "noseSizeZ",
        "mouthPosZ", "mouthSizeX", "mouthSizeZ",
        "earPosY", "earPosZ", "earSize",
        "cheekUpPosX", "cheekUpPosZ", "cheekDownPosX", "cheekDownPosZ",
        "jawPosX", "jawPosZ", "neckPos", "neckSize",
        "shoulderWid", "chestWid", "waistWid"
    };

    private Dictionary<string, List<GameObject>> categoryGroups = new Dictionary<string, List<GameObject>>();
    private List<Button> allCategoryButtons = new List<Button>();
    private List<Slider> allSliders = new List<Slider>();

    public Color selectedColor = new Color(0.2f, 0.6f, 1f);
    public Color normalColor = Color.white;

    void Start()
    {
        GenerateSystem();
    }

    void GenerateSystem()
    {
        // 清除舊有的物件與資料
        foreach (Transform child in contentParent) Destroy(child.gameObject);
        foreach (Transform child in categoryParent) Destroy(child.gameObject);
        allCategoryButtons.Clear();
        allSliders.Clear();
        categoryGroups.Clear();

        // 1. 提取所有分類
        HashSet<string> categories = new HashSet<string>();
        foreach (string f in allFeatures) categories.Add(GetCategoryName(f));

        // 2. 生成分類按鈕
        foreach (string cat in categories)
        {
            GameObject btnObj = Instantiate(categoryBtnPrefab, categoryParent);
            btnObj.name = "Btn_" + cat;

            TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = char.ToUpper(cat[0]) + cat.Substring(1);

            Button b = btnObj.GetComponent<Button>();
            allCategoryButtons.Add(b);

            string capturedCat = cat;
            b.onClick.AddListener(() => {
                ShowCategory(capturedCat);
                UpdateButtonColors(b);
            });
        }

        // 3. 生成所有 Slider
        foreach (string feature in allFeatures)
        {
            string cat = GetCategoryName(feature);
            GameObject newSliderObj = Instantiate(sliderPrefab, contentParent);

            TMP_Text label = newSliderObj.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = feature;

            Slider s = newSliderObj.GetComponent<Slider>();
            s.minValue = -100f; s.maxValue = 100f; s.value = 0f;
            allSliders.Add(s);

            string maxN = feature + "_max";
            string minN = feature + "_min";

            // 綁定混合變形邏輯
            s.onValueChanged.AddListener((val) => {
                UpdateMeshBlendShape(maxN, minN, val);
            });

            // 加入歷史紀錄偵測器
            AddHistoryTrigger(s);

            if (!categoryGroups.ContainsKey(cat)) categoryGroups[cat] = new List<GameObject>();
            categoryGroups[cat].Add(newSliderObj);
        }

        // 預設選中第一個分類
        if (allCategoryButtons.Count > 0)
        {
            ShowCategory(GetCategoryName(allFeatures[0]));
            UpdateButtonColors(allCategoryButtons[0]);
        }
    }

    private void UpdateMeshBlendShape(string maxN, string minN, float val)
    {
        if (targetMesh == null) return;
        int maxIdx = targetMesh.sharedMesh.GetBlendShapeIndex(maxN);
        int minIdx = targetMesh.sharedMesh.GetBlendShapeIndex(minN);

        if (val >= 0)
        {
            if (maxIdx != -1) targetMesh.SetBlendShapeWeight(maxIdx, val);
            if (minIdx != -1) targetMesh.SetBlendShapeWeight(minIdx, 0);
        }
        else
        {
            if (maxIdx != -1) targetMesh.SetBlendShapeWeight(maxIdx, 0);
            if (minIdx != -1) targetMesh.SetBlendShapeWeight(minIdx, -val);
        }
    }

    private void AddHistoryTrigger(Slider s)
    {
        float oldValue = s.value;
        EventTrigger trigger = s.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = s.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { oldValue = s.value; });
        trigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => {
            if (historyManager != null && !Mathf.Approximately(oldValue, s.value))
            {
                historyManager.RecordAction(s, oldValue, s.value);
            }
        });
        trigger.triggers.Add(pointerUp);
    }

    string GetCategoryName(string name)
    {
        if (name.Contains("eyebrow")) return "eyebrow";
        if (name.Contains("eyeball")) return "eyeball";
        if (name.Contains("eye")) return "eye";
        if (name.Contains("nose")) return "nose";
        if (name.Contains("mouth")) return "mouth";
        if (name.Contains("ear")) return "ear";
        if (name.Contains("cheek") || name.Contains("jaw")) return "face";
        return "body";
    }

    public void ShowCategory(string categoryName)
    {
        foreach (var group in categoryGroups)
        {
            foreach (GameObject obj in group.Value)
                obj.SetActive(group.Key == categoryName);
        }
    }

    void UpdateButtonColors(Button selectedBtn)
    {
        foreach (Button btn in allCategoryButtons)
        {
            ColorBlock cb = btn.colors;
            cb.normalColor = (btn == selectedBtn) ? selectedColor : normalColor;
            cb.selectedColor = (btn == selectedBtn) ? selectedColor : normalColor;
            btn.colors = cb;
        }
    }

    // --- 跨場景與外部調用方法 ---

    // 存檔：跳轉場景前呼叫此方法
    public void SaveAllFaceDataToManager()
    {
        if (CharacterDataManager.Instance == null)
        {
            Debug.LogError("找不到 CharacterDataManager Instance，無法存檔！");
            return;
        }

        CharacterDataManager.Instance.faceShapeData.Clear();
        foreach (Slider s in allSliders)
        {
            TMP_Text label = s.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                CharacterDataManager.Instance.faceShapeData[label.text] = s.value;
            }
        }
        Debug.Log("<color=orange>所有臉部數據已成功同步至 DataManager</color>");
    }

    public void SetSliderValueByName(string sliderName, float value)
    {
        bool found = false;
        foreach (Slider s in allSliders)
        {
            TMP_Text label = s.GetComponentInChildren<TMP_Text>();
            if (label != null && label.text == sliderName)
            {
                s.value = value;
                s.onValueChanged.Invoke(value);

                // 注意：在 ChatScene 加載時通常不需要切換分類 UI，所以這裡可以視需求註解掉
                // string category = GetCategoryName(sliderName);
                // ShowCategory(category);

                found = true;
                break;
            }
        }
        if (!found) Debug.LogWarning($"找不到特徵名為 {sliderName} 的 Slider");
    }

    public void ResetAllSliders()
    {
        if (historyManager != null) historyManager.ResetAll(allSliders);
        else foreach (var s in allSliders) s.value = 0f;
    }

    public void ChangeTarget(GameObject newModel)
    {
        if (newModel == null) return;
        targetMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();

        // 切換模型時，將所有 Slider 數值同步套用到新模型上
        foreach (var s in allSliders)
        {
            s.onValueChanged.Invoke(s.value);
        }
    }
}