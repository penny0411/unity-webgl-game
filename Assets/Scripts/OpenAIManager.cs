using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

[Serializable]
public class ChatRequest
{
    public string userId;
    public string playerName;
    public string charName;
    public string gender;
    public int age;
    public string personality;
    public string message;
}

[Serializable]
public class ChatResponse
{
    public string reply;
    public int remainingCredits;
}

public class OpenAIManager : MonoBehaviour
{
    public static OpenAIManager Instance;

    private string apiUrl = "https://unity-openai-server.onrender.com/chat";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public IEnumerator SendMessageToAI(string message, Action<string, int> onResponse)
    {
        ChatRequest data = new ChatRequest
        {
            userId = GameDataManager.Instance.playerId,
            playerName = GameDataManager.Instance.playerName,
            charName = GameDataManager.Instance.characterName,
            gender = GameDataManager.Instance.characterGender,
            age = GameDataManager.Instance.characterAge,
            personality = GameDataManager.Instance.characterPersonality,
            message = message
        };

        string json = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("API Error: " + request.error);
            onResponse?.Invoke("AI 連線失敗", 0);
        }
        else
        {
            ChatResponse response =
                JsonUtility.FromJson<ChatResponse>(request.downloadHandler.text);

            if (response != null)
                onResponse?.Invoke(response.reply, response.remainingCredits);
            else
                onResponse?.Invoke("回應解析錯誤", 0);
        }
    }
}