using System;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField]
    private Vector2 _pixelSize;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private Tank _tank;
    
    [SerializeField]
    private CameraResizer _cameraResizer;

    [SerializeField]
    private Canvas _canvas;
    
    private BlurWindow _blurWindow;

    public Rect Rect {
        get => _camera.pixelRect;
        set => _camera.pixelRect = value;
    }

    public Camera Camera => _camera;

    public Tank Tank => _tank;

    private void Start() {
        if (Application.platform != RuntimePlatform.WindowsPlayer) {
            _cameraResizer.gameObject.SetActive(false);
            return;
        }

        Vector2 center = new(Screen.width * 0.5f, Screen.height * 0.5f);
        Rect rect = new(center - _pixelSize * 0.5f, _pixelSize);

        _camera.pixelRect = rect;

        RectInt rectInt = new(
            Mathf.RoundToInt(rect.xMin),
            Mathf.RoundToInt(rect.yMin),
            Mathf.RoundToInt(rect.width),
            Mathf.RoundToInt(rect.height));

        _blurWindow = WindowManager.Instance.CreateBlurRegion(rectInt);
    }

    private void Update() {
        if (Application.platform != RuntimePlatform.WindowsPlayer) {
            return;
        }
        
        Rect rect = _camera.pixelRect;
        float scale = _canvas.scaleFactor;
        _cameraResizer._rect.anchoredPosition = rect.min / scale;
        _cameraResizer._rect.sizeDelta = rect.size / scale;
        
        _blurWindow.Rect = new RectInt(
            Mathf.RoundToInt(rect.xMin),
            Mathf.RoundToInt(rect.yMin),
            Mathf.RoundToInt(rect.width),
            Mathf.RoundToInt(rect.height));
    }
}
