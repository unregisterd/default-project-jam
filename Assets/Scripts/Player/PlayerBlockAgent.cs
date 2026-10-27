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
    [Tooltip("掉到这个高度以下就回到上一个安全位置")]
    [SerializeField] private float fallRespawnY = -12f;

    [Header("落地音效")]
    [Tooltip("留空则从 Resources/Audio/step_land 自动加载")]
    [SerializeField] private AudioClip landingClip;
    [Range(0f, 1f)]
    [SerializeField] private float landingVolume = 0.7f;

    private CharacterController _controller;
    private Block _standingBlock;
    private Vector3 _lastSafePosition;
    private AudioSource _audio;
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
    }

    private void Update()
    {
        if (transform.position.y < fallRespawnY)
        {
            RespawnAt(_lastSafePosition);
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
            PlayLandingFeedback();
        }

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
