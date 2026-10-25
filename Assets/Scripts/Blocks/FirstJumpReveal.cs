using UnityEngine;

public class FirstJumpReveal : MonoBehaviour
{
    [Tooltip("留空则自动找场景里的 PlayerInputReader")]
    [SerializeField] private PlayerInputReader input;
    [Tooltip("第一次跳跃时换色的方块（出生大方块）")]
    [SerializeField] private Block target;
    [Tooltip("换成的类型，例如 T_Fixed_Red")]
    [SerializeField] private BlockType revealedType;

    private bool _revealed;

    public bool Revealed => _revealed;

    private void Awake()
    {
        if (input == null)
        {
            input = FindObjectOfType<PlayerInputReader>();
        }
    }

    private void Update()
    {
        if (_revealed || input == null || !input.JumpPressed)
        {
            return;
        }

        Reveal();
    }

    public void Reveal()
    {
        if (_revealed)
        {
            return;
        }

        _revealed = true;

        if (target == null || revealedType == null)
        {
            return;
        }

        target.SetType(revealedType);
        target.ResetState();
    }

    public void SetTarget(Block block, BlockType type)
    {
        target = block;
        revealedType = type;
    }
}
