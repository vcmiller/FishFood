using System;
using System.Collections.Generic;
using Infohazard.Core;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Gravel : MonoBehaviour {
    private NativeArray<float3> _vertices;
    private NativeArray<int> _triangles;
    private NativeArray<float> _heightField;
    private Mesh _mesh;
    private int _topVertexCount;
    private RectInt _heightFieldArea;

    [SerializeField]
    private MeshFilter _meshFilter;

    [SerializeField]
    private MeshCollider _meshCollider;

    [SerializeField]
    private float _heightFieldDensity = 30;

    [SerializeField]
    private Tank _tank;

    private JobHandle _pourJobHandle;
    private bool _pendingPourJob;
    private Material _material;
    private static readonly int WaterYValue = Shader.PropertyToID("_WaterYValue");

    private void Start() {
        Mesh mesh = _meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Re-order the vertices so that all top vertices are first.
        // This will let us easily process them and avoid processing bottom vertices, which will not move.
        int[] vertexIndexMapping = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            vertexIndexMapping[i] = -1;
        }

        List<Vector3> newVertices = new();
        for (int i = 0; i < vertices.Length; i++) {
            Vector3 pos = vertices[i];
            if (pos.y < 0) continue;

            pos.y = -0.01f;

            vertexIndexMapping[i] = newVertices.Count;
            newVertices.Add(pos);
        }

        _topVertexCount = newVertices.Count;

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 pos = vertices[i];
            if (pos.y >= 0) continue;

            vertexIndexMapping[i] = newVertices.Count;
            newVertices.Add(pos);
        }

        _vertices = new NativeArray<float3>(newVertices.Count, Allocator.Persistent);
        for (int i = 0; i < newVertices.Count; i++) {
            _vertices[i] = newVertices[i];
        }

        _triangles = new NativeArray<int>(triangles.Length, Allocator.Persistent);
        for (int i = 0; i < triangles.Length; i++) {
            _triangles[i] = vertexIndexMapping[triangles[i]];
        }

        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;

        int heightFieldMinX = Mathf.FloorToInt(_tank.Bounds.min.x * _heightFieldDensity);
        int heightFieldMaxX = Mathf.CeilToInt(_tank.Bounds.max.x * _heightFieldDensity);
        int heightFieldMinZ = Mathf.FloorToInt(_tank.Bounds.min.z * _heightFieldDensity);
        int heightFieldMaxZ = Mathf.CeilToInt(_tank.Bounds.max.z * _heightFieldDensity);

        int heightFieldWidth = heightFieldMaxX - heightFieldMinX;
        int heightFieldDepth = heightFieldMaxZ - heightFieldMinZ;
        int heightFieldSize = (heightFieldWidth + 1) * (heightFieldDepth + 1);
        _heightField = new NativeArray<float>(heightFieldSize, Allocator.Persistent);
        _heightFieldArea = new RectInt(heightFieldMinX, heightFieldMinZ, heightFieldWidth, heightFieldDepth);
        
        _material = _meshFilter.GetComponent<MeshRenderer>().material;

        UpdateMesh();
    }

    private void OnDestroy() {
        _vertices.Dispose();
        _heightField.Dispose();
        _triangles.Dispose();
    }

    public void Pour(float rate, Vector3 position) {
        PourJob pourJob = new() {
            HeightField = _heightField,
            AddPos = position,
            AddVolume = rate * Time.deltaTime,
            HeightFieldArea = _heightFieldArea,
            HeightFieldDensity = _heightFieldDensity,
        };

        _pourJobHandle = pourJob.Schedule();
        _pendingPourJob = true;
    }

    private void Update() {
        _material.SetFloat(WaterYValue, _tank.WaterPlane.position.y);
    }

    private void LateUpdate() {
        if (_pendingPourJob) {
            _pourJobHandle.Complete();
            _pendingPourJob = false;

            HeightFieldToVerticesJob heightFieldToVerticesJob = new() {
                Vertices = _vertices,
                HeightField = _heightField,
                HeightFieldArea = _heightFieldArea,
                HeightFieldDensity = _heightFieldDensity,
                Transform = _meshFilter.transform.localToWorldMatrix,
            };

            JobHandle handle = heightFieldToVerticesJob.Schedule(_topVertexCount, 32);
            handle.Complete();

            UpdateMesh();
        }
    }

    private void UpdateMesh() {
        _mesh.SetVertices(_vertices);
        _mesh.SetIndices(_triangles, MeshTopology.Triangles, 0);
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshCollider.sharedMesh = _mesh;
    }

    [BurstCompile]
    public struct HeightFieldToVerticesJob : IJobParallelFor {
        public NativeArray<float3> Vertices;

        [ReadOnly]
        public NativeArray<float> HeightField;

        public RectInt HeightFieldArea;
        public float HeightFieldDensity;

        public float4x4 Transform;

        public void Execute(int index) {
            float3 vertex = Vertices[index];
            float4 worldPos = math.mul(Transform, new float4(vertex, 1));

            float x = worldPos.x * HeightFieldDensity - HeightFieldArea.xMin;
            float z = worldPos.z * HeightFieldDensity - HeightFieldArea.yMin;

            int x0 = math.clamp((int) math.floor(x), 0, HeightFieldArea.width);
            int z0 = math.clamp((int) math.floor(z), 0, HeightFieldArea.height);
            int x1 = math.clamp((int) math.ceil(x), 0, HeightFieldArea.width);
            int z1 = math.clamp((int) math.ceil(z), 0, HeightFieldArea.height);

            float xLerp = math.clamp(x - x0, 0, 1);
            float zLerp = math.clamp(z - z0, 0, 1);

            float hx0z0 = HeightField[x0 * HeightFieldArea.height + z0];
            float hx1z0 = HeightField[x1 * HeightFieldArea.height + z0];
            float hx0z1 = HeightField[x0 * HeightFieldArea.height + z1];
            float hx1z1 = HeightField[x1 * HeightFieldArea.height + z1];

            float hz0 = math.lerp(hx0z0, hx1z0, xLerp);
            float hz1 = math.lerp(hx0z1, hx1z1, xLerp);
            float h = math.lerp(hz0, hz1, zLerp);
            vertex.y = h - 0.01f;

            Vertices[index] = vertex;
        }
    }

    [BurstCompile]
    public struct PourJob : IJob {
        public NativeArray<float> HeightField;
        public float3 AddPos;
        public float AddVolume;

        public RectInt HeightFieldArea;
        public float HeightFieldDensity;

        private struct ToPourItem {
            public bool Explored;
            public float Weight;
        }

        public void Execute() {
            float addRadius = 0.15f;
            float maxSlope = 0.1f;

            float totalWeight = 0;
            NativeArray<ToPourItem> toPour = new(HeightField.Length, Allocator.Temp);
            UnsafeRingQueue<int2> queue = new(HeightField.Length, Allocator.Temp);

            int addPosX = math.clamp((int) math.round(AddPos.x * HeightFieldDensity) - HeightFieldArea.xMin, 0,
                                     HeightFieldArea.width);

            int addPosZ = math.clamp((int) math.round(AddPos.z * HeightFieldDensity) - HeightFieldArea.yMin, 0,
                                     HeightFieldArea.height);

            float addPosH = HeightField[addPosX * HeightFieldArea.height + addPosZ];

            float2 addPos = new(addPosX, addPosZ);
            int initialAddRange = (int) math.ceil(addRadius * HeightFieldDensity);

            float rSqr = addRadius * addRadius;

            for (int x = addPosX - initialAddRange; x <= addPosX + initialAddRange; x++) {
                if (x < 0 || x >= HeightFieldArea.width) continue;

                for (int z = addPosZ - initialAddRange; z <= addPosZ + initialAddRange; z++) {
                    if (z < 0 || z >= HeightFieldArea.height) continue;

                    float2 pos = new(x, z);
                    float curHeight = HeightField[x * HeightFieldArea.height + z];
                    float yOffset = addPosH - curHeight;
                    float hOffset = math.length(pos - addPos) / HeightFieldDensity;

                    if (hOffset > addRadius) continue;
                    float lengthSqr = hOffset * hOffset;

                    float slope = hOffset > 0 ? yOffset / hOffset : 0;

                    float weight = math.max(1 - lengthSqr * 0.15f / rSqr, slope * 1.2f / maxSlope);
                    toPour[x * HeightFieldArea.height + z] = new ToPourItem { Explored = true, Weight = weight };
                    totalWeight += weight;
                    queue.Enqueue(new int2(x, z));
                }
            }

            const float sqrt2 = 1.41421356f;

            while (queue.TryDequeue(out int2 current)) {
                float h = HeightField[current.x * HeightFieldArea.height + current.y];
                for (int nx = -1; nx <= 1; nx++) {
                    for (int nz = -1; nz <= 1; nz++) {
                        if (nx == 0 && nz == 0) continue;

                        int x = current.x + nx;
                        int z = current.y + nz;

                        if (x < 0 || x >= HeightFieldArea.width || z < 0 || z >= HeightFieldArea.height) continue;

                        int nIndex = x * HeightFieldArea.height + z;
                        if (toPour[nIndex].Explored) continue;

                        float hOffset = (nx == 0 || nz == 0 ? 1 : sqrt2) / HeightFieldDensity;
                        float yOffset = h - HeightField[nIndex];

                        float slope = yOffset / hOffset;
                        if (slope < maxSlope) {
                            toPour[nIndex] = new ToPourItem { Explored = true };
                            continue;
                        }

                        float weight = slope * 1.2f / maxSlope;
                        toPour[nIndex] = new ToPourItem { Explored = true, Weight = weight };
                        totalWeight += weight;
                        queue.Enqueue(new int2(x, z));
                    }
                }
            }

            if (totalWeight == 0) return;

            float volumePerWeight = AddVolume / totalWeight;
            for (int x = 0; x < HeightFieldArea.width; x++) {
                for (int z = 0; z < HeightFieldArea.height; z++) {
                    int index = x * HeightFieldArea.height + z;
                    if (!toPour[index].Explored) continue;

                    HeightField[index] += toPour[index].Weight * volumePerWeight;
                }
            }
        }
    }
}
