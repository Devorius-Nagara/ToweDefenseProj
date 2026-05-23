using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public int Gold           { get; private set; }
    public int AttackerBudget { get; private set; }

    private int budgetIncreasePerRound;

    void Awake() => Instance = this;

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        UIManager.Instance?.RefreshHUD();
        return true;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        Gold += amount;
        StatisticsManager.Instance?.OnGoldEarned(amount);
        AudioManager.Instance?.PlayGold();
        UIManager.Instance?.RefreshHUD();
    }

    public void IncreaseAttackerBudget() => AttackerBudget += budgetIncreasePerRound;

    public void Reset(int startGold, int startBudget, int budgetIncrease)
    {
        Gold                   = startGold;
        AttackerBudget         = startBudget;
        budgetIncreasePerRound = budgetIncrease;
        UIManager.Instance?.RefreshHUD();
    }
}
