using UnityEngine;
using System;

public class CharacterEquipmentHandler : MonoBehaviour
{
    [Serializable]
    public class Outfit
    {
        public string name;
        public GameObject[] parts;
    }

    [Header("核心引用")]
    public GenderSwitcher genderSwitcher;

    [Header("男裝與髮型")]
    public Outfit[] maleOutfits;
    public GameObject[] maleHairs;

    [Header("女裝與髮型")]
    public Outfit[] femaleOutfits;
    public GameObject[] femaleHairs;

    public void SwitchOutfit(int index)
    {
        HideAllOfCategory("cloth");
        bool isMale = IsMaleActive();
        if (isMale)
        {
            if (index >= 0 && index < maleOutfits.Length)
                ToggleParts(maleOutfits[index].parts, true);
        }
        else
        {
            if (index >= 0 && index < femaleOutfits.Length)
                ToggleParts(femaleOutfits[index].parts, true);
        }

        if (CharacterDataManager.Instance != null)
            CharacterDataManager.Instance.outfitIndex = index;

        RefreshCharacter();
    }

    public void SwitchHair(int index)
    {
        HideAllOfCategory("hair");
        bool isMale = IsMaleActive();
        GameObject[] currentHairs = isMale ? maleHairs : femaleHairs;

        if (index >= 0 && index < currentHairs.Length)
        {
            GameObject targetHair = currentHairs[index];
            if (targetHair != null)
                SetObjectAndChildrenActive(targetHair, true);
        }

        if (CharacterDataManager.Instance != null)
            CharacterDataManager.Instance.hairIndex = index;

        RefreshCharacter();
    }

    public void ClearAllEquipment()
    {
        HideAllOfCategory("cloth");
        HideAllOfCategory("hair");
        if (CharacterDataManager.Instance != null)
        {
            CharacterDataManager.Instance.outfitIndex = -1;
            CharacterDataManager.Instance.hairIndex = -1;
        }
        RefreshCharacter();
    }

    private bool IsMaleActive() => genderSwitcher != null && genderSwitcher.maleModel.activeSelf;

    private void RefreshCharacter()
    {
        if (genderSwitcher != null)
        {
            GameObject target = IsMaleActive() ? genderSwitcher.maleModel : genderSwitcher.femaleModel;
            genderSwitcher.UpdateGenderParts(target);
        }
    }

    private void SetObjectAndChildrenActive(GameObject obj, bool state)
    {
        if (obj == null) return;
        obj.SetActive(state);
        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
            child.gameObject.SetActive(state);
    }

    private void ToggleParts(GameObject[] parts, bool state)
    {
        foreach (var p in parts) if (p != null) SetObjectAndChildrenActive(p, state);
    }

    private void HideAllOfCategory(string category)
    {
        GameObject[] models = { genderSwitcher.maleModel, genderSwitcher.femaleModel };
        foreach (var model in models)
        {
            if (model == null) continue;
            Transform[] allChildren = model.GetComponentsInChildren<Transform>(true);
            foreach (var t in allChildren)
            {
                if (t.gameObject == model) continue;
                string n = t.name.ToLower();
                if (category == "cloth") { if (n.Contains("cloth")) t.gameObject.SetActive(false); }
                else if (category == "hair") { if (n.Contains("hair")) t.gameObject.SetActive(false); }
            }
        }
    }
}