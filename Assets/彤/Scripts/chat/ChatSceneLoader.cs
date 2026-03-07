using UnityEngine;

public class ChatSceneLoader : MonoBehaviour
{
    [Header("場景中的角色管理器")]
    public CharacterEquipmentHandler equipmentHandler;

    [Header("臉部生成器")]
    public FaceAutoGenerator faceGenerator;

    void Start()
    {
        if (CharacterDataManager.Instance != null)
        {
            var data = CharacterDataManager.Instance;

            // 1. 還原性別
            bool isMale = data.selectedGender == 0;
            equipmentHandler.genderSwitcher.maleModel.SetActive(isMale);
            equipmentHandler.genderSwitcher.femaleModel.SetActive(!isMale);

            // 2. 初始化臉部生成器 (必須在還原數據前執行，確保抓到正確的 Mesh)
            GameObject activeModel = isMale ? equipmentHandler.genderSwitcher.maleModel : equipmentHandler.genderSwitcher.femaleModel;
            if (faceGenerator != null)
            {
                faceGenerator.ChangeTarget(activeModel);

                // 3. 還原臉部 BlendShape 數值
                foreach (var record in data.faceShapeData)
                {
                    faceGenerator.SetSliderValueByName(record.Key, record.Value);
                }
            }

            // 4. 還原裝備與髮型
            if (data.outfitIndex != -1) equipmentHandler.SwitchOutfit(data.outfitIndex);
            if (data.hairIndex != -1) equipmentHandler.SwitchHair(data.hairIndex);

            // 5. 還原顏色 (解開註解並執行)
            if (equipmentHandler.genderSwitcher != null)
            {
                equipmentHandler.genderSwitcher.ApplyColors(activeModel, data.skinColor, data.eyeColor);
            }

            Debug.Log("<color=green>ChatScene 角色數據載入完全成功！</color>");
        }
        else
        {
            Debug.LogWarning("找不到 CharacterDataManager，請確保是從捏臉場景進入的。");
        }
    }
}