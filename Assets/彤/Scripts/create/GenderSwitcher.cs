using UnityEngine;

public class GenderSwitcher : MonoBehaviour
{
    [Header("模型引用")]
    public GameObject maleModel;
    public GameObject femaleModel;

    [Header("系統引用")]
    public FaceAutoGenerator faceAutoGenerator;

    [Header("調色盤串接")]
    public SkinColorConnector skinColorConnector;
    public EyeColorConnector eyeColorConnector;
    public ColorPaletteController colorPalette;

    void Start()
    {
        // 如果 DataManager 已經有存性別，優先讀取
        if (CharacterDataManager.Instance != null)
        {
            if (CharacterDataManager.Instance.selectedGender == 0) ShowMale();
            else ShowFemale();
        }
        else
        {
            ShowFemale();
        }
    }

    public void ShowMale()
    {
        if (maleModel == null || femaleModel == null) return;
        maleModel.SetActive(true);
        femaleModel.SetActive(false);
        if (faceAutoGenerator != null) faceAutoGenerator.ChangeTarget(maleModel);

        // 存入 DataManager
        if (CharacterDataManager.Instance != null) CharacterDataManager.Instance.selectedGender = 0;

        UpdateGenderParts(maleModel);
    }

    public void ShowFemale()
    {
        if (maleModel == null || femaleModel == null) return;
        maleModel.SetActive(false);
        femaleModel.SetActive(true);
        if (faceAutoGenerator != null) faceAutoGenerator.ChangeTarget(femaleModel);

        // 存入 DataManager
        if (CharacterDataManager.Instance != null) CharacterDataManager.Instance.selectedGender = 1;

        UpdateGenderParts(femaleModel);
    }

    // --- 新增：供 ChatSceneLoader 呼叫的顏色套用方法 ---
    public void ApplyColors(GameObject targetModel, Color skin, Color eye)
    {
        if (skinColorConnector == null || eyeColorConnector == null) return;

        // 重新掃描當前模型的 Renderer
        UpdateGenderParts(targetModel);

        // 強制指派顏色數值
        skinColorConnector.SetColor(skin);
        eyeColorConnector.SetColor(eye);

        Debug.Log("已成功套用膚色與瞳色");
    }

    public void UpdateGenderParts(GameObject targetModel)
    {
        if (skinColorConnector == null || eyeColorConnector == null) return;

        skinColorConnector.ResetTargets();
        eyeColorConnector.ResetTargets();

        Renderer[] renderers = targetModel.GetComponentsInChildren<Renderer>(true);
        SkinnedMeshRenderer bodySMR = null;

        // 找出身體模型用於骨骼同步
        foreach (var r in renderers)
        {
            if (r.name.Contains("ca01") && r is SkinnedMeshRenderer)
            {
                bodySMR = (SkinnedMeshRenderer)r;
                break;
            }
        }

        foreach (Renderer r in renderers)
        {
            if (!r.gameObject.activeSelf) continue;
            string n = r.name.ToLower();

            // 1. 配件處理：骨骼同步
            if (n.Contains("hair") || n.Contains("cloth"))
            {
                if (n.Contains("cloth") && r is SkinnedMeshRenderer clothSMR && bodySMR != null)
                {
                    clothSMR.bones = bodySMR.bones;
                    clothSMR.rootBone = bodySMR.rootBone;
                }
            }

            // 2. 顏色同步
            skinColorConnector.ClearAndAddTarget(r);
            eyeColorConnector.AddTarget(r);
        }

        if (colorPalette != null) colorPalette.UpdateColor();
    }
}