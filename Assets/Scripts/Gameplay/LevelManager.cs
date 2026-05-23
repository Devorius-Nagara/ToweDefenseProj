using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds the three built-in level configurations and tracks which one is selected.
/// Persists across scene reloads via DontDestroyOnLoad.
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public int CurrentLevelIndex { get; private set; } = 1; // 1-based

    // ── Level definitions ─────────────────────────────────────────────

    public static readonly LevelConfig[] Levels =
    {
        new LevelConfig
        {
            Index        = 1,
            Name         = "Forest Path",
            Description  = "A gentle straight road through the woods.",
            Difficulty   = 1,
            GridCols     = 12, GridRows = 8,
            PathCorners  = new[]
            {
                new Vector2Int(0, 3), new Vector2Int(11, 3)   // simple straight
            },
            MaxRounds      = 8,
            StartGold      = 400,
            StartBudget    = 150,
            BudgetIncrease = 20,
            PrepDuration   = 35f,
        },
        new LevelConfig
        {
            Index        = 2,
            Name         = "Crossroads",
            Description  = "A winding path with two sharp turns.",
            Difficulty   = 2,
            GridCols     = 12, GridRows = 8,
            PathCorners  = new[]
            {
                new Vector2Int(0, 4), new Vector2Int(3, 4),
                new Vector2Int(3, 1), new Vector2Int(7, 1),
                new Vector2Int(7, 6), new Vector2Int(11, 6)
            },
            MaxRounds      = 10,
            StartGold      = 300,
            StartBudget    = 200,
            BudgetIncrease = 30,
            PrepDuration   = 30f,
        },
        new LevelConfig
        {
            Index        = 3,
            Name         = "Labyrinth",
            Description  = "A maze of corridors — good luck!",
            Difficulty   = 3,
            GridCols     = 12, GridRows = 8,
            PathCorners  = new[]
            {
                new Vector2Int(0, 6),  new Vector2Int(2, 6),
                new Vector2Int(2, 2),  new Vector2Int(5, 2),
                new Vector2Int(5, 5),  new Vector2Int(8, 5),
                new Vector2Int(8, 1),  new Vector2Int(11, 1)
            },
            MaxRounds      = 12,
            StartGold      = 250,
            StartBudget    = 260,
            BudgetIncrease = 40,
            PrepDuration   = 25f,
        }
    };

    public LevelConfig Current => Levels[CurrentLevelIndex - 1];

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectLevel(int index)
    {
        CurrentLevelIndex = Mathf.Clamp(index, 1, Levels.Length);
    }

    /// <summary>Reloads the scene so GameBootstrap rebuilds with the new level.</summary>
    public void LoadSelectedLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

// ── Plain data struct (no MonoBehaviour) ──────────────────────────────
public struct LevelConfig
{
    public int          Index;
    public string       Name;
    public string       Description;
    public int          Difficulty;       // 1, 2, 3
    public int          GridCols, GridRows;
    public Vector2Int[] PathCorners;
    public int          MaxRounds;
    public int          StartGold;
    public int          StartBudget;
    public int          BudgetIncrease;
    public float        PrepDuration;

    public string DifficultyStars => Difficulty switch { 1 => "★☆☆", 2 => "★★☆", _ => "★★★" };
}
