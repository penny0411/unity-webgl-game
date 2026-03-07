using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI 面板清單")]
    public GameObject skinPanel;  // 捏臉面板
    public GameObject colorPanel; // 調色面板
    public GameObject clothPanel; // 換裝面板

    void Start()
    {
        // 初始狀態：只顯示捏臉
        ShowSkinPanel();
    }

    // --- 導覽功能 ---

    // 1. 顯示捏臉 (從調色返回時呼叫)
    public void ShowSkinPanel()
    {
        SetAllPanelsInactive();
        skinPanel.SetActive(true);
    }

    // 2. 顯示調色 (從捏臉下一步，或從換裝上一步時呼叫)
    public void ShowColorPanel()
    {
        SetAllPanelsInactive();
        colorPanel.SetActive(true);
    }

    // 3. 顯示換裝 (從調色下一步時呼叫)
    public void ShowClothPanel()
    {
        SetAllPanelsInactive();
        clothPanel.SetActive(true);
    }

    // 私有輔助函式：先關閉所有面板，避免重疊
    private void SetAllPanelsInactive()
    {
        if (skinPanel != null) skinPanel.SetActive(false);
        if (colorPanel != null) colorPanel.SetActive(false);
        if (clothPanel != null) clothPanel.SetActive(false);
    }
}