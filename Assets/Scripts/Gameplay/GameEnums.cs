public enum GameState
{
    MainMenu,
    Preparation,
    Battle,
    RoundEnd,
    Victory,
    Defeat
}

public enum TowerType
{
    Archer,
    Mage,
    Freezer,
    Cannon
}

public enum TargetingMode
{
    FirstInPath,   // closest to base (default)
    LastInPath,
    LowestHP,
    HighestHP
}
