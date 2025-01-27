using Infohazard.Core;
using UnityEngine;
using UnityEngine.UI;

public class FishPlacementTool : Tool {
    public Fish _placementObject;

    public RectTransform _validPlacementIndicator;
    public RectTransform _invalidPlacementIndicator;
    public Image _placementImage;
    
    public AudioSource _audioSource;

    private Image _buttonImage;

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
            Fish fish = Instantiate(_placementObject, point, Quaternion.identity);
            fish._tank = _camera.Tank;
            _audioSource.PlayOneShot(_audioSource.clip);
        }
    }

    private bool TryGetPlacementPoint(out Vector3 point) {
        point = Vector3.zero;
        Rect cameraRect = _camera.Rect;
        if (!cameraRect.Contains(Input.mousePosition)) return false;

        Ray ray = _camera.Camera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new(_camera.transform.forward, _camera.Tank.FishBounds.center);

        if (!plane.Raycast(ray, out float distance)) {
            point = Vector3.zero;
            return false;
        }

        Vector3 position = ray.GetPoint(distance);
        if (!_camera.Tank.FishBounds.Contains(position)) {
            point = Vector3.zero;
            return false;
        }

        point = position;
        return true;
    }
}
