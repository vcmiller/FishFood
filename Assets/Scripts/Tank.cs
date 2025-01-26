using System;
using UnityEngine;

public class Tank : MonoBehaviour {
    [SerializeField]
    private Bounds _bounds;

    public Bounds Bounds => _bounds;

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }
}
