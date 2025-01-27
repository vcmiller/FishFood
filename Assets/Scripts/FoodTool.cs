using UnityEngine;

public class FoodTool : Tool {
    public FishFood _foodPrefab;
    public int _numToCreate;
    public float _range = 0.1f;
    public AudioSource _sound;
    private static readonly int Shake = Animator.StringToHash("Shake");

    protected override void Update() {
        base.Update();

        if (Input.GetMouseButtonDown(0)) {
            Rect cameraRect = _camera.Rect;
            if (!cameraRect.Contains(Input.mousePosition)) return;

            _animator.SetTrigger(Shake);
            _sound.PlayOneShot(_sound.clip);

            Bounds bounds = _camera.Tank.Bounds;

            float x = ((Input.mousePosition.x - cameraRect.x) / cameraRect.width) * bounds.size.x + bounds.min.x;
            float y = bounds.max.y;
            float z = bounds.center.z;
            Vector3 basePosition = new(x, y, z);

            for (int i = 0; i < _numToCreate; i++) {
                Vector3 position = basePosition + Random.insideUnitSphere * _range;
                FishFood food = Instantiate(_foodPrefab, position, Quaternion.identity);
                food.Tank = _camera.Tank;
            }
        }
    }
}
