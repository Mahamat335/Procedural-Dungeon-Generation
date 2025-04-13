using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public int Width = 50;
    public int Height = 50;
    public int Rows = 3;
    public int Columns = 3;
    public float InitialRockPercentage = 0.5f;
    public int IterationCount = 4;
    public int RockThreshold = 5;
    public int MooreNeighborhoodSize = 1;

    public SpriteRenderer DungeonSprite;

    private int[,,,] _grid;
    private Texture2D _texture;
    private Dictionary<Vector2Int, int> _gridSeeds = new();

    private void OnEnable()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        _grid = new int[Rows, Columns, Width, Height];
        _texture = new Texture2D(Columns * Width, Rows * Height, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;
        DungeonSprite.sprite = Sprite.Create(_texture, new Rect(0, 0, Width * Columns, Height * Rows), new Vector2(0.5f, 0.5f), 1);

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Columns; col++)
            {
                GenerateGrid(row, col);
            }
        }

        _texture.Apply();
    }

    private void GenerateGrid(int row, int column)
    {
        Vector2Int key = new Vector2Int(row, column);
        if (!_gridSeeds.ContainsKey(key))
            _gridSeeds[key] = Random.Range(0, int.MaxValue);

        Random.InitState(_gridSeeds[key]);
        RandomFillGrid(row, column);

        for (int i = 0; i < IterationCount; i++)
        {
            ApplyCellularAutomata(row, column);
        }

        DrawGrid(row, column);
    }

    private void RandomFillGrid(int row, int column)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                    _grid[row, column, x, y] = 1; // Kenarları her zaman kaya yap
                else
                    _grid[row, column, x, y] = Random.value < InitialRockPercentage ? 1 : 0;
            }
        }
    }

    private void ApplyCellularAutomata(int row, int column)
    {
        int[,] newGrid = new int[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int rockCount = CountRockNeighbors(row, column, x, y);
                newGrid[x, y] = rockCount >= RockThreshold ? 1 : 0;
            }
        }

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _grid[row, column, x, y] = newGrid[x, y];

                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                    _grid[row, column, x, y] = 1;
            }
        }
    }

    private int CountRockNeighbors(int row, int column, int x, int y)
    {
        int count = 0;
        for (int dx = -MooreNeighborhoodSize; dx <= MooreNeighborhoodSize; dx++)
        {
            for (int dy = -MooreNeighborhoodSize; dy <= MooreNeighborhoodSize; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                {
                    if (_grid[row, column, nx, ny] == 1)
                        count++;
                }
            }
        }
        return count;
    }

    private bool IsWall(int row, int column, int x, int y)
    {
        if (_grid[row, column, x, y] == 0) return false;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                {
                    if (_grid[row, column, nx, ny] == 0)
                        return true;
                }
            }
        }

        return false;
    }

    private void DrawGrid(int row, int column)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Color color;
                if (_grid[row, column, x, y] == 0)
                    color = Color.white;
                else if (IsWall(row, column, x, y))
                    color = Color.gray;
                else
                    color = Color.black;

                _texture.SetPixel(column * Width + x, row * Height + y, color);
            }
        }
    }
}
