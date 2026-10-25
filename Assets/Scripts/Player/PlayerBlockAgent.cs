using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerBlockAgent : MonoBehaviour
{
    [Tooltip("留空则自动获取")]
    [SerializeField] private FirstPersonMotor motor;
    [SerializeField] private PlayerColorSelector colorSelector;
    [SerializeField] private LayerMask groundMask = ~0;
    [Tooltip("从脚底向下探测方块的距离")]
    [SerializeField] private float groundProbeDistance = 0.35f;
    [Tooltip("离地判定缓冲，避免走下小台阶时误判")]
    [SerializeField] private float airborneGrace = 0.1f;
    [Tooltip("掉到这个高度以下就回到上一个安全位置")]
    [SerializeField] private float fallRespawnY = -12f;
    [SerializeField] private float respawnAboveTop = 0.08f;

    private CharacterController _controller;
    private Block _standingBlock;
    private Block _sourceBlock;
    private string _sourceColorId;
    private bool _airborne;
    private float _lastGroundedTime;
    private Vector3 _lastSafePosition;

    public Block StandingBlock => _standingBlock;
    public Block SourceBlock => _sourceBlock;
    public bool IsAirborne => _airborne;

    public event Action<Block, Block> LandingResolved;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        if (motor == null)
        {
            motor = GetComponent<FirstPersonMotor>();
        }

        if (colorSelector == null)
        {
            colorSelector = GetComponent<PlayerColorSelector>();
        }

        _lastSafePosition = transform.position;
        _lastGroundedTime = Time.time;
    }

    private void Update()
    {
        if (transform.position.y < fallRespawnY)
        {
            RespawnAt(_lastSafePosition);
            return;
        }

        if (_controller.isGrounded)
        {
            _lastGroundedTime = Time.time;

            if (_airborne)
            {
                _airborne = false;
                ResolveLanding();
                return;
            }

            Block under = ProbeBlock();

            if (under != null)
            {
                _standingBlock = under;
                _lastSafePosition = transform.position;
            }

            return;
        }

        if (!_airborne && Time.time - _lastGroundedTime > airborneGrace)
        {
            BeginAirborne();
        }
    }

    private void BeginAirborne()
    {
        _airborne = true;
        _sourceBlock = _standingBlock;
        _sourceColorId = _sourceBlock != null ? _sourceBlock.CurrentColorId : null;
    }

    private void ResolveLanding()
    {
        Block landed = ProbeBlock();

        if (_sourceBlock == null)
        {
            _standingBlock = landed;
            return;
        }

        if (landed == _sourceBlock)
        {
            _standingBlock = landed;
            ClearSource();
            return;
        }

        bool success = landed != null && landed.CurrentColorId == _sourceColorId;

        if (success)
        {
            _standingBlock = landed;
            landed.TryAdvance();

            if (colorSelector != null)
            {
                colorSelector.RotateNext();
            }

            Block source = _sourceBlock;
            ClearSource();
            LandingResolved?.Invoke(source, landed);
            return;
        }

        Block failedSource = _sourceBlock;
        _standingBlock = failedSource;
        ClearSource();

        if (failedSource != null)
        {
            TeleportOnto(failedSource);
        }

        LandingResolved?.Invoke(failedSource, landed);
    }

    private void ClearSource()
    {
        _sourceBlock = null;
        _sourceColorId = null;
    }

    private Block ProbeBlock()
    {
        Vector3 origin = transform.position + Vector3.up * 0.15f;
        float distance = groundProbeDistance + 0.15f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponentInParent<Block>();
        }

        return null;
    }

    private void TeleportOnto(Block block)
    {
        Collider collider = block.GetComponentInChildren<Collider>();
        Vector3 center = collider != null ? collider.bounds.center : block.transform.position;
        Vector3 target = new Vector3(center.x, block.GetTopHeight() + respawnAboveTop, center.z);
        RespawnAt(target);
    }

    public void RespawnAt(Vector3 position)
    {
        if (motor != null)
        {
            motor.Teleport(position);
        }
        else
        {
            transform.position = position;
        }

        _standingBlock = null;
        ClearSource();
        _airborne = false;
        _lastGroundedTime = Time.time;
        _lastSafePosition = position;
    }

    public void NotifyExternalMove()
    {
        _airborne = false;
        ClearSource();
        _lastGroundedTime = Time.time;
    }
}
