using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("UI 引用")]
    public GameObject confirmationPopup;

    [Header("系統引用")]
    public GenderSwitcher genderSwitcher;
    public FaceAutoGenerator faceAutoGenerator; // 新增：用來存取臉部數據

    // 1. 按下換裝介面的「進入聊天」按鈕時呼叫
    public void OpenConfirmation()
    {
        if (confirmationPopup != null)
            confirmationPopup.SetActive(true);
    }

    // 2. 按下彈窗中的「否」時呼叫
    public void CloseConfirmation()
    {
        if (confirmationPopup != null)
            confirmationPopup.SetActive(false);
    }

    // 3. 按下彈窗中的「是」時呼叫 (數據存檔與跳轉)
    public void ConfirmAndGoToChat()
    {
        if (CharacterDataManager.Instance != null)
        {
            // A. 紀錄性別 (0: 男, 1: 女)
            CharacterDataManager.Instance.selectedGender = genderSwitcher.maleModel.activeSelf ? 0 : 1;

            // B. 紀錄顏色：直接從 genderSwitcher 的 Connector 中抓取
            if (genderSwitcher.skinColorConnector != null)
                CharacterDataManager.Instance.skinColor = genderSwitcher.skinColorConnector.currentColor;

            if (genderSwitcher.eyeColorConnector != null)
                CharacterDataManager.Instance.eyeColor = genderSwitcher.eyeColorConnector.currentColor;

            // C. 【關鍵】紀錄臉部所有 BlendShape 數值
            if (faceAutoGenerator != null)
            {
                faceAutoGenerator.SaveAllFaceDataToManager();
            }

            Debug.Log("<color=cyan>所有數據已同步至 DataManager，準備跳轉！</color>");
        }

        // D. 跳轉場景
        SceneManager.LoadScene("ChatScene");
    }
}