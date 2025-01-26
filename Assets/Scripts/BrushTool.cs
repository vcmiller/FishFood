using UnityEngine;

public class BrushTool : Tool {
    [SerializeField]
    private float _baseScrubRate = 1.0f;

    [SerializeField]
    private float _scrubRatePerMovement = 0.1f;

    private static readonly int Scrub = Animator.StringToHash("Scrub");

    protected override void Update() {
        base.Update();

        bool scrubbing = Input.GetMouseButton(0);

        Rect cameraRect = _camera.Rect;
        bool inCamera = cameraRect.Contains(Input.mousePosition);

        _animator.SetBool(Scrub, scrubbing && inCamera);

        if (!inCamera || !scrubbing) return;

        float scrub = _baseScrubRate * Time.deltaTime +
                      _scrubRatePerMovement * Input.mousePositionDelta.magnitude;

        _camera.Tank.AlgaeLevel = Mathf.Max(0, _camera.Tank.AlgaeLevel - scrub);
    }
}
