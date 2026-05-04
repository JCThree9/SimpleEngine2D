using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using Engine.Core;
using Engine.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectHub.Scenes;

/// <summary>
/// The project hub scene — lists game projects found in the parent directory
/// and lets you launch them by clicking or pressing Enter.
/// </summary>
public class HubScene : Scene
{
    private List<string> _projectPaths = new();
    private int _selectedIndex = 0;
    private SpriteFont? _font;
    private float _inputCooldown = 0f;

    //variables for new project creation
    private string tempFileTestMessage="";
    private string tempFileName = "NewProjectTest";

    public override void Initialize()
    {
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

        //new Project
        if (Input.IsKeyPressed(Keys.F3))
        {
            CreateNewProject();
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

        renderer.SpriteBatch.DrawString(_font, "[Enter] Launch  [F3] New Project  [F5] Refresh  [Esc] Quit",
            new Vector2(600, 20), Color.Gray);

        //temporary message for printing debugging messages for project hub code
        //renderer.SpriteBatch.DrawString(_font, tempFileTestMessage,new Vector2(100, 300), Color.Gray);

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
            if (dirName == "Engine" || dirName == "ProjectHub" ||dirName == "DontDelete" ||
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

    //still implementing, makes a new game project
    private void CreateNewProject()
    {
        // Look for .csproj files in sibling directories
        var hubDir = AppDomain.CurrentDomain.BaseDirectory;

        // Navigate up to the solution root (bin/Debug/net9.0 → project → solution)
        var solutionDir = Path.GetFullPath(Path.Combine(hubDir, "..", "..", "..", ".."));

        //takes original solution directory and makes a copy of the "DontDelete" project to use as a template for new projects
        var originalSolutionDir = solutionDir;
        var copiedProjectDir = Path.Combine(originalSolutionDir, "DontDelete");

        //Combines directory with projectname to make new project directory
        solutionDir = Path.Combine(solutionDir, tempFileName);

        //file test message for debugging
        //tempFileTestMessage = "Creating new project in " + solutionDir+"\nCopied from template at " + copiedProjectDir;

        //makes the file paths into directory info objects for easier information transfer
        var sourceDir = new DirectoryInfo(copiedProjectDir);
        var targetDir = new DirectoryInfo(solutionDir);

        CopyDir(sourceDir, targetDir);

        ScanProjects(); // Refresh the project list to include the new project
    }

    //helper method for copying directories
    private void CopyDir(DirectoryInfo sourceDir, DirectoryInfo targetDir)
    {
        Directory.CreateDirectory(targetDir.FullName);

        //subdir copying
        foreach (var subDir in sourceDir.GetDirectories())
        {
            var tarSubDir = targetDir.CreateSubdirectory(subDir.Name);
            CopyDir(subDir, tarSubDir);
        }


        //File copying
        foreach (var file in sourceDir.GetFiles())
        {
            file.CopyTo(Path.Combine(targetDir.FullName, file.Name), true);
        }
    }
}
