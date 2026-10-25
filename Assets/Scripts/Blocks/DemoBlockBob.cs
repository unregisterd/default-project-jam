using UnityEngine;

public class DemoBlockBob : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.15f;
    [SerializeField] private float speed = 1.1f;
    [SerializeField] private float spinSpeed = 25f;

    private Vector3 _startPosition;
    private float _phase;

    private void Awake()
    {
        _startPosition = transform.position;
        _phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * speed + _phase) * amplitude;
        transform.position = _startPosition + Vector3.up * offset;
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }
}
