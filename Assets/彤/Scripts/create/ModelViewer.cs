using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections; // 必須引用此開頭以使用協程

public class ModelViewer : MonoBehaviour
{
    [Header("目標對象")]
    public Transform target;

    [Header("旋轉設定")]
    public float rotateSpeed = 0.2f;
    private Vector2 lastMousePos;
    private bool isDragging = false;

    [Header("視角參數設定")]
    // 遠鏡頭 (原始數值)
    public float farFOV = 60f;
    public float farHeight = 3.04f;

    // 近鏡頭 (對準臉部)
    public float closeFOV = 20f;
    public float closeHeight = 3.29f;

    [Header("平滑過渡速度")]
    public float duration = 0.5f; // 滑動到目的地需要的秒數

    private Camera cam;
    private Coroutine transitionCoroutine; // 用來記錄目前的滑動動作

    void Start()
    {
        cam = GetComponent<Camera>();
        // 初始化直接定位，不滑動
        cam.fieldOfView = farFOV;
        Vector3 pos = transform.position;
        pos.y = farHeight;
        transform.position = pos;
    }

    void Update()
    {
        // 防止穿透 UI 旋轉模型
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            isDragging = false;
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // 旋轉邏輯 (維持你原本的寫法)
        if (mouse.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMousePos = mouse.position.ReadValue();
        }
        if (mouse.leftButton.wasReleasedThisFrame) isDragging = false;

        if (isDragging && target != null)
        {
            Vector2 currentMousePos = mouse.position.ReadValue();
            float deltaX = currentMousePos.x - lastMousePos.x;
            target.Rotate(Vector3.up, -deltaX * rotateSpeed, Space.World);
            lastMousePos = currentMousePos;
        }
    }

    // --- 給 UI 按鈕呼叫的功能 (現在會動態滑動) ---

    public void SetCloseUpView()
    {
        StartTransition(closeHeight, closeFOV);
    }

    public void SetFarView()
    {
        StartTransition(farHeight, farFOV);
    }

    // 啟動平滑切換的輔助函式
    private void StartTransition(float targetHeight, float targetFOV)
    {
        // 如果原本還在滑，先停掉舊的，避免衝突
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(SmoothMove(targetHeight, targetFOV));
    }

    // 核心滑動邏輯 (Coroutine)
    IEnumerator SmoothMove(float targetHeight, float targetFOV)
    {
        float elapsed = 0f;
        float startHeight = transform.position.y;
        float startFOV = cam.fieldOfView;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 使用 SmoothStep 讓過渡曲線有漸快漸慢的「高級感」
            float curve = Mathf.SmoothStep(0, 1, t);

            // 更新高度
            Vector3 currentPos = transform.position;
            currentPos.y = Mathf.Lerp(startHeight, targetHeight, curve);
            transform.position = currentPos;

            // 更新 FOV
            cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, curve);

            yield return null; // 等待下一幀
        }

        // 確保最終位置絕對精準
        Vector3 finalPos = transform.position;
        finalPos.y = targetHeight;
        transform.position = finalPos;
        cam.fieldOfView = targetFOV;
    }
}