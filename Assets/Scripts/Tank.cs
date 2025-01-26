using System;
using UnityEngine;

public class Tank : MonoBehaviour {
    [SerializeField]
    private Bounds _bounds;

    [SerializeField]
    private float _baseAlgaeGrowthRate = 1.0f;

    [SerializeField]
    private float _algaeFoodConsumption = 1.0f;

    [SerializeField]
    private MeshRenderer[] _glass;

    [SerializeField]
    private Color _originalGlassColor;

    [SerializeField]
    private Color _algaeColor;

    [SerializeField]
    private Material _glassMaterial;

    [SerializeField]
    private float _algaeLevelForFullColor;

    public Bounds Bounds => _bounds;

    public float DissolvedFoodLevel { get; set; }

    public float AlgaeLevel { get; set; }

    public float AlgaeLevelForFullColor => _algaeLevelForFullColor;

    private Material _glassMaterialInstance;

    private void Start() {
        _glassMaterialInstance = new Material(_glassMaterial);
        _glassMaterialInstance.color = _originalGlassColor;

        foreach (MeshRenderer glass in _glass) {
            glass.sharedMaterial = _glassMaterialInstance;
        }
    }

    private void OnDestroy() {
        DestroyImmediate(_glassMaterialInstance);
    }

    private void Update() {
        if (DissolvedFoodLevel > 0) {
            DissolvedFoodLevel -= _algaeFoodConsumption * Time.deltaTime;
            AlgaeLevel += _baseAlgaeGrowthRate * Time.deltaTime;

            Color glassColor = Color.Lerp(_originalGlassColor, _algaeColor, AlgaeLevel / _algaeLevelForFullColor);
            _glassMaterialInstance.color = glassColor;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }
}
