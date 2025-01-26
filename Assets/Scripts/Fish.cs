using System;
using System.Collections.Generic;
using Infohazard.Core;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fish : MonoBehaviour {
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private SkinnedMeshRenderer _skinRenderer;

    public Tank _tank;

    [SerializeField]
    private int _species;

    [Header("Movement")]
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _radius;

    [SerializeField]
    private float _rotateSpeed;

    [SerializeField]
    private LayerMask _avoidanceLayers;

    [SerializeField]
    private float _avoidanceDistance = 0.1f;

    [Header("Bubbles")]
    [SerializeField]
    private ParticleSystem _bubbleParticles;

    [SerializeField]
    private float _minBubbleDelay = 2f;

    [SerializeField]
    private float _maxBubbleDelay = 4f;

    [Header("Steering")]
    [SerializeField]
    private float _randomSteeringFrequency = 1;

    [SerializeField]
    private float _randomSteeringMagnitude = 60;

    // Flocking
    [SerializeField]
    private float _flockAvoidanceWeight = 1;

    [SerializeField]
    private float _flockAlignmentWeight = 1;

    [SerializeField]
    private float _flockCohesionWeight = 1;

    [SerializeField]
    private float _flockSeparationWeight = 1;

    [SerializeField]
    private float _spearationDistance = 0.2f;

    [SerializeField]
    private float _neighborDistance = 1;

    [SerializeField]
    private float _flockRotateSpeed = 180;
    
    [SerializeField]
    private float _returnToHorizontalWeight = 1;

    [Header("Feeding")]
    [SerializeField]
    private float _foodDetectionRadius = 0.5f;

    [SerializeField]
    private float _foodSteeringWeight = 3;

    [SerializeField]
    private float _feedingCooldown = 1;

    [SerializeField]
    private float _feedingSpeedMultiplier = 2;

    [SerializeField]
    private float _maxFoodToEat = 10;

    [SerializeField]
    private float _startingFood = 1;

    [SerializeField]
    private float _foodDecayRate = 0.05f;

    [Header("Health")]
    [SerializeField]
    private int _skinnyBlendShapeIndex = 0;

    [SerializeField]
    private int _bloatedBlendShapeIndex = 1;

    [SerializeField]
    private float _foodForMaxBloat = 3;

    [SerializeField]
    private float _foodForMinBloat = 1.5f;

    [SerializeField]
    private float _foodForMaxSkinny = 0;

    [SerializeField]
    private float _foodForMinSkinny = 0.5f;

    [SerializeField]
    private float _maxHealth = 100;

    [SerializeField]
    private float _starvationDamagePerSecond = 2;

    [SerializeField]
    private float _overfeedingDamagePerSecond = 2;

    [SerializeField]
    private float _algaeDamagePerSecond = 2;

    [SerializeField]
    private float _healthRegenRate = 5;

    [SerializeField]
    private float _healthRegenDelay = 5;

    [SerializeField]
    private Color _noHealthColor = Color.white;

    [SerializeField]
    private float _maxFadeToNoHealthColor = 0.8f;

    [Header("Death")]
    [SerializeField]
    private float _deadRotateSpeed = 90;

    [SerializeField]
    private float _deadSpeed = 0.5f;

    private PassiveTimer _keepRotatingTimer;
    private PassiveTimer _bubbleTimer;
    private PassiveTimer _feedingTimer;
    private PassiveTimer _healthRegenTimer;
    private Quaternion _lastDesiredRotation;
    private float _randomSteeringTime;
    private Vector3 _randomSteeringOffsets;
    private bool _isEating;

    private Material[] _materials;
    private Color[] _originalColors;
    private Color[] _originalEmissionColors;

    public float Food { get; set; }

    public float Health { get; set; }

    public bool IsADeadFish { get; private set; }

    private static readonly Dictionary<int, List<Fish>> _speciesGroups = new();

    private static readonly int Swimming = Animator.StringToHash("Swimming");
    private static readonly int Offset = Animator.StringToHash("Offset");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Start() {
        _animator.SetBool(Swimming, true);

        _keepRotatingTimer = new PassiveTimer(0, 1);

        _randomSteeringOffsets = new Vector3(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100));

        _bubbleTimer = new PassiveTimer(Random.Range(_minBubbleDelay, _maxBubbleDelay));

        if (!_speciesGroups.TryGetValue(_species, out List<Fish> speciesGroup)) {
            speciesGroup = new List<Fish>();
            _speciesGroups.Add(_species, speciesGroup);
        }

        speciesGroup.Add(this);

        _animator.SetFloat(Offset, Random.value);

        _feedingTimer = new PassiveTimer(0, _feedingCooldown);
        _healthRegenTimer = new PassiveTimer(0, _healthRegenDelay);

        Food = _startingFood;
        Health = _maxHealth;

        _materials = _skinRenderer.materials;
        _originalColors = new Color[_materials.Length];
        _originalEmissionColors = new Color[_materials.Length];

        for (int i = 0; i < _materials.Length; i++) {
            _originalColors[i] = _materials[i].color;
            _originalEmissionColors[i] = _materials[i].GetColor(EmissionColor);
        }
    }

    private void OnDestroy() {
        if (_speciesGroups.TryGetValue(_species, out List<Fish> speciesGroup)) {
            speciesGroup.Remove(this);
        }
    }

    private void Damage(float amount) {
        Health = Mathf.Clamp(Health - amount, 0, _maxHealth);
        _healthRegenTimer.StartInterval();

        if (Health <= 0) {
            IsADeadFish = true;
        }
    }

    private void Update() {
        if (IsADeadFish) {
            UpdateDead();
            return;
        }

        if (_keepRotatingTimer.IsIntervalEnded || _isEating) {
            ApplySteering(out float speed, out _isEating);
            
            Vector3 newPos = transform.position + transform.forward * (speed * Time.deltaTime);

            float padding = 0.01f;
            for (int i = 0; i < 5; i++) {
                Vector3 dir = newPos - transform.position;
                float dist = dir.magnitude;
                dir /= dist;
                if (Physics.SphereCast(transform.position - dir * padding, _radius * 0.8f, dir,
                        out RaycastHit hit, dist + padding, _avoidanceLayers)) {

                    if (i < 4) {
                        float extraDistance = dist - (hit.distance - padding);
                        newPos += hit.normal * (extraDistance * Vector3.Dot(dir, -hit.normal));
                    } else {
                        newPos = transform.position + dir * (hit.distance - padding);
                    }
                }
            }
            
            transform.position = newPos;
        }

        ApplyCollisionDetection();

        if (!_keepRotatingTimer.IsIntervalEnded && !_isEating) {
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, _lastDesiredRotation, _rotateSpeed * Time.deltaTime);
        }

        if (_bubbleTimer.IsIntervalEnded) {
            _bubbleParticles.Play();
            _bubbleTimer.Interval = Random.Range(_minBubbleDelay, _maxBubbleDelay);
            _bubbleTimer.StartInterval();
        }

        UpdateFood();
        UpdateHealth();
    }

    private void LateUpdate() {
        if (!IsADeadFish) {
            UpdateVisuals();
        }
    }

    private void UpdateDead() {
        _animator.SetBool(Swimming, false);

        Quaternion targetRotation = MathUtility.YZRotation(Vector3.down, transform.forward);
        transform.rotation =
            Quaternion.Slerp(transform.rotation, targetRotation, _deadRotateSpeed * Time.deltaTime);

        Vector3 position = transform.position;
        position.y = Mathf.MoveTowards(position.y, _tank.Bounds.max.y, _deadSpeed * Time.deltaTime);
        transform.position = position;
    }

    private void UpdateFood() {
        Food -= _foodDecayRate * Time.deltaTime;
        Food = Mathf.Max(0, Food);

        if (Food <= _foodForMaxSkinny) {
            Damage(_starvationDamagePerSecond * Time.deltaTime);
        } else if (Food >= _foodForMaxBloat) {
            Damage(_overfeedingDamagePerSecond * Time.deltaTime);
        }
    }

    private void UpdateHealth() {
        if (_healthRegenTimer.IsIntervalEnded) {
            Health = Mathf.Clamp(Health + _healthRegenRate * Time.deltaTime, 0, _maxHealth);
        }

        if (_tank.AlgaeLevel > _tank.AlgaeLevelForFullColor) {
            Damage(_algaeDamagePerSecond * Time.deltaTime);
        }
    }

    private void UpdateVisuals() {
        float bloat = Mathf.InverseLerp(_foodForMinBloat, _foodForMaxBloat, Food);
        float skinny = Mathf.InverseLerp(_foodForMinSkinny, _foodForMaxSkinny, Food);

        _skinRenderer.SetBlendShapeWeight(_bloatedBlendShapeIndex, bloat * 100);
        _skinRenderer.SetBlendShapeWeight(_skinnyBlendShapeIndex, skinny * 100);

        float colorLerp = Mathf.Min(_maxFadeToNoHealthColor, 1.0f - Health / _maxHealth);
        for (int i = 0; i < _materials.Length; i++) {
            _materials[i].color = Color.Lerp(_originalColors[i], _noHealthColor, colorLerp);
            _materials[i].SetColor(EmissionColor, Color.Lerp(_originalEmissionColors[i], Color.black, colorLerp));
        }
    }

    private void ApplySteering(out float moveSpeed, out bool isEating) {
        moveSpeed = _speed;
        float rotateSpeed = _flockRotateSpeed;

        float hungerWeight = Mathf.InverseLerp(_maxFoodToEat, _foodForMaxBloat, Food);

        Vector3 foodSteering = CalculateFoodSteering(out FishFood foundFood) * _foodSteeringWeight * hungerWeight;
        Vector3 randomSteering = CalculateRandomSteering() * _randomSteeringMagnitude;
        Vector3 cohesion = CalculateCohesion() * _flockCohesionWeight;
        Vector3 alignment = CalculateAlignment() * _flockAlignmentWeight;
        Vector3 separation = CalculateSeparation() * _flockSeparationWeight;

        if (foundFood) {
            float multiplier = Mathf.Lerp(1, _feedingSpeedMultiplier, hungerWeight);
            moveSpeed *= multiplier;
            rotateSpeed *= multiplier;
            isEating = hungerWeight > 0;
        } else {
            isEating = false;
        }
        
        Vector3 returnToHorizontal = CalculateReturnToHorizontal(isEating) * _returnToHorizontalWeight;

        // Combine the forces
        Vector3 flockingForce = foodSteering + randomSteering + cohesion + alignment + separation + returnToHorizontal;

        // Calculate the desired direction
        Vector3 desiredDirection = transform.forward + flockingForce * Time.deltaTime;
        desiredDirection = desiredDirection.normalized;

        // Rotate the fish towards the desired direction
        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    private Vector3 CalculateFoodSteering(out FishFood foundFood) {
        foundFood = null;

        if (!_feedingTimer.IsIntervalEnded) return Vector3.zero;

        float nearestFoodDistance = _foodDetectionRadius * _foodDetectionRadius;

        foreach (FishFood food in FishFood.FishFoods) {
            float distSqr = (food.transform.position - transform.position).sqrMagnitude;
            if (!(distSqr < nearestFoodDistance)) continue;
            nearestFoodDistance = distSqr;
            foundFood = food;
        }

        if (!foundFood) return Vector3.zero;
        return (foundFood.transform.position - transform.position).normalized;
    }

    private Vector3 CalculateRandomSteering() {
        _randomSteeringTime += Time.deltaTime * _randomSteeringFrequency;

        float x = Mathf.PerlinNoise(_randomSteeringOffsets.x + _randomSteeringTime, 0) * 2 - 1;
        float y = Mathf.PerlinNoise(_randomSteeringOffsets.y + _randomSteeringTime, 0) * 2 - 1;
        float z = Mathf.PerlinNoise(_randomSteeringOffsets.z + _randomSteeringTime, 0) * 2 - 1;

        return new Vector3(x, y, z);
    }

    private Vector3 CalculateCohesion() {
        Vector3 centerOfMass = Vector3.zero;
        int neighborCount = 0;

        if (_speciesGroups.TryGetValue(_species, out List<Fish> sameSpeciesFish)) {
            foreach (Fish fish in sameSpeciesFish) {
                if (fish == this ||
                    !(Vector3.Distance(transform.position, fish.transform.position) < _neighborDistance)) continue;
                centerOfMass += fish.transform.position;
                neighborCount++;
            }
        }

        if (neighborCount > 0) {
            centerOfMass /= neighborCount;
            return (centerOfMass - transform.position).normalized;
        }

        return Vector3.zero;
    }

    private Vector3 CalculateAlignment() {
        Vector3 averageDirection = Vector3.zero;
        int neighborCount = 0;

        if (_speciesGroups.TryGetValue(_species, out List<Fish> sameSpeciesFish)) {
            foreach (Fish fish in sameSpeciesFish) {
                if (fish != this && Vector3.Distance(transform.position, fish.transform.position) < _neighborDistance) {
                    averageDirection += fish.transform.forward;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0) {
            averageDirection /= neighborCount;
            return averageDirection.normalized;
        }

        return Vector3.zero;
    }

    private Vector3 CalculateSeparation() {
        Vector3 avoidance = Vector3.zero;
        int neighborCount = 0;


        foreach (List<Fish> list in _speciesGroups.Values) {
            foreach (Fish fish in list) {
                if (fish == this) continue;

                float distance = Vector3.Distance(transform.position, fish.transform.position);
                if (distance < _spearationDistance) {
                    avoidance += (transform.position - fish.transform.position) / distance;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0) {
            avoidance /= neighborCount;
            return avoidance.normalized;
        }

        return Vector3.zero;
    }

    private Vector3 CalculateReturnToHorizontal(bool isFeeding) {
        return new Vector3(0, -transform.forward.y, 0);
    }

    private void ApplyCollisionDetection() {
        Vector3 position = transform.position;
        Bounds bounds = _tank.FishBounds;

        Vector3 hitNormal = Vector3.zero;
        bool hit = false;

        if (position.x < bounds.min.x + _radius) {
            hit = true;
            hitNormal += Vector3.right;
            position.x = bounds.min.x + _radius;
        } else if (position.x > bounds.max.x - _radius) {
            hit = true;
            hitNormal += Vector3.left;
            position.x = bounds.max.x - _radius;
        }

        if (position.y < bounds.min.y + _radius) {
            hit = true;
            hitNormal += Vector3.up;
            position.y = bounds.min.y + _radius;
        } else if (position.y > bounds.max.y - _radius) {
            hit = true;
            hitNormal += Vector3.down;
            position.y = bounds.max.y - _radius;
        }

        if (position.z < bounds.min.z + _radius) {
            hit = true;
            hitNormal += Vector3.forward;
            position.z = bounds.min.z + _radius;
        } else if (position.z > bounds.max.z - _radius) {
            hit = true;
            hitNormal += Vector3.back;
            position.z = bounds.max.z - _radius;
        }

        if (hit) {
            transform.position = position;
            hitNormal = hitNormal.normalized;

            Vector3 newForward = Vector3.Reflect(transform.forward, hitNormal);
            _lastDesiredRotation = Quaternion.LookRotation(newForward, Vector3.up);
            _keepRotatingTimer.Interval = Quaternion.Angle(transform.rotation, _lastDesiredRotation) / _rotateSpeed;
            _keepRotatingTimer.StartInterval();
        } else if (Physics.SphereCast(position, _radius, transform.forward, out RaycastHit hitInfo, _avoidanceDistance,
                               _avoidanceLayers)) {
            Vector3 newForward = Vector3.Reflect(transform.forward, hitInfo.normal);
            _lastDesiredRotation = Quaternion.LookRotation(newForward, Vector3.up);
            _keepRotatingTimer.Interval = Quaternion.Angle(transform.rotation, _lastDesiredRotation) / _rotateSpeed;
            _keepRotatingTimer.StartInterval();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out FishFood food) && _feedingTimer.TryConsume()) {
            Food += food.Value;
            food.OnEat();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
