using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;

public class FaceAISystem : MonoBehaviour
{
    [Header("API 設定")]
    public string apiKey = "";

    [Header("UI 連結")]
    public TMP_InputField myInputField;
    public FaceAutoGenerator faceGenerator;

    [System.Serializable] public class GeminiRequest { public List<Content> contents; }
    [System.Serializable] public class Content { public List<Part> parts; }
    [System.Serializable] public class Part { public string text; }

    [System.Serializable] public class FaceData { public List<FeatureValue> features; }
    [System.Serializable] public class FeatureValue { public string name; public float value; }

    public void SimpleAskAI()
    {
        if (string.IsNullOrEmpty(apiKey)) { Debug.LogError("請填入 API Key！"); return; }
        if (myInputField == null) { Debug.LogError("未連結 InputField！"); return; }
        StartCoroutine(CallGemini(myInputField.text));
    }

    IEnumerator CallGemini(string userInput)
    {
        string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=" + apiKey.Trim();
        string featuresList = "eyebrowPosX, eyeSizeX, eyeRotate, noseSizeX, mouthSizeX, jawPosX";
        // 強烈要求 AI 只回傳 JSON，不要加 Markdown 標籤
        string systemPrompt = $"你是一個捏臉專家。請根據描述決定參數({featuresList})並加上_max或_min。只准回傳 JSON 格式如下：{{\"features\":[{{\"name\":\"eyeSizeX_max\",\"value\":80}}]}}。不要回傳任何 Markdown 或文字解釋。";

        GeminiRequest req = new GeminiRequest
        {
            contents = new List<Content> {
                new Content { parts = new List<Part> { new Part { text = systemPrompt + "\n玩家指令：" + userInput } } }
            }
        };
        string jsonPayload = JsonUtility.ToJson(req);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("<color=green>【連線成功】</color>");
                ProcessResponse(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"失敗: {request.downloadHandler.text}");
            }
        }
    }

    void ProcessResponse(string responseText)
    {
        string flattened = responseText.Replace("\\\"", "\"").Replace("\\n", " ").Replace("\\r", "");
        Match match = Regex.Match(flattened, @"\{""features"".*?\]\}", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        if (match.Success)
        {
            string cleanJson = match.Value;
            try
            {
                FaceData data = JsonUtility.FromJson<FaceData>(cleanJson);
                foreach (var item in data.features)
                {
                    string rawName = item.name;
                    float finalValue = item.value;

                    // 1. 名稱對齊 (eyeSizeX_max -> eyeSizeX)
                    string sliderName = rawName.Replace("_max", "").Replace("_min", "");

                    // 2. 正負值映射 (-100 ~ 100)
                    if (rawName.Contains("_min"))
                    {
                        finalValue = -item.value;
                    }

                    // 3. 呼叫你的原始函式
                    faceGenerator.SetSliderValueByName(sliderName, finalValue);

                    // --- 新增：強制連動保險 ---
                    // 在畫面上直接尋找與參數同名的 Slider 物件並撥動它
                    GameObject sliderObj = GameObject.Find(sliderName);
                    if (sliderObj != null)
                    {
                        UnityEngine.UI.Slider s = sliderObj.GetComponentInChildren<UnityEngine.UI.Slider>();
                        if (s != null)
                        {
                            s.value = finalValue;
                            // 手動觸發 Slider 的事件，確保模型會跟著動
                            s.onValueChanged.Invoke(finalValue);
                        }
                    }

                    Debug.Log($"<color=lime>>> 強制更新：{sliderName} 設定為 {finalValue}</color>");
                }
                Debug.Log("<color=yellow>★ 捏臉連動完成！ ★</color>");
            }
            catch (System.Exception e)
            {
                Debug.LogError("解析失敗: " + e.Message);
            }
        }
    }
}