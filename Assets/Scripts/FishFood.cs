using System;
using System.Collections.Generic;
using UnityEngine;

public class FishFood : MonoBehaviour {
    public static List<FishFood> FishFoods { get; } = new();

    private void Awake() {
        FishFoods.Add(this);
    }

    private void OnDestroy() {
        FishFoods.Remove(this);
    }
}
