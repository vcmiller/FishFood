using Infohazard.Core;
using UnityEngine;
using UnityEngine.UI;

public class GroundObjectPlacementTool : Tool {
    public PlaceableObject _placementObject;

    public RectTransform _validPlacementIndicator;
    public RectTransform _invalidPlacementIndicator;
    public Image _placementImage;

    private Image _buttonImage;

    public LayerMask _groundLayerMask;

    protected override void Awake() {
        base.Awake();

        _buttonImage = _button.GetComponent<Image>();
    }

    public override void Activate() {
        base.Activate();

        _placementImage.sprite = _buttonImage.sprite;
    }

    protected override void Update() {
        base.Update();

        bool canPlace = TryGetPlacementPoint(out Vector3 point);

        _validPlacementIndicator.gameObject.SetActive(canPlace);
        _invalidPlacementIndicator.gameObject.SetActive(!canPlace);

        if (canPlace && Input.GetMouseButtonDown(0)) {
            Instantiate(_placementObject, point, Quaternion.identity);
        }
    }

    private bool TryGetPlacementPoint(out Vector3 point) {
        point = Vector3.zero;
        Rect cameraRect = _camera.Rect;
        if (!cameraRect.Contains(Input.mousePosition)) return false;

        Ray ray = _camera.Camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _groundLayerMask)) {
            point = Vector3.zero;
            return false;
        }

        Vector3 max = hit.point + _placementObject._localBounds.max;
        Vector3 min = hit.point + _placementObject._localBounds.min;

        DebugUtility.DrawDebugBounds(new Bounds((min + max) * 0.5f, max - min), Color.red);

        if (!_camera.Tank.Bounds.Contains(max) || !_camera.Tank.Bounds.Contains(min)) {
            point = Vector3.zero;
            return false;
        }

        point = hit.point;
        return true;
    }
}
