using System;
using UnityEngine;

public class AbilityWallet : MonoBehaviour
{
    [Tooltip("同时持有的上限")]
    [SerializeField] private int maxPoints = 4;
    [SerializeField] private int startPoints;

    public int Points { get; private set; }
    public int MaxPoints => maxPoints;

    public event Action<int, int> Changed;
    public event Action<int> Overflowed;

    private void Awake()
    {
        Points = Mathf.Clamp(startPoints, 0, maxPoints);
        Changed?.Invoke(Points, maxPoints);
    }

    public int Add(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int accepted = Mathf.Min(amount, maxPoints - Points);

        if (accepted <= 0)
        {
            Overflowed?.Invoke(amount);
            return 0;
        }

        Points += accepted;

        if (accepted < amount)
        {
            Overflowed?.Invoke(amount - accepted);
        }

        Changed?.Invoke(Points, maxPoints);
        return accepted;
    }

    public bool CanAfford(int cost)
    {
        return Points >= cost;
    }

    public bool TrySpend(int cost)
    {
        if (cost <= 0 || Points < cost)
        {
            return false;
        }

        Points -= cost;
        Changed?.Invoke(Points, maxPoints);
        return true;
    }

    public void Configure(int newMaxPoints, int newStartPoints)
    {
        maxPoints = Mathf.Max(1, newMaxPoints);
        startPoints = newStartPoints;
        Points = Mathf.Clamp(startPoints, 0, maxPoints);
        Changed?.Invoke(Points, maxPoints);
    }

    public void SetPoints(int value)
    {
        Points = Mathf.Clamp(value, 0, maxPoints);
        Changed?.Invoke(Points, maxPoints);
    }
}
