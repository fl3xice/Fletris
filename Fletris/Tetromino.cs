using SFML.System;

namespace Fletris;

public enum TetrominoType
{
    O, I, T, S, Z, L, J
}

public class Tetromino
{
    private static readonly Vector2i[][] Shapes =
    [
        [new Vector2i(0, 0), new Vector2i(1, 0), new Vector2i(0, 1), new Vector2i(1, 1)], // O
        [new Vector2i(0, 0), new Vector2i(-1, 0), new Vector2i(1, 0), new Vector2i(2, 0)], // I
        [new Vector2i(0, 0), new Vector2i(-1, 0), new Vector2i(1, 0), new Vector2i(0, 1)], // T
        [new Vector2i(0, 0), new Vector2i(1, 0), new Vector2i(0, 1), new Vector2i(-1, 1)], // S
        [new Vector2i(0, 0), new Vector2i(-1, 0), new Vector2i(0, 1), new Vector2i(1, 1)], // Z
        [new Vector2i(0, 0), new Vector2i(-1, 0), new Vector2i(-1, 1), new Vector2i(1, 0)], // L
        [new Vector2i(0, 0), new Vector2i(1, 0), new Vector2i(1, 1), new Vector2i(-1, 0)] // J
    ];

    public Vector2i Position { get; set; }
    public Vector2i[] Cells { get; set; }
    public MinoColor Color { get; init; }
    public TetrominoType Type { get; }
    
    public Tetromino()
    {
        var rand = new Random();
        var shapeIndex = rand.Next(Shapes.Length);
        Cells = (Vector2i[])Shapes[shapeIndex].Clone();
        Color = (MinoColor)shapeIndex;
        Type = (TetrominoType)shapeIndex;
        Position = new Vector2i(5, 0); // Start position at the top middle
    }

    public void Rotate()
    {
        for (var i = 0; i < Cells.Length; i++)
        {
            Cells[i] = new Vector2i(-Cells[i].Y, Cells[i].X);
        }
    }
}