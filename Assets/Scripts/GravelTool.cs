﻿using Infohazard.Core;
using UnityEngine;

public class GravelTool : Tool {
    public float _pourRate = 30;
    
    public float _amplitude = 0.2f;

    public ParticleSystem[] _pourEffects;
    public bool[] _effectsAtTop;

    public override void Deactivate() {
        base.Deactivate();
        _camera.Tank.PourAmplitude = 0;
        SetPlayingParticles(false);
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

        Bounds bounds = _camera.Tank.Bounds;

        float x = ((Input.mousePosition.x - cameraRect.x) / cameraRect.width) * bounds.size.x + bounds.min.x;
        float y = bounds.min.y;
        float z = bounds.max.z;

        Vector3 position = new(x, y, z);
        _camera.Tank.Gravel.Pour(_pourRate, position);
        
        _camera.Tank.PourAmplitude = _amplitude;
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
                emission.enabled = true;
            } else {
                emission.enabled = false;
            }
        }
    }
}
