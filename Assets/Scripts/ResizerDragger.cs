using Infohazard.Core;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResizerDragger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [SerializeField]
    private CameraController _camera;
    
    [SerializeField]
    private RectTransform _oppositeCorner;

    private Vector2 _startClickOppositeCorner;
    private Vector2 _resizeVector;

    private float _minDiagonal = 200;
    
    private bool _dragging;
    
    public void OnPointerDown(PointerEventData eventData) {
        _dragging = true;
        _startClickOppositeCorner = _oppositeCorner.position;
        _resizeVector = (transform.position - _oppositeCorner.position).normalized;
    }

    private void Update() {
        if (!_dragging) return;
        
        Vector2 mousePos = Input.mousePosition.ToXY();
        mousePos.x = Mathf.Clamp(mousePos.x, 0, Screen.width);
        mousePos.y = Mathf.Clamp(mousePos.y, 0, Screen.height);

        Vector2 vector = Vector3.Project(mousePos - _startClickOppositeCorner, _resizeVector);
        if (vector.magnitude < _minDiagonal) vector = vector.normalized * _minDiagonal;

        if (_startClickOppositeCorner.x + vector.x < 0) {
            vector.x = -_startClickOppositeCorner.x;
            vector.y = vector.x / _resizeVector.x * _resizeVector.y;
        } else if (_startClickOppositeCorner.x + vector.x > Screen.width) {
            vector.x = Screen.width - _startClickOppositeCorner.x;
            vector.y = vector.x / _resizeVector.x * _resizeVector.y;
        }
        
        if (_startClickOppositeCorner.y + vector.y < 0) {
            vector.y = -_startClickOppositeCorner.y;
            vector.x = vector.y / _resizeVector.y * _resizeVector.x;
        } else if (_startClickOppositeCorner.y + vector.y > Screen.height) {
            vector.y = Screen.height - _startClickOppositeCorner.y;
            vector.x = vector.y / _resizeVector.y * _resizeVector.x;
        }
        
        Vector2 selfPos = _startClickOppositeCorner + vector;
        
        float xMin = Mathf.Min(selfPos.x, _startClickOppositeCorner.x);
        float xMax = Mathf.Max(selfPos.x, _startClickOppositeCorner.x);
        float yMin = Mathf.Min(selfPos.y, _startClickOppositeCorner.y);
        float yMax = Mathf.Max(selfPos.y, _startClickOppositeCorner.y);
        
        _camera.Rect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    public void OnPointerUp(PointerEventData eventData) {
        _dragging = false;
        EventSystem.current.SetSelectedGameObject(null);
    }
}