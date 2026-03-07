using UnityEngine;

public class HistoryPanelController : MonoBehaviour
{
    // 拖入 HistoryPanel 物件
    public GameObject historyPanel;

    // 按下按鈕時呼叫，切換顯示 / 隱藏
    public void ToggleHistory()
    {
        if (historyPanel != null)
        {
            historyPanel.SetActive(!historyPanel.activeSelf);
        }
        else
        {
            Debug.LogError("HistoryPanel 沒有被指定！");
        }
    }
}
