using UnityEngine;

[DisallowMultipleComponent]
public class BlockFlipper : MonoBehaviour
{
    [SerializeField] private PlayerInputReader input;
    [Tooltip("留空则自动取子物体上的 Camera")]
    [SerializeField] private Camera viewCamera;
    [Tooltip("可翻转距离（米）")]
    [SerializeField] private float range = 8f;
    [SerializeField] private LayerMask blockingMask = ~0;
    [Tooltip("两次翻转之间的最短间隔（秒）")]
    [SerializeField] private float cooldown = 0.1f;

    private float _nextFlipTime;

    private void Awake()
    {
        if (input == null)
        {
            input = GetComponent<PlayerInputReader>();
        }

        if (viewCamera == null)
        {
            viewCamera = GetComponentInChildren<Camera>();
        }
    }

    private void Update()
    {
        if (input == null || !input.FlipPressed || Time.time < _nextFlipTime)
        {
            return;
        }

        _nextFlipTime = Time.time + cooldown;

        if (TryGetTargetBlock(out Block block))
        {
            block.Flip();
        }
    }

    public bool TryGetTargetBlock(out Block block)
    {
        block = null;

        if (viewCamera == null)
        {
            return false;
        }

        Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, range, blockingMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        block = hit.collider.GetComponentInParent<Block>();
        return block != null;
    }
}
