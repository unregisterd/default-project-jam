using TMPro;
using UnityEngine;

public class NpcSymbol : MonoBehaviour
{
    [Header("奖励")]
    [SerializeField] private AbilityType ability = AbilityType.Courage;
    [SerializeField] private int points = 2;

    [Header("文案")]
    [Tooltip("NPC 台词 id，例如 npc_light_line")]
    [SerializeField] private string dialogueId = "npc_light_line";
    [Tooltip("能力说明 id，例如 ability_courage_desc；留空则不显示")]
    [SerializeField] private string abilityDescId = "ability_courage_desc";

    [Header("触发")]
    [SerializeField] private float range = 3.5f;
    [SerializeField] private float retryInterval = 0.5f;

    [Header("表现")]
    [SerializeField] private float worldTextHeight = 2.2f;
    [SerializeField] private float worldTextWidth = 6f;
    [SerializeField] private bool createRipples = true;

    private AbilityWallet _wallet;
    private Transform _player;
    private TextMeshPro _worldText;
    private RippleEmitter _ripple;
    private bool _inRange;
    private bool _claimed;
    private float _nextRetryTime;
    private string _promptLine = string.Empty;

    private void Awake()
    {
        _wallet = FindObjectOfType<AbilityWallet>();
        PlayerBlockAgent agent = FindObjectOfType<PlayerBlockAgent>();

        if (agent != null)
        {
            _player = agent.transform;
        }

        CreateWorldText();

        if (createRipples)
        {
            _ripple = gameObject.GetComponent<RippleEmitter>();

            if (_ripple == null)
            {
                _ripple = gameObject.AddComponent<RippleEmitter>();
            }
        }
    }

    private void Update()
    {
        if (_player == null)
        {
            return;
        }

        bool inRange = Vector3.Distance(_player.position, transform.position) <= range;

        if (inRange != _inRange)
        {
            _inRange = inRange;

            if (inRange)
            {
                ShowText();
            }
            else
            {
                HideText();
            }
        }

        if (!_inRange)
        {
            return;
        }

        UpdateWorldTextFacing();

        if (!_claimed && Time.time >= _nextRetryTime)
        {
            _nextRetryTime = Time.time + retryInterval;
            TryClaim();
        }
    }

    private void TryClaim()
    {
        if (_wallet == null)
        {
            return;
        }

        int free = _wallet.MaxPoints - _wallet.Points;

        if (free < points)
        {
            _promptLine = Loc.Language == "en" ? "Star Energy is full." : "星光已满，先用掉一些吧";
            ShowText();
            return;
        }

        _wallet.Add(points);
        _claimed = true;
        _promptLine = Loc.Language == "en" ? $"+{points} Star Energy" : $"+{points} 点星光";
        ShowText();

        Debug.Log($"[NPC] 领取 {ability} +{points}（当前 {_wallet.Points}/{_wallet.MaxPoints}）");
    }

    private void ShowText()
    {
        string speaker = Loc.Speaker(dialogueId);
        string line = Loc.Text(dialogueId);
        string desc = string.IsNullOrEmpty(abilityDescId) ? string.Empty : Loc.Text(abilityDescId);

        if (!string.IsNullOrEmpty(desc))
        {
            line += "\n" + desc;
        }

        if (!string.IsNullOrEmpty(_promptLine))
        {
            line += "\n" + _promptLine;
        }

        NpcPromptHud.Show(speaker, line);

        if (_worldText != null)
        {
            string header = string.IsNullOrEmpty(speaker) ? string.Empty : $"<b>{speaker}</b>\n";
            _worldText.text = header + Loc.Text(dialogueId);
            _worldText.gameObject.SetActive(true);
        }
    }

    private void HideText()
    {
        _promptLine = string.Empty;
        NpcPromptHud.Clear(Loc.Speaker(dialogueId));

        if (_worldText != null)
        {
            _worldText.gameObject.SetActive(false);
        }
    }

    private void CreateWorldText()
    {
        GameObject textObject = new GameObject("WorldText");
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = Vector3.up * worldTextHeight;

        _worldText = textObject.AddComponent<TextMeshPro>();
        _worldText.alignment = TextAlignmentOptions.Center;
        _worldText.fontSize = 3f;
        _worldText.enableWordWrapping = true;
        _worldText.rectTransform.sizeDelta = new Vector2(worldTextWidth, 3f);
        _worldText.color = Color.black;
        _worldText.outlineWidth = 0.25f;
        _worldText.outlineColor = Color.white;
        _worldText.raycastTarget = false;

        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts/SourceHanSansCN SDF");

        if (font == null)
        {
            font = TMP_Settings.defaultFontAsset;
        }

        if (font != null)
        {
            _worldText.font = font;
        }

        _worldText.gameObject.SetActive(false);
    }

    private void UpdateWorldTextFacing()
    {
        Camera camera = Camera.main;

        if (_worldText != null && camera != null && _worldText.gameObject.activeSelf)
        {
            _worldText.transform.rotation = camera.transform.rotation;
        }
    }

    private void OnValidate()
    {
        points = Mathf.Max(1, points);
        range = Mathf.Max(1f, range);
    }
}
