using Infohazard.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResizerMoveDragger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField]
    private CameraController _camera;

    private Vector2 _startClickPoint;
    private Vector2 _startRectPosition;
    
    private bool _dragging;
    
    public void OnPointerDown(PointerEventData eventData) {
        _dragging = true;
        _startClickPoint = Input.mousePosition;
        _startRectPosition = _camera.Rect.position;
    }

    private void Update() {
        if (!_dragging) return;

        Vector2 delta = Input.mousePosition.ToXY() - _startClickPoint;
        
        Rect rect = _camera.Rect;
        rect.position = _startRectPosition + delta;
        if (rect.x < 0) rect.x = 0;
        if (rect.y < 0) rect.y = 0;
        if (rect.xMax > Screen.width) rect.x = Screen.width - rect.width;
        if (rect.yMax > Screen.height) rect.y = Screen.height - rect.height;
        _camera.Rect = rect;
    }

    public void OnPointerUp(PointerEventData eventData) {
        _dragging = false;
        EventSystem.current.SetSelectedGameObject(null);
    }
}
