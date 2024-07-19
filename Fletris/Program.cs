using Fletris;
using SFML.Audio;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

var window = new RenderWindow(new VideoMode(500, 600), "Fletris", Styles.Titlebar | Styles.Close);

var music = new Music("Assets/Fletris.ogg");

music.Loop = true;
music.Play();
music.Volume = 15f;

window.Closed += (sender, _) =>
{
    StopMusic();
    ((RenderWindow)sender!).Close();
};
window.SetFramerateLimit(165);

const int rows = 20;
const int columns = 10;
var minos = new Mino[columns, rows];
for (var i = 0; i < columns; i++)
{
    for (var j = 0; j < rows; j++)
    {
        minos[i, j] = new Mino(MinoColor.Dark, new Vector2f(25f * i + 25f + 100f, 25f * j + 25f));
    }
}

var currentTetromino = new Tetromino();
var clock = new Clock();

const float defaultUpdateInterval = 0.5f;
const float moveCooldown = 0.1f;

var updateInterval = defaultUpdateInterval;
var elapsedTime = 0f;
var leftMoveElapsed = 0f;
var rightMoveElapsed = 0f;
var softDropElapsed = 0f;
var hardDropPressed = false;
var rotatePressed = false;
const float lockDelay = 0.008f;
var lockElapsedTime = 0f;
var lockStarted = false;

var thresholdShader = new Shader(null, null, "Assets/threshold.frag");
var blurShader = new Shader(null, null, "Assets/blur.frag");
var shakeShader = new Shader(null, null, "Assets/shake.frag");

var shakeTime = 0.0f;
const float shakeIntensity = 0.00099f;
thresholdShader.SetUniform("threshold", 0.1f);

var sceneTexture = new RenderTexture(800, 600);
var bloomTexture = new RenderTexture(800, 600);

window.SetActive(true);
    
var clockShader = new Clock();
while (window.IsOpen)
{
    window.DispatchEvents();
    sceneTexture.Clear(Color.Black);
    bloomTexture.Clear(Color.Black);
    
    shakeTime += clockShader.Restart().AsSeconds();
    shakeShader.SetUniform("time", shakeTime);
    shakeShader.SetUniform("intensity", shakeIntensity);
    
    for (var x = 0; x < columns; x++)
    {
        for (var y = 0; y < rows; y++)
        {
            sceneTexture.Draw(minos[x, y]);
        }
    }

    DrawGhostTetromino(sceneTexture, currentTetromino);
    DrawTetromino(sceneTexture, currentTetromino);
    
    sceneTexture.Display();
    
    var sceneSprite = new Sprite(sceneTexture.Texture);
    bloomTexture.Draw(sceneSprite, new RenderStates(thresholdShader));
    bloomTexture.Display();

    const float blurStrength = 3f;
    blurShader.SetUniform("tex_offset", new Vector2f(1.0f / window.Size.X * blurStrength, 0.0f));
    var bloomSprite = new Sprite(bloomTexture.Texture);
    bloomTexture.Draw(bloomSprite, new RenderStates(blurShader));
    bloomTexture.Display();

    blurShader.SetUniform("tex_offset", new Vector2f(0.0f, 2.0f / window.Size.Y * blurStrength));
    bloomSprite = new Sprite(bloomTexture.Texture);
    bloomTexture.Draw(bloomSprite, new RenderStates(blurShader));
    bloomTexture.Display();
    
    window.Clear(Color.Black);
    window.Draw(new Sprite(sceneTexture.Texture), new RenderStates(shakeShader));
    window.Draw(new Sprite(bloomTexture.Texture), new RenderStates(BlendMode.Add));
    
    var deltaTime = clock.Restart().AsSeconds();
    elapsedTime += deltaTime;
    leftMoveElapsed += deltaTime;
    rightMoveElapsed += deltaTime;
    softDropElapsed += deltaTime;
    
    

    if (elapsedTime >= updateInterval)
    {
        elapsedTime = 0f;
        var movedTetromino = new Tetromino
        {
            Position = new Vector2i(currentTetromino.Position.X, currentTetromino.Position.Y + 1),
            Cells = currentTetromino.Cells,
            Color = currentTetromino.Color
        };

        if (!IsCollision(movedTetromino))
        {
            currentTetromino.Position = movedTetromino.Position;
            lockStarted = false;
            lockElapsedTime = 0f;
        }
        else
        {
            if (!lockStarted)
            {
                lockStarted = true;
                lockElapsedTime = 0f;
            }

            lockElapsedTime += deltaTime;

            if (lockElapsedTime >= lockDelay || hardDropPressed)
            {
                PlaceTetromino(currentTetromino);
                ClearLines();
                currentTetromino = new Tetromino();
                lockStarted = false;
                if (IsCollision(currentTetromino))
                {
                    StopMusic();
                    window.Close();
                }
            }
        }
    }

    var leftKeyPressed = Keyboard.IsKeyPressed(Keyboard.Key.Left);
    var rightKeyPressed = Keyboard.IsKeyPressed(Keyboard.Key.Right);
    var downKeyPressed = Keyboard.IsKeyPressed(Keyboard.Key.Down);

    if (leftKeyPressed && leftMoveElapsed >= moveCooldown)
    {
        var movedTetromino = new Tetromino
        {
            Position = new Vector2i(currentTetromino.Position.X - 1, currentTetromino.Position.Y),
            Cells = currentTetromino.Cells,
            Color = currentTetromino.Color
        };

        if (!IsCollision(movedTetromino))
        {
            currentTetromino.Position = movedTetromino.Position;
        }

        leftMoveElapsed = 0f;
    }

    if (rightKeyPressed && rightMoveElapsed >= moveCooldown)
    {
        var movedTetromino = new Tetromino
        {
            Position = new Vector2i(currentTetromino.Position.X + 1, currentTetromino.Position.Y),
            Cells = currentTetromino.Cells,
            Color = currentTetromino.Color
        };

        if (!IsCollision(movedTetromino))
        {
            currentTetromino.Position = movedTetromino.Position;
        }

        rightMoveElapsed = 0f;
    }

    if (downKeyPressed && softDropElapsed >= 0.1f)
    {
        var movedTetromino = new Tetromino
        {
            Position = new Vector2i(currentTetromino.Position.X, currentTetromino.Position.Y + 1),
            Cells = currentTetromino.Cells,
            Color = currentTetromino.Color
        };

        if (!IsCollision(movedTetromino))
        {
            currentTetromino.Position = movedTetromino.Position;
        }

        softDropElapsed = 0f;
    }

    if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
    {
        if (!rotatePressed && currentTetromino.Type != TetrominoType.O)
        {
            RotateTetromino(currentTetromino);
            rotatePressed = true;
        }
    }
    else
    {
        rotatePressed = false;
    }

    if (Keyboard.IsKeyPressed(Keyboard.Key.Space))
    {
        if (!hardDropPressed)
        {
            HardDrop();
            elapsedTime = updateInterval;
            hardDropPressed = true;
        }
    }
    else
    {
        hardDropPressed = false;
    }

    updateInterval = Keyboard.IsKeyPressed(Keyboard.Key.Down) ? 0.1f : defaultUpdateInterval;
    
    window.Display();
}

return;

bool IsCollision(Tetromino tetromino)
{
    return (from cell in tetromino.Cells let x = cell.X + tetromino.Position.X let y = cell.Y + tetromino.Position.Y where x < 0 || x >= columns || y < 0 || y >= rows || (minos[x, y].Color != MinoColor.Dark) select x).Any();
}

void RotateTetromino(Tetromino tetromino)
{
    var rotatedTetromino = new Tetromino
    {
        Position = tetromino.Position,
        Cells = (Vector2i[])tetromino.Cells.Clone(),
        Color = tetromino.Color
    };

    rotatedTetromino.Rotate();

    var kickOffsets = new List<Vector2i>
    {
        new(0, 0), // no offset
        new(1, 0), // right
        new(-1, 0), // left
        new(0, -1), // up
        new(0, 1) // down
    };

    foreach (var testPosition in kickOffsets.Select(offset => new Vector2i(tetromino.Position.X + offset.X, tetromino.Position.Y + offset.Y)))
    {
        rotatedTetromino.Position = testPosition;

        if (IsCollision(rotatedTetromino)) continue;
        tetromino.Cells = rotatedTetromino.Cells;
        tetromino.Position = testPosition;
        return;
    }
}

void ClearLines()
{
    for (var y = rows - 1; y >= 0; y--)
    {
        var isFullLine = true;
        for (var x = 0; x < columns; x++)
        {
            if (minos[x, y].Color != MinoColor.Dark) continue;
            isFullLine = false;
            break;
        }

        if (!isFullLine) continue;
        for (var yy = y; yy > 0; yy--)
        {
            for (var xx = 0; xx < columns; xx++)
            {
                minos[xx, yy].Color = minos[xx, yy - 1].Color;
            }
        }

        for (var xx = 0; xx < columns; xx++)
        {
            minos[xx, 0].Color = MinoColor.Dark;
        }

        y++;
    }
}

void DrawGhostTetromino(RenderTexture win, Tetromino tetromino)
{
    var ghostTetromino = new Tetromino
    {
        Position = new Vector2i(tetromino.Position.X, tetromino.Position.Y),
        Cells = (Vector2i[])tetromino.Cells.Clone(),
        Color = MinoColor.Preview
    };

    while (!IsCollision(new Tetromino
           {
               Position = new Vector2i(ghostTetromino.Position.X, ghostTetromino.Position.Y + 1),
               Cells = ghostTetromino.Cells,
               Color = ghostTetromino.Color
           }))
    {
        ghostTetromino.Position = new Vector2i(ghostTetromino.Position.X, ghostTetromino.Position.Y + 1);
    }

    DrawTetromino(win, ghostTetromino);
}

void PlaceTetromino(Tetromino tetromino)
{
    foreach (var cell in tetromino.Cells)
    {
        var x = cell.X + tetromino.Position.X;
        var y = cell.Y + tetromino.Position.Y;
        if (x is >= 0 and < columns && y is >= 0 and < rows)
        {
            minos[x, y].Color = tetromino.Color;
        }
    }
}

void HardDrop()
{
    while (!IsCollision(new Tetromino
           {
               Position = new Vector2i(currentTetromino.Position.X, currentTetromino.Position.Y + 1),
               Cells = currentTetromino.Cells,
               Color = currentTetromino.Color
           }))
    {
        currentTetromino.Position = new Vector2i(currentTetromino.Position.X, currentTetromino.Position.Y + 1);
    }
}

void DrawTetromino(RenderTexture win, Tetromino tetromino)
{
    foreach (var cell in tetromino.Cells)
    {
        var x = cell.X + tetromino.Position.X;
        var y = cell.Y + tetromino.Position.Y;
        if (x < 0 || x >= columns || y < 0 || y >= rows) continue;
        var mino = new Mino(tetromino.Color, new Vector2f(25f * x + 25f + 100f, 25f * y + 25f));
        win.Draw(mino);
    }
}

void StopMusic()
{
    music.Stop();
    music.Dispose();
}
