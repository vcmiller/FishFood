using System;
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

    private float _rotation;

    private Mesh _mesh;
    private Material[] _materials;
    private Matrix4x4 _matrix;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    protected override void Awake() {
        base.Awake();

        _buttonImage = _button.GetComponent<Image>();
        
        MeshFilter meshFilter = _placementObject.GetComponentInChildren<MeshFilter>();
        _mesh = meshFilter.sharedMesh;
        _materials = meshFilter.GetComponent<MeshRenderer>().sharedMaterials;
        _matrix = _placementObject.transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;

        for (int i = 0; i < _materials.Length; i++) {
            Material mat = new(_materials[i]);
            mat.SetColor(EmissionColor, new Color(0, 0.2f, 0));
            _materials[i] = mat;
        }
    }

    private void OnDestroy() {
        for (int i = 0; i < _materials.Length; i++) {
            DestroyImmediate(_materials[i]);
        }
    }

    public override void Activate() {
        base.Activate();

        _placementImage.sprite = _buttonImage.sprite;
        _rotation = 0;
    }

    public override void Deactivate() {
        base.Deactivate();
        
        _placementImage.enabled = true;
    }

    protected override void Update() {
        base.Update();

        bool canPlace = TryGetPlacementPoint(out Vector3 point);

        _validPlacementIndicator.gameObject.SetActive(canPlace);
        _invalidPlacementIndicator.gameObject.SetActive(!canPlace);
        _placementImage.enabled = canPlace;
        
        _rotation += Input.mouseScrollDelta.y * 10;

        if (canPlace) {
            Matrix4x4 meshPreviewMatrix =
                Matrix4x4.TRS(point, Quaternion.Euler(0, _rotation, 0), Vector3.one) * _matrix;

            for (int i = 0; i < _materials.Length; i++) {
                Graphics.DrawMesh(_mesh, meshPreviewMatrix, _materials[i], 0, _camera.Camera, i);
            }
        }

        if (canPlace && Input.GetMouseButtonDown(0)) {
            Instantiate(_placementObject, point, Quaternion.Euler(0, _rotation, 0));
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
