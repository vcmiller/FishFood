using System;
using UnityEngine;

public class WaterTool : Tool {
    public float _pourRate = 0.25f;
    
    public float _amplitude = 0.1f;

    public ParticleSystem[] _pourEffects;
    public bool[] _effectsAtTop;
    
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
        
        Bounds bounds = _camera.Tank.Bounds;
        float x = ((Input.mousePosition.x - cameraRect.x) / cameraRect.width) * bounds.size.x + bounds.min.x;
        float z = bounds.center.z;

        Vector3 pos = _camera.Tank.WaterPlane.position;
        pos.y = Mathf.Min(pos.y + _pourRate * Time.deltaTime, _camera.Tank.Bounds.max.y);
        _camera.Tank.WaterPlane.position = pos;
        _camera.Tank.PourAmplitude = pos.y < _camera.Tank.Bounds.max.y ? _amplitude : 0;
        _camera.Tank.PourPosition = new Vector3(x, pos.y, z);

        SetPlayingParticles(pos.y < _camera.Tank.Bounds.max.y);
    }
    
    private void SetPlayingParticles(bool playing) {
        for (int i = 0; i < _pourEffects.Length; i++) {
            ParticleSystem effect = _pourEffects[i];
            if (!effect.isPlaying) effect.Play();

            ParticleSystem.EmissionModule emission = effect.emission;
            if (playing) {
                Vector3 pos = _camera.Tank.PourPosition;
                if (_effectsAtTop[i]) {
                    pos.y = _camera.Tank.Bounds.max.y;
                }

                effect.transform.position = pos;
                emission.enabled = true;
            } else {
                emission.enabled = false;
            }
        }
    }

    public override void Deactivate() {
        base.Deactivate();
        _camera.Tank.PourAmplitude = 0;
        SetPlayingParticles(false);
    }
}
