using UnityEngine;

public class WebGLInputFix : MonoBehaviour
{
    void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Input.captureAllKeyboardInput = true;
#endif
    }
}