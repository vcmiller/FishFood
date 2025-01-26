using UnityEngine;

public class WaterTool : Tool {
    public float _pourRate = 0.25f;

    protected override void Update() {
        base.Update();

        bool pouring = Input.GetMouseButton(0);

        Rect cameraRect = _camera.Rect;
        bool inCamera = cameraRect.Contains(Input.mousePosition);

        _toolTransform.localRotation = pouring && inCamera ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;

        if (!inCamera || !pouring) return;

        Vector3 pos = _camera.Tank.WaterPlane.position;
        pos.y = Mathf.Min(pos.y + _pourRate * Time.deltaTime, _camera.Tank.Bounds.max.y);
        _camera.Tank.WaterPlane.position = pos;
    }
}
