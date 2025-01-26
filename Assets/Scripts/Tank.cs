using System;
using Infohazard.Core;
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

    [SerializeField]
    private Gravel _gravel;

    [SerializeField]
    private Transform _waterPlane;

    public Bounds Bounds => _bounds;

    public Bounds FishBounds { get; private set; }

    public float DissolvedFoodLevel { get; set; }

    public float AlgaeLevel { get; set; }

    public float AlgaeLevelForFullColor => _algaeLevelForFullColor;

    public Gravel Gravel => _gravel;

    public Transform WaterPlane => _waterPlane;

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
        }

        Color glassColor = Color.Lerp(_originalGlassColor, _algaeColor, AlgaeLevel / _algaeLevelForFullColor);
        _glassMaterialInstance.color = glassColor;

        Bounds fishBounds = Bounds;
        fishBounds.max = fishBounds.max.WithY(_waterPlane.position.y);
        FishBounds = fishBounds;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }
}
