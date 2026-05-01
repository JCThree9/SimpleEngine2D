using Engine.Core;
using Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SimpleEngine2D.Engine.UI;

namespace ProjectHub.Scenes;

/// <summary>
/// The project hub scene — lists game projects found in the parent directory
/// and lets you launch them by clicking or pressing Enter.
/// </summary>
public class HubScene : Scene
{
    private TextInputField _testInput;
    private List<string> _projectPaths = new();
    private int _selectedIndex = 0;
    private SpriteFont? _font;
    private float _inputCooldown = 0f;

    public override void Initialize()
    {
        _testInput = new TextInputField(new Rectangle(50, 500, 300, 40), "Type here");
        _testInput.Activate(Game!.Window);
        UseCamera = false; // UI-only scene, draw in screen space
        ScanProjects();

        // Load the font for displaying project names
        try
        {
            _font = Content.Load<SpriteFont>("DebugFont");
        }
        catch
        {
            // Font not available — will draw rectangles as fallback
        }
    }

    public override void Update(GameTime gameTime)
    {
        _testInput.Update(gameTime);
        _inputCooldown -= Time.DeltaTime;
        if (_inputCooldown > 0) return;

        // Navigate the list
        if (Input.IsKeyPressed(Keys.Down) || Input.IsKeyPressed(Keys.S))
        {
            _selectedIndex = Math.Min(_selectedIndex + 1, _projectPaths.Count - 1);
            _inputCooldown = 0.15f;
        }
        if (Input.IsKeyPressed(Keys.Up) || Input.IsKeyPressed(Keys.W))
        {
            _selectedIndex = Math.Max(_selectedIndex - 1, 0);
            _inputCooldown = 0.15f;
        }

        // Launch selected project
        if (Input.IsKeyPressed(Keys.Enter) && _projectPaths.Count > 0)
        {
            ProjectLauncher.Launch(_projectPaths[_selectedIndex]);
        }

        // Refresh project list
        if (Input.IsKeyPressed(Keys.F5))
        {
            ScanProjects();
        }
        if (Mouse.GetState().LeftButton == ButtonState.Pressed)
        {

        if (_testInput.ContainsPoint(Mouse.GetState().Position))
        {
        _testInput.Activate(Game!.Window);
        }
        }

        // Quit
        if (Input.IsKeyPressed(Keys.Escape))
        {
            Game!.Exit();
        }
    }

    public override void Draw(Renderer renderer)
    {
        // Title bar
        renderer.DrawRect(new Rectangle(0, 0, Game!.ScreenWidth, 60), new Color(30, 30, 40));

        // Project list background
        renderer.DrawRect(new Rectangle(0, 60, Game.ScreenWidth, Game.ScreenHeight - 60), new Color(20, 20, 28));

        if (_font == null) return;

        // Draw title
        renderer.SpriteBatch.DrawString(_font, "SimpleEngine2D - Project Hub",
            new Vector2(20, 20), Color.White);

        renderer.SpriteBatch.DrawString(_font, "[Enter] Launch  [F5] Refresh  [Esc] Quit",
            new Vector2(600, 20), Color.Gray);

        if (_projectPaths.Count == 0)
        {
            renderer.SpriteBatch.DrawString(_font, "No projects found. Add game projects next to the ProjectHub directory.",
                new Vector2(40, 100), Color.DarkGray);
            return;
        }

        // Draw project list
        for (int i = 0; i < _projectPaths.Count; i++)
        {
            var y = 80 + i * 50;
            var isSelected = i == _selectedIndex;

            // Selection highlight
            if (isSelected)
            {
                renderer.DrawRect(new Rectangle(10, y, Game.ScreenWidth - 20, 44), new Color(40, 60, 90));
            }

            // Project name
            var name = Path.GetFileName(_projectPaths[i]);
            var textColor = isSelected ? Color.Cyan : Color.LightGray;
            renderer.SpriteBatch.DrawString(_font, name, new Vector2(30, y + 12), textColor);

            // Path (smaller, dimmer)
            // Sanitize path for font-safe ASCII display
            var safePath = new string(_projectPaths[i].Select(c => c > 126 ? '?' : c).ToArray());
            renderer.SpriteBatch.DrawString(_font, safePath,
                new Vector2(300, y + 12), Color.DarkGray);
        }
        // Draw test input field
        var pixel = new Texture2D(Game!.GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });
        _testInput.Draw(renderer.SpriteBatch, _font!, pixel);
    }

    private void ScanProjects()
    {
        _projectPaths.Clear();

        // Look for .csproj files in sibling directories
        var hubDir = AppDomain.CurrentDomain.BaseDirectory;

        // Navigate up to the solution root (bin/Debug/net9.0 → project → solution)
        var solutionDir = Path.GetFullPath(Path.Combine(hubDir, "..", "..", "..", ".."));

        if (!Directory.Exists(solutionDir)) return;

        foreach (var dir in Directory.GetDirectories(solutionDir))
        {
            var dirName = Path.GetFileName(dir);

            // Skip the engine, this hub, and hidden/build directories
            if (dirName == "Engine" || dirName == "ProjectHub" ||
                dirName.StartsWith(".") || dirName == "obj" || dirName == "bin")
                continue;

            // Check if it contains a .csproj and has a MonoGame reference
            var csprojFiles = Directory.GetFiles(dir, "*.csproj");
            if (csprojFiles.Length > 0)
            {
                _projectPaths.Add(dir);
            }
        }

        _selectedIndex = Math.Clamp(_selectedIndex, 0, Math.Max(0, _projectPaths.Count - 1));
    }
}
