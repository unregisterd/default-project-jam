using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawnManager : MonoBehaviour
{
    [Header("引用（留空自动查找）")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private FirstPersonMotor motor;
    [SerializeField] private PlayerLook look;
    [SerializeField] private PlayerBlockAgent agent;
    [SerializeField] private Hud_StarEnergy hud;

    [Header("死亡判定")]
    [Tooltip("向下探测方块的距离，没命中就判死")]
    [SerializeField] private float blockProbeLength = 80f;
    [SerializeField] private LayerMask groundMask = ~0;
    [Tooltip("绝对地板，低于它就判死")]
    [SerializeField] private float worldFloorY = -50f;

    [Header("死亡演出")]
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private AudioClip deathClip;
    [Range(0f, 1f)]
    [SerializeField] private float deathVolume = 0.8f;

    [Header("开场演出")]
    [SerializeField] private bool playIntro = true;
    [SerializeField] private float openingHold = 2.5f;
    [SerializeField] private float narrationHold = 4.5f;
    [SerializeField] private float hintHold = 2.5f;
    [SerializeField] private float introFadeIn = 1.2f;

    private bool _busy;
    private bool _hasBeenGrounded;
    private float _controlEnabledAt;
    private float _spawnGrace = 3f;
    private AudioSource _audio;
    private EndingDirector _ending;

    public bool IsBusy => _busy;

    private void Awake()
    {
        if (input == null) input = GetComponent<PlayerInputReader>();
        if (motor == null) motor = GetComponent<FirstPersonMotor>();
        if (look == null) look = GetComponent<PlayerLook>();
        if (agent == null) agent = GetComponent<PlayerBlockAgent>();
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

    private void Start()
    {
        _controlEnabledAt = Time.time;

        if (playIntro)
        {
            StartCoroutine(IntroRoutine());
        }
    }

    private void Update()
    {
        if (_busy)
        {
            return;
        }

        if (_ending != null && _ending.IsPlaying)
        {
            return;
        }

        if (input != null && input.ResetPressed)
        {
            StartCoroutine(DeathRoutine());
            return;
        }

        if (transform.position.y < worldFloorY)
        {
            StartCoroutine(DeathRoutine());
            return;
        }

        if (agent != null && agent.IsGrounded)
        {
            _hasBeenGrounded = true;
        }

        if (agent != null && !agent.IsGrounded && !HasBlockBelow() && Time.time - _controlEnabledAt > _spawnGrace)
        {
            if (!_hasBeenGrounded && Time.time - _controlEnabledAt < _spawnGrace * 3f)
            {
                Debug.LogError("[Respawn] 出生点下方没有任何方块，玩家一直在下落。请检查 Player 与出生台的坐标。");
                _busy = true;
                return;
            }

            StartCoroutine(DeathRoutine());
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

    private IEnumerator IntroRoutine()
    {
        _busy = true;
        SetControlEnabled(false);

        if (hud != null) hud.SetVisible(false);

        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.SetAlpha(1f);
        }

        ShowLine("intro_opening", 2f);
        yield return new WaitForSeconds(openingHold);

        ShowLine("intro_narration", 2f);
        yield return new WaitForSeconds(narrationHold);

        ShowLine("intro_hint", 2f);
        yield return new WaitForSeconds(hintHold);

        if (UITextManager.Instance != null)
        {
            UITextManager.Instance.HideText();
        }

        if (ScreenFader.Instance != null)
        {
            yield return ScreenFader.Instance.Fade(0f, introFadeIn);
        }

        if (hud != null) hud.SetVisible(true);
        SetControlEnabled(true);
        _controlEnabledAt = Time.time;
        _hasBeenGrounded = false;
        _busy = false;
    }

    private IEnumerator DeathRoutine()
    {
        if (_busy)
        {
            yield break;
        }

        _busy = true;
        SetControlEnabled(false);

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
            string line = $"{Loc.Text("death_fade")}\n{Loc.Text("death_return")}";
            UITextManager.Instance.ShowText(line, 0f, true);
        }

        yield return new WaitForSeconds(holdDuration);

        Scene scene = SceneManager.GetActiveScene();

        if (scene.buildIndex >= 0)
        {
            SceneManager.LoadScene(scene.buildIndex);
        }
        else
        {
            Debug.LogError("[Respawn] 当前场景不在 Build Settings 里，无法重开。请把 Main.unity 加入构建设置。");
            _busy = false;
        }
    }

    private void ShowLine(string id, float typeSpeed)
    {
        if (UITextManager.Instance == null)
        {
            return;
        }

        string speaker = Loc.Speaker(id);
        string text = Loc.Text(id);
        string content = string.IsNullOrEmpty(speaker) ? text : $"{speaker}\n{text}";

        UITextManager.Instance.ShowText(content, 0f, true);
    }

    private void SetControlEnabled(bool enabled)
    {
        if (motor != null) motor.enabled = enabled;
        if (look != null) look.enabled = enabled;
        if (agent != null) agent.enabled = enabled;
    }
}
