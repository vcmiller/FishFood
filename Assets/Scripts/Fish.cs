using System;
using System.Collections.Generic;
using Infohazard.Core;
using UnityEngine;
using Random = UnityEngine.Random;

public class Fish : MonoBehaviour {
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private Tank _tank;

    [SerializeField]
    private int _species;

    [Header("Movement")]
    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _radius;

    [SerializeField]
    private float _rotateSpeed;

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

    [Header("Feeding")]
    [SerializeField]
    private float _foodDetectionRadius = 0.5f;

    [SerializeField]
    private float _foodSteeringWeight = 3;

    [SerializeField]
    private float _feedingCooldown = 1;

    [SerializeField]
    private float _feedingSpeedMultiplier = 2;

    private PassiveTimer _keepRotatingTimer;
    private PassiveTimer _bubbleTimer;
    private PassiveTimer _feedingTimer;
    private Quaternion _lastDesiredRotation;
    private float _randomSteeringTime;
    private Vector3 _randomSteeringOffsets;

    private static readonly Dictionary<int, List<Fish>> _speciesGroups = new();

    private static readonly int Swimming = Animator.StringToHash("Swimming");
    private static readonly int Offset = Animator.StringToHash("Offset");

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
    }

    private void OnDestroy() {
        if (_speciesGroups.TryGetValue(_species, out List<Fish> speciesGroup)) {
            speciesGroup.Remove(this);
        }
    }

    private void Update() {
        if (_keepRotatingTimer.IsIntervalEnded) {
            ApplySteering(out float speed);
            transform.Translate(Vector3.forward * (speed * Time.deltaTime));
        }

        ApplyCollisionDetection();

        if (!_keepRotatingTimer.IsIntervalEnded) {
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, _lastDesiredRotation, _rotateSpeed * Time.deltaTime);
        }

        if (_bubbleTimer.IsIntervalEnded) {
            _bubbleParticles.Play();
            _bubbleTimer.Interval = Random.Range(_minBubbleDelay, _maxBubbleDelay);
            _bubbleTimer.StartInterval();
        }
    }

    private void ApplySteering(out float moveSpeed) {
        moveSpeed = _speed;

        Vector3 foodSteering = CalculateFoodSteering(out FishFood foundFood) * _foodSteeringWeight;
        Vector3 randomSteering = CalculateRandomSteering() * _randomSteeringMagnitude;
        Vector3 cohesion = CalculateCohesion() * _flockCohesionWeight;
        Vector3 alignment = CalculateAlignment() * _flockAlignmentWeight;
        Vector3 separation = CalculateSeparation() * _flockSeparationWeight;

        if (foundFood) {
            moveSpeed *= _feedingSpeedMultiplier;
        }

        // Combine the forces
        Vector3 flockingForce = foodSteering + randomSteering + cohesion + alignment + separation;

        // Calculate the desired direction
        Vector3 desiredDirection = transform.forward + flockingForce * Time.deltaTime;
        desiredDirection = desiredDirection.normalized;

        // Rotate the fish towards the desired direction
        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up);
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, _flockRotateSpeed * Time.deltaTime);
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

    private Vector3 CalculateCohesion()
    {
        Vector3 centerOfMass = Vector3.zero;
        int neighborCount = 0;

        if (_speciesGroups.TryGetValue(_species, out List<Fish> sameSpeciesFish))
        {
            foreach (Fish fish in sameSpeciesFish) {
                if (fish == this ||
                    !(Vector3.Distance(transform.position, fish.transform.position) < _neighborDistance)) continue;
                centerOfMass += fish.transform.position;
                neighborCount++;
            }
        }

        if (neighborCount > 0)
        {
            centerOfMass /= neighborCount;
            return (centerOfMass - transform.position).normalized;
        }

        return Vector3.zero;
    }

    private Vector3 CalculateAlignment()
    {
        Vector3 averageDirection = Vector3.zero;
        int neighborCount = 0;

        if (_speciesGroups.TryGetValue(_species, out List<Fish> sameSpeciesFish))
        {
            foreach (Fish fish in sameSpeciesFish)
            {
                if (fish != this && Vector3.Distance(transform.position, fish.transform.position) < _neighborDistance)
                {
                    averageDirection += fish.transform.forward;
                    neighborCount++;
                }
            }
        }

        if (neighborCount > 0)
        {
            averageDirection /= neighborCount;
            return averageDirection.normalized;
        }

        return Vector3.zero;
    }

    private Vector3 CalculateSeparation()
    {
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

        if (neighborCount > 0)
        {
            avoidance /= neighborCount;
            return avoidance.normalized;
        }

        return Vector3.zero;
    }

    private void ApplyCollisionDetection() {
        Vector3 position = transform.position;
        Bounds bounds = _tank.Bounds;

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
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.TryGetComponent(out FishFood food) && _feedingTimer.TryConsume()) {
            Destroy(food.gameObject);
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
