using Chip8;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

RenderWindow window = new(new VideoMode(1280, 720), "Chip8");
CPU chip8 = new("./IBM Logo.ch8");

window.Closed += (_, __) => window.Close();

List<RectangleShape> rects = new();

while (window.IsOpen)
{
    window.DispatchEvents();
    window.Clear();

    chip8.Cycle();

    for (int y = 0; y < 32; y++)
    {
        for (int x = 0; x < 64; x++)
        {
            if (chip8.Vram[y * 64 + x] > 0)
            {
                rects.Add(new RectangleShape(new Vector2f(20, 20))
                {
                    Position = new Vector2f(x, y) * 20,
                    FillColor = Color.Green
                });
            }
        }
    }

    foreach (var r in rects) { window.Draw(r); }

    window.Display();
}