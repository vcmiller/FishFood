using UnityEngine;

public class GravelTool : Tool {
    public float _pourRate = 30;

    protected override void Update() {
        base.Update();

        bool pouring = Input.GetMouseButton(0);

        Rect cameraRect = _camera.Rect;
        bool inCamera = cameraRect.Contains(Input.mousePosition);

        _toolTransform.localRotation = pouring && inCamera ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;

        if (!inCamera || !pouring) return;

        Bounds bounds = _camera.Tank.Bounds;

        float x = ((Input.mousePosition.x - cameraRect.x) / cameraRect.width) * bounds.size.x + bounds.min.x;
        float y = bounds.min.y;
        float z = bounds.max.z;

        Vector3 position = new(x, y, z);
        _camera.Tank.Gravel.Pour(_pourRate, position);
    }
}
