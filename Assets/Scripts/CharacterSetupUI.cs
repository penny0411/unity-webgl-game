using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class CharacterSetupUI : MonoBehaviour
{
    public TMP_InputField characterNameInputField;
    public TMP_Dropdown genderDropdown;
    public TMP_InputField personalityInputField;
    public Slider ageSlider;
    public TMP_Text ageText;

    public TMP_Text warningText;   //  一定要有這行

    private void Start()
    {
        ageSlider.onValueChanged.AddListener(UpdateAgeText);
        UpdateAgeText(ageSlider.value);

        warningText.gameObject.SetActive(false);  // 一開始隱藏警告
    }

    void UpdateAgeText(float value)
    {
        ageText.text = "年齡: " + ((int)value).ToString();
    }

    public void OnClickConfirm()
    {
        // ====== 防呆檢查 ======

        // 檢查角色名稱
        if (string.IsNullOrWhiteSpace(characterNameInputField.text))
        {
            warningText.text = "設定未輸入完成";
            warningText.gameObject.SetActive(true);
            return;  // ← 這行很重要
        }

        // 檢查性格設定
        if (string.IsNullOrWhiteSpace(personalityInputField.text))
        {
            warningText.text = "設定未輸入完成";
            warningText.gameObject.SetActive(true);
            return;  // ← 這行很重要
        }

        // 檢查 Account
        if (AccountManager.Instance == null ||
            AccountManager.Instance.CurrentAccount == null)
        {
            Debug.LogError("Account not found!");
            return;
        }

        // ====== 如果通過檢查 ======

        warningText.gameObject.SetActive(false);

        AccountManager.Instance.CurrentAccount.characterName =
            characterNameInputField.text;

        AccountManager.Instance.CurrentAccount.gender =
            genderDropdown.options[genderDropdown.value].text;

        AccountManager.Instance.CurrentAccount.age =
            (int)ageSlider.value;

        AccountManager.Instance.CurrentAccount.personality =
            personalityInputField.text;

        AccountManager.Instance.SaveAccount();

        GameDataManager.Instance.characterName = characterNameInputField.text;
        GameDataManager.Instance.characterGender = genderDropdown.options[genderDropdown.value].text;
        GameDataManager.Instance.characterAge = (int)ageSlider.value;
        GameDataManager.Instance.characterPersonality = personalityInputField.text;

        // 只有全部通過才會跳轉
        SceneManager.LoadScene("ChatScene");
    }


}
