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
    
    public float PourAmplitude { get; set; }

    public Vector3 PourPosition { get; set; }
    
    public Transform WaterPlane => _waterPlane;

    private Material _glassMaterialInstance;
    private Material _waterPlaneMaterialInstance;
    private static readonly int Amplitude = Shader.PropertyToID("_Amplitude");
    private static readonly int Position = Shader.PropertyToID("_PourPosition");

    private void Awake() {
        _waterPlaneMaterialInstance = _waterPlane.GetComponent<MeshRenderer>().material;
    }

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
        
        float curAmp = _waterPlaneMaterialInstance.GetFloat(Amplitude);
        curAmp = Mathf.MoveTowards(curAmp, PourAmplitude, 0.5f * Time.deltaTime);
        _waterPlaneMaterialInstance.SetFloat(Amplitude, curAmp);

        if (curAmp > 0) {
            Vector3 curPourPosition = _waterPlaneMaterialInstance.GetVector(Position);
            Vector3 localPourPosition = _waterPlane.InverseTransformPoint(PourPosition);
            localPourPosition.y = 0;
            curPourPosition = Vector3.MoveTowards(curPourPosition, localPourPosition, 1 * Time.deltaTime);
            _waterPlaneMaterialInstance.SetVector(Position, curPourPosition);
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }
}
