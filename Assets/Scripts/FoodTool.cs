using UnityEngine;

public class FoodTool : Tool {
    public GameObject _foodPrefab;
    public int _numToCreate;
    public float _range = 0.1f;

    public CameraController _camera;

    protected override void Update() {
        base.Update();

        if (Input.GetMouseButtonDown(0)) {
            Rect cameraRect = _camera.Rect;
            if (!cameraRect.Contains(Input.mousePosition)) return;

            Bounds bounds = _camera.Tank.Bounds;

            float x = ((Input.mousePosition.x - cameraRect.x) / cameraRect.width) * bounds.size.x + bounds.min.x;
            float y = bounds.max.y;
            float z = bounds.center.z;
            Vector3 basePosition = new(x, y, z);

            for (int i = 0; i < _numToCreate; i++) {
                Vector3 position = basePosition + Random.insideUnitSphere * _range;
                Instantiate(_foodPrefab, position, Quaternion.identity);
            }
        }
    }
}
