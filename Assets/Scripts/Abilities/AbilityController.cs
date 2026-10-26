using System;
using UnityEngine;

[DisallowMultipleComponent]
public class AbilityController : MonoBehaviour
{
    [SerializeField] private PlayerInputReader input;

    [Header("星光池")]
    [SerializeField] private int maxPoints = 4;
    [SerializeField] private int startPoints;
    [Tooltip("每个技能的全局冷却")]
    [SerializeField] private float cooldown = 0.6f;

    [Header("勇气")]
    [SerializeField] private float courageHeight = 3f;
    [SerializeField] private float courageForwardSpeed = 2f;

    [Header("耐心")]
    [SerializeField] private float patienceRiseHeight = 2f;
    [SerializeField] private float patienceDuration = 1f;

    [Header("希望")]
    [SerializeField] private GameObject hopePlatformPrefab;
    [SerializeField] private BlockType hopePlatformType;
    [SerializeField] private float hopeForwardDistance = 2.5f;
    [SerializeField] private float hopeUpOffset = 0.8f;
    [SerializeField] private float hopeLifetime = 10f;
    [SerializeField] private float hopeBlinkTime = 2f;

    [Header("智慧")]
    [SerializeField] private float wisdomRadius = 12f;

    [Header("方块外观")]
    [Tooltip("希望平台 / 隐藏块用的调色板")]
    [SerializeField] private BlockPalette palette;

    [Header("调试")]
    [Tooltip("自动挂一个临时 HUD，方便看星光数")]
    [SerializeField] private bool showDebugHud = true;

    private AbilityWallet _wallet;
    private CourageAbility _courage;
    private PatienceAbility _patience;
    private HopeAbility _hope;
    private WisdomAbility _wisdom;
    private float _nextUseTime;

    public AbilityWallet Wallet => _wallet;

    public event Action<AbilityType> Used;
    public event Action<AbilityType, string> Rejected;

    private void Awake()
    {
        if (input == null)
        {
            input = GetComponent<PlayerInputReader>();
        }

        _wallet = EnsureComponent(_wallet);
        _wallet.Configure(maxPoints, startPoints);

        _courage = EnsureComponent(_courage);
        _courage.Configure(courageHeight, courageForwardSpeed);

        _patience = EnsureComponent(_patience);
        _patience.Configure(patienceRiseHeight, patienceDuration);

        _hope = EnsureComponent(_hope);
        _hope.Configure(hopePlatformPrefab, hopePlatformType, palette, hopeForwardDistance, hopeUpOffset, hopeLifetime, hopeBlinkTime);

        _wisdom = EnsureComponent(_wisdom);
        _wisdom.Configure(wisdomRadius);

        if (showDebugHud && GetComponent<Hud_StarEnergy>() == null)
        {
            gameObject.AddComponent<Hud_StarEnergy>();
        }
    }

    private void Update()
    {
        if (input == null)
        {
            return;
        }

        if (input.Ability1Pressed)
        {
            TryUse(_courage);
        }

        if (input.Ability2Pressed)
        {
            TryUse(_patience);
        }

        if (input.Ability3Pressed)
        {
            TryUse(_hope);
        }

        if (input.Ability4Pressed)
        {
            TryUse(_wisdom);
        }
    }

    public bool TryUse(IAbility ability)
    {
        if (ability == null)
        {
            return false;
        }

        if (Time.time < _nextUseTime)
        {
            Rejected?.Invoke(ability.Type, "冷却中");
            return false;
        }

        if (!ability.CanUse(out string reason))
        {
            Rejected?.Invoke(ability.Type, reason);
            return false;
        }

        if (_wallet == null || !_wallet.TrySpend(ability.Cost))
        {
            Rejected?.Invoke(ability.Type, "星光不足");
            return false;
        }

        ability.Execute();
        _nextUseTime = Time.time + cooldown;
        Used?.Invoke(ability.Type);
        return true;
    }

    private T EnsureComponent<T>(T current) where T : Component
    {
        if (current != null)
        {
            return current;
        }

        T found = GetComponent<T>();

        if (found != null)
        {
            return found;
        }

        return gameObject.AddComponent<T>();
    }
}
