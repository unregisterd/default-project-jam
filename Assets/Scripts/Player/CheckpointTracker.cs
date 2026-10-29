using System.Collections.Generic;
using UnityEngine;

public class CheckpointTracker : MonoBehaviour
{
    [SerializeField] private PlayerBlockAgent agent;
    [SerializeField] private AbilityWallet wallet;
    [Tooltip("站在段 Entry 附近多少米内算进入该段")]
    [SerializeField] private float entryRange = 2.5f;

    private readonly HashSet<NpcSymbol> _claimedAtCheckpoint = new HashSet<NpcSymbol>();
    private bool _initialized;

    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }
    public int Points { get; private set; }

    public bool HasCheckpoint => _initialized;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponent<PlayerBlockAgent>();
        }

        if (wallet == null)
        {
            wallet = GetComponent<AbilityWallet>();
        }

        Position = transform.position;
        Rotation = transform.rotation;
        Points = wallet != null ? wallet.Points : 0;
    }

    private void OnEnable()
    {
        if (agent != null)
        {
            agent.StandingBlockChanged += HandleStandingBlockChanged;
        }
    }

    private void OnDisable()
    {
        if (agent != null)
        {
            agent.StandingBlockChanged -= HandleStandingBlockChanged;
        }
    }

    private void HandleStandingBlockChanged(Block block)
    {
        if (block == null)
        {
            return;
        }

        TowerSection section = block.GetComponentInParent<TowerSection>();

        if (section == null)
        {
            return;
        }

        if (section.Entry != null)
        {
            float distance = Vector3.Distance(block.transform.position, section.Entry.position);

            if (distance > entryRange)
            {
                return;
            }
        }

        Snapshot(block);
    }

    private void Snapshot(Block block)
    {
        if (wallet == null)
        {
            wallet = GetComponent<AbilityWallet>();
        }

        Vector3 position = block.transform.position;
        position.y = block.GetTopHeight() + 0.06f;

        Position = position;
        Rotation = transform.rotation;
        Points = wallet != null ? wallet.Points : 0;
        _initialized = true;

        _claimedAtCheckpoint.Clear();

        foreach (NpcSymbol npc in FindObjectsOfType<NpcSymbol>())
        {
            if (npc.HasGivenPoints)
            {
                _claimedAtCheckpoint.Add(npc);
            }
        }

        Debug.Log($"[Checkpoint] {block.name} → 点数 {Points}，已领 NPC {_claimedAtCheckpoint.Count} 个");
    }

    public bool WasClaimedAtCheckpoint(NpcSymbol npc)
    {
        return _claimedAtCheckpoint.Contains(npc);
    }

    public void RestoreNpcs()
    {
        foreach (NpcSymbol npc in FindObjectsOfType<NpcSymbol>())
        {
            if (npc != null && npc.HasGivenPoints && !_claimedAtCheckpoint.Contains(npc))
            {
                npc.ResetClaim();
                Debug.Log($"[Checkpoint] 回档 NPC：{npc.name}");
            }
        }
    }
}
