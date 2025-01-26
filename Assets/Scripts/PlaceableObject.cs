using System;
using UnityEngine;

public class PlaceableObject : MonoBehaviour {
    public Bounds _localBounds;

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + _localBounds.center, _localBounds.size);
    }
}
