using UnityEngine;

public class BlockRevealOnFirstJump : MonoBehaviour
{
    [Tooltip("留空则自动找场景里的 PlayerInputReader")]
    [SerializeField] private PlayerInputReader input;
    [Tooltip("第一次跳跃时一起显现颜色的方块")]
    [SerializeField] private Block[] targets = new Block[0];

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

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].Reveal();
            }
        }
    }

    public void SetTargets(Block[] blocks)
    {
        targets = blocks ?? new Block[0];
    }
}
