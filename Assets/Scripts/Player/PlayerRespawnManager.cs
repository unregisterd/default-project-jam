using System.Collections;
using UnityEngine;

public class PlayerRespawnManager : MonoBehaviour
{
    [Header("引用（留空自动查找）")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private FirstPersonMotor motor;
    [SerializeField] private PlayerLook look;
    [SerializeField] private PlayerBlockAgent agent;
    [SerializeField] private AbilityWallet wallet;
    [SerializeField] private CheckpointTracker checkpoint;
    [SerializeField] private Hud_StarEnergy hud;

    [Header("死亡判定")]
    [Tooltip("向下探测方块的距离，没命中就判死")]
    [SerializeField] private float blockProbeLength = 80f;
    [SerializeField] private LayerMask groundMask = ~0;
    [Tooltip("绝对地板，低于它就判死")]
    [SerializeField] private float worldFloorY = -50f;

    [Header("演出")]
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private AudioClip deathClip;
    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 0.8f;

    private bool _dead;
    private AudioSource _audio;
    private EndingDirector _ending;

    public bool IsDead => _dead;

    private void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputReader>();
        if (motor == null) motor = GetComponent<FirstPersonMotor>();
        if (look == null) look = GetComponent<PlayerLook>();
        if (agent == null) agent = GetComponent<PlayerBlockAgent>();
        if (wallet == null) wallet = GetComponent<AbilityWallet>();
        if (checkpoint == null) checkpoint = GetComponent<CheckpointTracker>();
        if (hud == null) hud = GetComponent<Hud_StarEnergy>();
        _ending = FindObjectOfType<EndingDirector>();

        _audio = GetComponent<AudioSource>();

        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.spatialBlend = 0f;
        }
    }

    private void Update()
    {
        if (_dead || (checkpoint != null && !checkpoint.HasCheckpoint))
        {
            return;
        }

        if (_ending != null && _ending.IsPlaying)
        {
            return;
        }

        if (input != null && input.ResetPressed)
        {
            Die();
            return;
        }

        if (transform.position.y < worldFloorY)
        {
            Die();
            return;
        }

        if (agent != null && !agent.IsGrounded && !HasBlockBelow())
        {
            Die();
        }
    }

    private bool HasBlockBelow()
    {
        Vector3 origin = transform.position + Vector3.up * 0.2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, blockProbeLength, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponentInParent<Block>() != null;
        }

        return false;
    }

    public void Die()
    {
        if (_dead)
        {
            return;
        }

        _dead = true;
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        if (motor != null) motor.enabled = false;
        if (look != null) look.enabled = false;
        if (agent != null) agent.enabled = false;
        if (hud != null) hud.SetVisible(false);

        if (deathClip != null && _audio != null)
        {
            _audio.PlayOneShot(deathClip, deathVolume);
        }

        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.Fade(1f, fadeOutDuration);
        }

        if (UITextManager.Instance != null)
        {
            string fade = Loc.Text("death_fade");
            string back = Loc.Text("death_return");
            UITextManager.Instance.ShowText($"{fade}\n{back}", 0f, true);
        }

        yield return new WaitForSeconds(holdDuration);

        Restore();

        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.Fade(0f, fadeInDuration);
        }

        if (UITextManager.Instance != null)
        {
            UITextManager.Instance.HideText();
        }

        if (hud != null) hud.SetVisible(true);
        if (motor != null) motor.enabled = true;
        if (look != null) look.enabled = true;
        if (agent != null) agent.enabled = true;

        _dead = false;
    }

    private void Restore()
    {
        if (checkpoint == null)
        {
            return;
        }

        if (motor != null)
        {
            motor.Teleport(checkpoint.Position);
        }
        else
        {
            transform.position = checkpoint.Position;
        }

        transform.rotation = checkpoint.Rotation;

        if (look != null)
        {
            look.ResetPitch();
        }

        if (wallet == null)
        {
            wallet = GetComponent<AbilityWallet>();
        }

        if (wallet != null)
        {
            wallet.SetPoints(checkpoint.Points);
        }

        checkpoint.RestoreNpcs();

        if (agent != null)
        {
            agent.enabled = true;
            agent.NotifyExternalMove();
        }

        Debug.Log($"[Respawn] 回到检查点，点数 {checkpoint.Points}");
    }
}
