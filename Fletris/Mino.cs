using SFML.Graphics;
using SFML.System;

namespace Fletris;

public enum MinoColor
{
    Orange,
    Green,
    Yellow,
    Purple,
    LightBlue,
    Blue,
    Cyan,
    Dark,
    Preview,
}


public class Mino : Drawable
{
    private static readonly Image _image = new Image("Assets/main.png");
    private static readonly Dictionary<MinoColor, Texture> _textures = new()
    {
        { MinoColor.Orange, new Texture(_image, new IntRect(0, 0, 5, 5))},
        { MinoColor.Green, new Texture(_image, new IntRect(5, 0, 5, 5))},
        { MinoColor.Yellow, new Texture(_image, new IntRect(10, 0, 5, 5))},
        { MinoColor.LightBlue, new Texture(_image, new IntRect(15, 0, 5, 5))},
        { MinoColor.Purple, new Texture(_image, new IntRect(20, 0, 5, 5))},
        { MinoColor.Blue, new Texture(_image, new IntRect(25, 0, 5, 5))},
        { MinoColor.Cyan, new Texture(_image, new IntRect(30, 0, 5, 5))},
        { MinoColor.Dark, new Texture(_image, new IntRect(35, 0, 5, 5))},
        { MinoColor.Preview, new Texture(_image, new IntRect(40, 0, 5, 5))},
    };

    private readonly RectangleShape _shape;
    private MinoColor _color;

    public MinoColor Color
    {
        get => _color;
        set
        {
            _color = value;
            _shape.Texture = _textures[value];
        }
    }

    public Vector2f Position
    {
        get => _shape.Position;
        set => _shape.Position = value;
    }

    public Mino(MinoColor color, Vector2f position)
    {
        _shape = new RectangleShape
        {
            Size = new Vector2f(25f, 25f),
            Position = position
        };

        Color = color;
    }

    public void Reset()
    {
        Color = MinoColor.Dark;
    }

    public void Draw(RenderTarget target, RenderStates states)
    {
        _shape.Draw(target, states);
    }
}