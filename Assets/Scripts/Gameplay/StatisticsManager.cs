using UnityEngine;

/// <summary>
/// Tracks in-session stats and persists all-time records via PlayerPrefs.
/// </summary>
public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance { get; private set; }

    // ── Session (resets each game) ────────────────────────────────────
    public int SessionKills        { get; private set; }
    public int SessionGoldEarned   { get; private set; }
    public int SessionTowersPlaced { get; private set; }
    public int SessionTowersSold   { get; private set; }

    // ── All-time (PlayerPrefs) ────────────────────────────────────────
    public int TotalGamesPlayed => PlayerPrefs.GetInt("s_games",  0);
    public int TotalWins        => PlayerPrefs.GetInt("s_wins",   0);
    public int TotalKills       => PlayerPrefs.GetInt("s_kills",  0);
    public int TotalGoldEarned  => PlayerPrefs.GetInt("s_gold",   0);
    public int HighestRound     => PlayerPrefs.GetInt("s_round",  0);
    public float WinRate => TotalGamesPlayed > 0
        ? TotalWins / (float)TotalGamesPlayed * 100f : 0f;

    void Awake() => Instance = this;

    // ── Called by gameplay systems ────────────────────────────────────
    public void OnEnemyKilled()          => SessionKills++;
    public void OnGoldEarned(int amount) => SessionGoldEarned  += amount;
    public void OnTowerPlaced()          => SessionTowersPlaced++;
    public void OnTowerSold()            => SessionTowersSold++;

    public void OnGameEnd(bool won, int round)
    {
        PlayerPrefs.SetInt("s_games", TotalGamesPlayed + 1);
        if (won) PlayerPrefs.SetInt("s_wins", TotalWins + 1);
        PlayerPrefs.SetInt("s_kills", TotalKills + SessionKills);
        PlayerPrefs.SetInt("s_gold",  TotalGoldEarned + SessionGoldEarned);
        if (round > HighestRound) PlayerPrefs.SetInt("s_round", round);
        PlayerPrefs.Save();
    }

    public void ResetSession()
    {
        SessionKills        = 0;
        SessionGoldEarned   = 0;
        SessionTowersPlaced = 0;
        SessionTowersSold   = 0;
    }

    public void ResetAllTimeStats()
    {
        foreach (var k in new[] { "s_games", "s_wins", "s_kills", "s_gold", "s_round" })
            PlayerPrefs.DeleteKey(k);
        PlayerPrefs.Save();
    }
}
