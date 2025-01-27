using Infohazard.Core;
using UnityEngine;

public class GravelTool : Tool {
    public float _pourRate = 30;
    
    public float _amplitude = 0.2f;

    public ParticleSystem[] _pourEffects;
    public bool[] _effectsAtTop;
    
    public AudioSource _pourSoundSource;
    public AudioClip[] _pourSounds;
    
    public PassiveTimer _pourSoundTimer;

    public override void Deactivate() {
        base.Deactivate();
        _camera.Tank.PourAmplitude = 0;
        SetPlayingParticles(false);
    }

    protected override void Awake() {
        base.Awake();
        
        _pourSoundTimer.Initialize();
    }

    protected override void Update() {
        base.Update();

        bool pouring = Input.GetMouseButton(0);

        Rect cameraRect = _camera.Rect;
        bool inCamera = cameraRect.Contains(Input.mousePosition);

        _toolTransform.localRotation = pouring && inCamera ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;

        if (!inCamera || !pouring) {
            _camera.Tank.PourAmplitude = 0;
            SetPlayingParticles(false);
            return;
        }

        if (_pourSoundTimer.TryConsume()) {
            _pourSoundSource.PlayOneShot(_pourSounds[Random.Range(0, _pourSounds.Length)]);
        }

        Bounds bounds = _camera.Tank.Bounds;

        float x = ((Input.mousePosition.x - cameraRect.x) / cameraRect.width) * bounds.size.x + bounds.min.x;
        float y = bounds.min.y;
        float z = bounds.max.z;

        Vector3 position = new(x, y, z);
        _camera.Tank.Gravel.Pour(_pourRate, position);

        _camera.Tank.PourAmplitude = _camera.Tank.WaterPlane.position.y > _camera.Tank.Bounds.min.y ? _amplitude : 0;
        _camera.Tank.PourPosition = new Vector3(x, _camera.Tank.WaterPlane.position.y, bounds.center.z);
        SetPlayingParticles(true);
    }
    
    private void SetPlayingParticles(bool playing) {
        for (int i = 0; i < _pourEffects.Length; i++) {
            ParticleSystem effect = _pourEffects[i];
            ParticleSystem.EmissionModule emission = effect.emission;
            if (playing) {
                if (!effect.isPlaying) effect.Play();
                Vector3 pos = _camera.Tank.PourPosition;
                if (_effectsAtTop[i]) {
                    pos.y = _camera.Tank.Bounds.max.y;
                }
                
                effect.transform.position = pos;
                emission.enabled = _effectsAtTop[i] || _camera.Tank.WaterPlane.position.y > _camera.Tank.Bounds.min.y;
            } else {
                emission.enabled = false;
            }
        }
    }
}
