using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DungeonGenerator : MonoBehaviour
{
    public int Width = 25;
    public int Height = 25;
    public int Rows = 5;
    public int Columns = 5;
    public float InitialRockPercentage = 0.5f; // Başlangıçta ne kadar taş olacak
    public int IterationCount; // Kaç iterasyon çalışacak
    public int RockThreshold; // Bir hücre taş olması için kaç taş komşusu olmalı
    public int MooreNeighborhoodSize; // Komşuluk mesafesi (M)

    public SpriteRenderer DungeonSprite;

    private int[,,,] _grid;
    private Texture2D _texture;

    private void OnEnable()
    {
        GenerateDungeon();
    }

    private void GenerateDungeon()
    {
        _grid = new int[Rows, Columns, Width, Height];
        _texture = new Texture2D(Columns * Width, Rows * Height, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point; // Anti-aliasing kaldırıldı, keskin pikseller
        _texture.wrapMode = TextureWrapMode.Clamp;
        DungeonSprite.sprite = Sprite.Create(_texture, new Rect(0, 0, Width * Columns, Height * Rows), new Vector2(0.5f, 0.5f), 1);
        for (int row = 0; row < Rows; row++)
        {
            for (int column = 0; column < Columns; column++)
            {
                GenerateGrid(row, column);
            }
        }
    }

    private void GenerateGrid(int row, int column)
    {
        RandomFillGrid(row, column);

        for (int i = 0; i < IterationCount; i++)
        {
            ApplyCellularAutomata(row, column, i == IterationCount - 1);
        }

        DrawGrid(row, column);
    }

    private void RandomFillGrid(int row, int column)
    {
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _grid[row, column, x, y] = Random.value < InitialRockPercentage ? 1 : 0;

                if (x == 0 || x == Width - 1 || y == 0 || y == Height - 1)
                {
                    _grid[row, column, x, y] = 1;
                }
            }
        }
    }

    private void ApplyCellularAutomata(int row, int column, bool isLastIteration)
    {
        int[,] newGrid = new int[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                int neighborRocks = CountRockNeighbors(row, column, x, y);
                if (neighborRocks >= RockThreshold)
                    newGrid[x, y] = 1;
                else
                    newGrid[x, y] = 0;
            }
        }

        // newGrid'i _grid[row, column] konumuna atamak
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _grid[row, column, x, y] = newGrid[x, y];
                if (isLastIteration == false && (x == 0 || x == Width - 1 || y == 0 || y == Height - 1))
                {
                    _grid[row, column, x, y] = 1;
                }
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
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height && !(dx == 0 && dy == 0))
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
        if (_grid[row, column, x, y] == 0) return false; // Kaya değilse devam et
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                {
                    if (_grid[row, column, nx, ny] == 0) return true; // Yanında kaya varsa duvar olmalı
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
                Color color = Color.black; // Varsayılan: rock
                if (_grid[row, column, x, y] == 0)
                    color = Color.white; // Floor
                else if (IsWall(row, column, x, y))
                    color = Color.gray; // Duvar

                _texture.SetPixel(column * Width + x, row * Height + y, color);
            }
        }
        _texture.Apply();
    }
}