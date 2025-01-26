using System;
using System.Collections.Generic;
using Infohazard.Core;
using UnityEngine;

public class FishFood : MonoBehaviour {
    public static List<FishFood> FishFoods { get; } = new();

    [SerializeField]
    private float _minLifetime = 10f;

    [SerializeField]
    private float _maxLifetime = 20f;

    [SerializeField]
    private GameObject _dissolveEffectPrefab;

    [SerializeField]
    private float _foodValue = 1f;

    private PassiveTimer _lifetimeTimer;

    public Tank Tank { get; set; }
    public float Value => _foodValue;

    private void Awake() {
        FishFoods.Add(this);

        _lifetimeTimer = new PassiveTimer(UnityEngine.Random.Range(_minLifetime, _maxLifetime));
    }

    private void Update() {
        if (_lifetimeTimer.IsIntervalEnded) {
            Instantiate(_dissolveEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
            Tank.DissolvedFoodLevel += _foodValue;
        }
    }

    private void OnDestroy() {
        FishFoods.Remove(this);
    }
}
