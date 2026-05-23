using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int Columns = 12;
    public int Rows = 8;
    public float CellSize = 1f;
    public Vector2 Origin = new Vector2(-6f, -4f);

    private CellState[,] grid;
    private GameObject[,] cellObjects;
    private GameObject[,] towerObjects;

    // Pre-generated medieval tile sprites (varied per cell for organic look)
    private Sprite[,] grassSprites;
    private Sprite[,] cobbleSprites;

    void Awake() => Instance = this;

    public void Initialize(HashSet<Vector2Int> pathCells)
    {
        grid        = new CellState[Columns, Rows];
        cellObjects = new GameObject[Columns, Rows];
        towerObjects = new GameObject[Columns, Rows];

        // Pre-generate unique tile sprites for each cell
        grassSprites  = new Sprite[Columns, Rows];
        cobbleSprites = new Sprite[Columns, Rows];
        for (int col = 0; col < Columns; col++)
        for (int row = 0; row < Rows; row++)
        {
            int seed = col * 100 + row;
            grassSprites[col, row]  = SpriteFactory.CreateMedievalGrass(seed, 64);
            cobbleSprites[col, row] = SpriteFactory.CreateMedievalCobblestone(seed, 64);
        }

        for (int col = 0; col < Columns; col++)
        for (int row = 0; row < Rows; row++)
        {
            bool isPath = pathCells.Contains(new Vector2Int(col, row));
            grid[col, row] = isPath ? CellState.Path : CellState.Empty;

            var cell = new GameObject($"Cell_{col}_{row}");
            cell.transform.parent   = transform;
            cell.transform.position = GetCellCenter(col, row);

            var sr = cell.AddComponent<SpriteRenderer>();
            sr.sprite = isPath ? cobbleSprites[col, row] : grassSprites[col, row];
            sr.sortingOrder = -1;
            // Scale = 1.0 — no gap, no visible grid lines
            cell.transform.localScale = Vector3.one * CellSize;

            cellObjects[col, row] = cell;
        }
    }

    public Vector3 GetCellCenter(int col, int row)
        => new Vector3(Origin.x + col * CellSize + CellSize * 0.5f,
                       Origin.y + row * CellSize + CellSize * 0.5f, 0f);

    public bool WorldToCell(Vector3 worldPos, out int col, out int row)
    {
        col = Mathf.FloorToInt((worldPos.x - Origin.x) / CellSize);
        row = Mathf.FloorToInt((worldPos.y - Origin.y) / CellSize);
        return col >= 0 && col < Columns && row >= 0 && row < Rows;
    }

    public bool CanPlaceTower(int col, int row)
        => col >= 0 && col < Columns && row >= 0 && row < Rows
           && grid[col, row] == CellState.Empty;

    public void PlaceTower(int col, int row, GameObject tower)
    {
        grid[col, row] = CellState.Occupied;
        towerObjects[col, row] = tower;
        tower.transform.position = GetCellCenter(col, row);
        // No visual change needed — tower sprite covers the cell
    }

    public void RemoveTower(int col, int row)
    {
        if (towerObjects[col, row] != null)
        {
            Destroy(towerObjects[col, row]);
            towerObjects[col, row] = null;
        }
        grid[col, row] = CellState.Empty;
    }

    /// <summary>No-op — hover colour change removed by design.</summary>
    public void SetHover(int col, int row, bool canPlace) { }

    /// <summary>No-op — hover colour change removed by design.</summary>
    public void ClearHover(int col, int row) { }

    public void ClearAllTowers()
    {
        for (int col = 0; col < Columns; col++)
        for (int row = 0; row < Rows; row++)
            if (grid[col, row] == CellState.Occupied)
                RemoveTower(col, row);
    }

    private bool IsValid(int col, int row)
        => col >= 0 && col < Columns && row >= 0 && row < Rows;

    public enum CellState { Empty, Path, Occupied }
}
