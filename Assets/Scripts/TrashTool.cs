using Infohazard.Core;
using UnityEngine;

public class TrashTool : Tool {
    [SerializeField]
    private LayerMask _layerMask;
    
    public RectTransform _validPlacementIndicator;
    public RectTransform _invalidPlacementIndicator;

    protected override void Update() {
        base.Update();
        
        bool canDelete = TryGetObjectToDelete(out GameObject obj);

        _validPlacementIndicator.gameObject.SetActive(canDelete);
        _invalidPlacementIndicator.gameObject.SetActive(!canDelete);
        
        if (canDelete && Input.GetMouseButtonDown(0)) {
            Destroy(obj);
        }
    }

    private bool TryGetObjectToDelete(out GameObject obj) {
        obj = null;
        Rect cameraRect = _camera.Rect;
        if (!cameraRect.Contains(Input.mousePosition)) return false;

        Ray ray = _camera.Camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, _layerMask)) {
            return false;
        }

        if (hit.collider.TryGetComponentInParent(out Fish fish)) {
            obj = fish.gameObject;
            return true;
        } else if (hit.collider.TryGetComponentInParent(out PlaceableObject placeableObject)) {
            obj = placeableObject.gameObject;
            return true;
        } else if (hit.collider.TryGetComponentInParent(out FishFood food)) {
            obj = food.gameObject;
            return true;
        }

        return false;
    }
}