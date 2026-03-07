using UnityEngine;
using System.Collections.Generic;

public class SkinColorConnector : MonoBehaviour
{
    public List<Renderer> bodyRenderers = new List<Renderer>();

    // 增加此屬性，讓 DataManager 存檔時可以抓到顏色
    public Color currentColor = Color.white;

    // 統一方法名稱為 SetColor，供外部調用
    public void SetColor(Color newColor)
    {
        currentColor = newColor; // 紀錄當前顏色

        if (bodyRenderers.Count == 0) return;

        foreach (Renderer r in bodyRenderers)
        {
            if (r == null) continue;
            Material[] allMaterials = r.materials;
            foreach (Material m in allMaterials)
            {
                if (m.name.ToLower().Contains("body"))
                {
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", newColor);
                    else if (m.HasProperty("_Color")) m.SetColor("_Color", newColor);
                }
            }
            r.materials = allMaterials;
        }
    }

    // 為了相容你現有的色盤呼叫，保留此方法並轉向 SetColor
    public void SetModelColor(Color newColor) => SetColor(newColor);

    public void ClearAndAddTarget(Renderer newRenderer)
    {
        if (newRenderer != null && !bodyRenderers.Contains(newRenderer))
            bodyRenderers.Add(newRenderer);
    }

    public void ResetTargets() => bodyRenderers.Clear();
}