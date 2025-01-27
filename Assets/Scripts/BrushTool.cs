using UnityEngine;

public class BrushTool : Tool {
    [SerializeField]
    private float _baseScrubRate = 1.0f;

    [SerializeField]
    private float _scrubRatePerMovement = 0.1f;
    
    [SerializeField]
    private AudioSource _audioSource;

    private static readonly int Scrub = Animator.StringToHash("Scrub");

    public override void Deactivate() {
        base.Deactivate();
        
        _audioSource.Stop();
    }

    protected override void Update() {
        base.Update();

        bool scrubbing = Input.GetMouseButton(0);

        Rect cameraRect = _camera.Rect;
        bool inCamera = cameraRect.Contains(Input.mousePosition);
        
        bool scrubbingInCamera = scrubbing && inCamera;

        _animator.SetBool(Scrub, scrubbingInCamera);
        
        if (scrubbingInCamera && !_audioSource.isPlaying) {
            _audioSource.Play();
        } else if (!scrubbingInCamera && _audioSource.isPlaying) {
            _audioSource.Stop();
        }

        if (!scrubbingInCamera) return;

        float scrub = _baseScrubRate * Time.deltaTime +
                      _scrubRatePerMovement * Input.mousePositionDelta.magnitude;

        _camera.Tank.AlgaeLevel = Mathf.Max(0, _camera.Tank.AlgaeLevel - scrub);
    }
}
