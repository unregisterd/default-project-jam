public interface IAbility
{
    AbilityType Type { get; }
    int Cost { get; }

    bool CanUse(out string reason);
    void Execute();
}
