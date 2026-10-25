using UnityEngine;

public class DemoBlockBob : MonoBehaviour
{
    [Tooltip("留空则自动找场景里的玩家")]
    [SerializeField] private Transform followTarget;
    [Tooltip("相对玩家的位置偏移（右 / 上 / 前）")]
    [SerializeField] private Vector3 followOffset = new Vector3(1.0f, 1.9f, 2.2f);
    [Tooltip("跟随平滑时间，越大越拖后腿")]
    [SerializeField] private float followSmoothTime = 0.25f;
    [Tooltip("跟随开关，关掉就只在原地漂浮")]
    [SerializeField] private bool followEnabled = true;

    [Header("漂浮与自转")]
    [SerializeField] private float amplitude = 0.12f;
    [SerializeField] private float speed = 1.1f;
    [SerializeField] private float spinSpeed = 25f;

    private Vector3 _anchor;
    private Vector3 _velocity;
    private float _phase;

    private void Awake()
    {
        _phase = Random.value * Mathf.PI * 2f;
        _anchor = transform.position;

        if (followTarget == null)
        {
            PlayerBlockAgent agent = FindObjectOfType<PlayerBlockAgent>();

            if (agent != null)
            {
                followTarget = agent.transform;
            }
        }
    }

    private void LateUpdate()
    {
        Vector3 desired = _anchor;

        if (followEnabled && followTarget != null)
        {
            desired = followTarget.position + followTarget.rotation * followOffset;
        }

        _anchor = Vector3.SmoothDamp(_anchor, desired, ref _velocity, followSmoothTime);

        float bob = Mathf.Sin(Time.time * speed + _phase) * amplitude;
        transform.position = _anchor + Vector3.up * bob;
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
    }

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }
}
