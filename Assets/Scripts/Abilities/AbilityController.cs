using System;
using UnityEngine;

public class AbilityController : MonoBehaviour
{
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private AbilityWallet wallet;
    [SerializeField] private CourageAbility courage;
    [SerializeField] private PatienceAbility patience;
    [SerializeField] private HopeAbility hope;
    [SerializeField] private WisdomAbility wisdom;
    [SerializeField] private float cooldown = 0.6f;

    private float _nextUseTime;

    public event Action<AbilityType> Used;
    public event Action<AbilityType, string> Rejected;

    private void Awake()
    {
        if (input == null)
        {
            input = GetComponent<PlayerInputReader>();
        }

        if (wallet == null)
        {
            wallet = GetComponent<AbilityWallet>();
        }

        if (courage == null)
        {
            courage = GetComponent<CourageAbility>();
        }

        if (patience == null)
        {
            patience = GetComponent<PatienceAbility>();
        }

        if (hope == null)
        {
            hope = GetComponent<HopeAbility>();
        }

        if (wisdom == null)
        {
            wisdom = GetComponent<WisdomAbility>();
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
            TryUse(courage);
        }

        if (input.Ability2Pressed)
        {
            TryUse(patience);
        }

        if (input.Ability3Pressed)
        {
            TryUse(hope);
        }

        if (input.Ability4Pressed)
        {
            TryUse(wisdom);
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

        if (wallet == null || !wallet.TrySpend(ability.Cost))
        {
            Rejected?.Invoke(ability.Type, "星光不足");
            return false;
        }

        ability.Execute();
        _nextUseTime = Time.time + cooldown;
        Used?.Invoke(ability.Type);
        return true;
    }
}
