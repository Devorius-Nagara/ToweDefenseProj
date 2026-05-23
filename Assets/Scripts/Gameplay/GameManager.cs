using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.MainMenu;

    public int CurrentRound { get; private set; }
    public int MaxRounds    { get; private set; }
    public int BaseHP       { get; private set; }
    public int MaxBaseHP    { get; private set; }

    private float prepTimer;
    private float prepDuration;

    void Awake() => Instance = this;

    void Start()
    {
        // UI is built next frame via GameBootstrap coroutine.
        // Kick off menu music once AudioManager is ready (it's also built in Awake).
        AudioManager.Instance?.PlayMenuMusic();
    }

    // ── Public API ────────────────────────────────────────────────────

    public void StartGame()
    {
        var lvl = LevelManager.Instance.Current;
        MaxRounds    = lvl.MaxRounds;
        MaxBaseHP    = 20;
        prepDuration = lvl.PrepDuration;

        CurrentRound = 0;
        BaseHP       = MaxBaseHP;

        EconomyManager.Instance.Reset(lvl.StartGold, lvl.StartBudget, lvl.BudgetIncrease);
        StatisticsManager.Instance?.ResetSession();

        BeginNextRound();
    }

    public void StartBattle()
    {
        if (State != GameState.Preparation) return;
        SetState(GameState.Battle);
        AudioManager.Instance?.PlayWaveStart();
        UIManager.Instance?.OnBattleStart();
        EnemySpawner.Instance.SpawnWave(EnemySpawner.Instance.PendingWave);
    }

    public void OnWaveComplete()
    {
        if (State != GameState.Battle) return;
        EconomyManager.Instance.IncreaseAttackerBudget();

        if (CurrentRound >= MaxRounds) { SetState(GameState.Victory); return; }

        SetState(GameState.RoundEnd);
        UIManager.Instance?.OnRoundEnd(CurrentRound, BaseHP, EconomyManager.Instance.Gold);
    }

    public void NextRound()
    {
        if (State == GameState.RoundEnd) BeginNextRound();
    }

    public void DamageBase(int amount)
    {
        if (State != GameState.Battle) return;
        BaseHP = Mathf.Max(0, BaseHP - amount);
        UIManager.Instance?.RefreshHUD();
        if (BaseHP <= 0) SetState(GameState.Defeat);
    }

    public void RestartGame()
    {
        EnemySpawner.Instance.ClearAll();
        GridManager.Instance.ClearAllTowers();
        TowerPlacer.Instance?.ClearPlacedData();
        StartGame();
    }

    public void GoToMainMenu()
    {
        EnemySpawner.Instance.ClearAll();
        GridManager.Instance.ClearAllTowers();
        TowerPlacer.Instance?.ClearPlacedData();
        PlayerPrefs.Save(); // flush IndexedDB in WebGL
        SetState(GameState.MainMenu);
    }

    // ── Internal ──────────────────────────────────────────────────────

    private void BeginNextRound()
    {
        CurrentRound++;
        SetState(GameState.Preparation);
        prepTimer = prepDuration;
        UIManager.Instance?.OnPreparationStart(CurrentRound);

        var wave = AIWaveBuilder.Instance.BuildWave(
            EconomyManager.Instance.AttackerBudget, CurrentRound);
        EnemySpawner.Instance.PendingWave = wave;
        UIManager.Instance?.ShowWaveInfo(wave.Count, EconomyManager.Instance.AttackerBudget);
    }

    void Update()
    {
        if (State == GameState.Preparation)
        {
            prepTimer -= Time.deltaTime;
            UIManager.Instance?.SetPrepTimer(prepTimer);
            if (prepTimer <= 0f) StartBattle();
        }
    }

    private void SetState(GameState s)
    {
        State = s;

        // ── Music transitions ─────────────────────────────────────────
        switch (s)
        {
            case GameState.MainMenu:
                AudioManager.Instance?.PlayMenuMusic();
                break;
            case GameState.Preparation:
            case GameState.Battle:
            case GameState.RoundEnd:
                AudioManager.Instance?.PlayGameplayMusic();
                break;
            case GameState.Victory:
            case GameState.Defeat:
                AudioManager.Instance?.StopMusic();
                break;
        }

        UIManager.Instance?.OnStateChanged(s);
    }
}
