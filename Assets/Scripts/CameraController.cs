using UnityEngine;

public class CameraController : MonoBehaviour {
    [SerializeField]
    private Vector2 _pixelSize;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private Tank _tank;

    public Rect Rect => _camera.pixelRect;

    public Tank Tank => _tank;

    private void Start() {
        Vector2 center = new(Screen.width * 0.5f, Screen.height * 0.5f);
        Rect rect = new(center - _pixelSize * 0.5f, _pixelSize);

        _camera.pixelRect = rect;

        RectInt rectInt = new(
            Mathf.RoundToInt(rect.xMin),
            Mathf.RoundToInt(rect.yMin),
            Mathf.RoundToInt(rect.width),
            Mathf.RoundToInt(rect.height));

        WindowManager.Instance.CreateBlurRegion(rectInt);
    }
}
