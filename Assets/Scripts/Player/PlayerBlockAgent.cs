using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerBlockAgent : MonoBehaviour
{
    [Tooltip("留空则自动获取")]
    [SerializeField] private FirstPersonMotor motor;
    [SerializeField] private LayerMask groundMask = ~0;
    [Tooltip("从脚底向下探测方块的距离")]
    [SerializeField] private float groundProbeDistance = 0.35f;
    [Tooltip("掉到低于最后站立方块多少米就回退（推荐 3~5）")]
    [SerializeField] private float fallDepth = 3.5f;
    [Tooltip("绝对兜底高度（找不到最后方块时使用）")]
    [SerializeField] private float fallRespawnY = -12f;

    [Header("落地音效")]
    [Tooltip("留空则从 Resources/Audio/step_land 自动加载")]
    [SerializeField] private AudioClip landingClip;
    [Range(0f, 1f)]
    [SerializeField] private float landingVolume = 0.7f;

    [Header("走路脚步")]
    [Tooltip("留空则复用落地音效")]
    [SerializeField] private AudioClip stepClip;
    [Tooltip("每走多少米响一次")]
    [SerializeField] private float stepDistance = 2f;
    [Range(0f, 1f)]
    [SerializeField] private float stepVolume = 0.35f;

    private CharacterController _controller;
    private Block _standingBlock;
    private Block _lastStandingBlock;
    private float _lastStandingTop;
    private Vector3 _lastSafePosition;
    private AudioSource _audio;
    private float _stepAccumulator;
    private float _lastLandingTime = -10f;
    private bool _wasGrounded = true;

    public Block StandingBlock => _standingBlock;

    public event Action<Block> StandingBlockChanged;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        if (motor == null)
        {
            motor = GetComponent<FirstPersonMotor>();
        }

        _lastSafePosition = transform.position;

        _audio = GetComponent<AudioSource>();

        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
        }

        if (landingClip == null)
        {
            landingClip = Resources.Load<AudioClip>("Audio/step_land");
        }

        if (stepClip == null)
        {
            stepClip = landingClip;
        }
    }

    private void Update()
    {
        if (ShouldRespawn())
        {
            RespawnOnLastBlock();
            return;
        }

        if (!_controller.isGrounded)
        {
            _wasGrounded = false;
            return;
        }

        if (!_wasGrounded)
        {
            _wasGrounded = true;
            _lastLandingTime = Time.time;
            _stepAccumulator = stepDistance * 0.5f;
            PlayLandingFeedback();
        }

        UpdateFootsteps();

        Block under = ProbeBlock();

        if (under == null)
        {
            return;
        }

        if (under != _standingBlock)
        {
            _standingBlock = under;
            StandingBlockChanged?.Invoke(under);
        }

        _lastStandingBlock = under;
        _lastStandingTop = under.GetTopHeight();

        _lastSafePosition = transform.position;
    }

    public Block ProbeBlock()
    {
        float feetY = GetFeetY();
        Vector3 origin = new Vector3(transform.position.x, feetY + 0.08f, transform.position.z);
        float distance = 0.08f + groundProbeDistance;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponentInParent<Block>();
        }

        return null;
    }

    public float GetFeetY()
    {
        if (_controller != null && _controller.height > 0f)
        {
            return transform.TransformPoint(_controller.center).y - _controller.height * 0.5f;
        }

        return transform.position.y;
    }

    private bool ShouldRespawn()
    {
        if (_lastStandingBlock != null && _lastStandingBlock.gameObject.activeInHierarchy)
        {
            return transform.position.y < _lastStandingTop - fallDepth;
        }

        return transform.position.y < fallRespawnY;
    }

    private void RespawnOnLastBlock()
    {
        Block block = _lastStandingBlock;

        if (block == null)
        {
            RespawnAt(_lastSafePosition);
            Debug.Log("[Agent] 掉出地图，回到上一个安全点");
            return;
        }

        Collider collider = block.GetComponentInChildren<Collider>();
        Vector3 center = collider != null ? collider.bounds.center : block.transform.position;
        float top = block.GetTopHeight();
        float feetOffset = transform.position.y - GetFeetY();
        Vector3 target = new Vector3(center.x, top + 0.06f + feetOffset, center.z);

        RespawnAt(target);
        _wasGrounded = true;
        Debug.Log($"[Agent] 掉落，回到方块 {block.name}（顶面 {top:F1}m）");
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
        _lastSafePosition = position;
    }


    private void PlayLandingFeedback()
    {
        Block block = ProbeBlock();

        if (block != null)
        {
            block.TryAdvance();

            BlockFeedback feedback = block.GetComponent<BlockFeedback>();

            if (feedback == null)
            {
                feedback = block.gameObject.AddComponent<BlockFeedback>();
            }

            feedback.Play();
        }

        PlayLandingSound();
    }

    private void UpdateFootsteps()
    {
        if (_audio == null || stepClip == null || Time.time - _lastLandingTime < 0.15f)
        {
            return;
        }

        Vector3 velocity = _controller.velocity;
        velocity.y = 0f;
        float speed = velocity.magnitude;

        if (speed < 0.3f)
        {
            return;
        }

        _stepAccumulator += speed * Time.deltaTime;

        if (_stepAccumulator < stepDistance)
        {
            return;
        }

        _stepAccumulator = 0f;
        _audio.pitch = 0.92f + UnityEngine.Random.Range(0f, 0.16f);
        _audio.PlayOneShot(stepClip, stepVolume);
    }

    private void PlayLandingSound()
    {
        if (_audio == null || landingClip == null)
        {
            return;
        }

        _audio.pitch = 0.95f + UnityEngine.Random.Range(0f, 0.1f);
        _audio.PlayOneShot(landingClip, landingVolume);
    }

    public void NotifyExternalMove()
    {
        _lastSafePosition = transform.position;
    }

    private void OnValidate()
    {
        CharacterController controller = GetComponent<CharacterController>();

        if (controller == null)
        {
            return;
        }

        float expectedCenterY = controller.height * 0.5f;
        bool offCenter = Mathf.Abs(controller.center.x) > 0.001f
                         || Mathf.Abs(controller.center.z) > 0.001f
                         || Mathf.Abs(controller.center.y - expectedCenterY) > 0.01f;

        if (offCenter)
        {
            Debug.LogWarning(
                $"[PlayerBlockAgent] CharacterController 的 center 应为 (0, {expectedCenterY:0.##}, 0)，当前 {controller.center}。",
                this);
        }
    }
}
