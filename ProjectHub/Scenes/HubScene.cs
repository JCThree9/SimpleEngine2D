using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using Engine.Core;
using Engine.Rendering;
using Engine.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ProjectHub.Scenes;

public class HubScene : Scene
{
    private TextInputField _testInput;
    private List<string> _projectPaths = new();
    private int _selectedIndex = 0;
    private SpriteFont? _font;
    private float _inputCooldown = 0f;

    //variables for new project creation
    private Texture2D _pixel2;
    private TextInputField _ProjectNameInput;
    Boolean projectnamevisi=false;
    private string tempFileTestMessage="";
    private string tempFileName = "NewProjectTest";

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

        //projectcreation initlizations
        _pixel2 = new Texture2D(Game!.GraphicsDevice, 1, 1);
        _pixel2.SetData(new[] { Color.White });
        int panelW = 500;
        int panelH = 150;
        int inputW = 400;
        int inputH = 40;
        int panelY = (Game!.ScreenHeight - panelH) / 2;
        int inputY = panelY + 70;
        
        _ProjectNameInput = new TextInputField(new Rectangle((Game.ScreenWidth - inputW) / 2, inputY, inputW, inputH), "");
        // When they press Enter
        _ProjectNameInput.OnConfirmed += (name) =>
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                tempFileName = name;
                CreateNewProject();
            }
            projectnamevisi = false;
            _ProjectNameInput.Deactivate(Game!.Window);
             _inputCooldown = 0.5f;
        };
        // When they press Escape
        _ProjectNameInput.OnCancelled += () =>
        {
            projectnamevisi = false;
            _ProjectNameInput.Deactivate(Game!.Window);
             _inputCooldown = 0.5f;
        };

    }

    public override void Update(GameTime gameTime)
    {
        // If the project name input is active, update it and skip all other input
        if (projectnamevisi)
        {
            _ProjectNameInput.Update(gameTime);
            return; // Skip all other input while naming
        }

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

        if (_projectPaths.Count > 0)
        {
            var mousePos = Input.MousePosition;

            for (int i = 0; i < _projectPaths.Count; i++)
            {
                var y = 80 + i * 50;
                var rowRect = new Rectangle(10, y, Game!.ScreenWidth - 20, 44);

                if (rowRect.Contains(mousePos))
                {
                    _selectedIndex = i;

                    if (Input.IsLeftMousePressed())
                        ProjectLauncher.Launch(_projectPaths[_selectedIndex]);
                }
            }
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

        //new Project
        if (Input.IsKeyPressed(Keys.F3))
        {
            projectnamevisi = true;
            _ProjectNameInput.SetText("");
            _ProjectNameInput.Activate(Game!.Window);
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
        // Draw test input field
        //var pixel = new Texture2D(Game!.GraphicsDevice, 1, 1);
        //pixel.SetData(new[] { Color.White });
        //_testInput.Draw(renderer.SpriteBatch, _font!, pixel);


        //new project name input field
        if(projectnamevisi)
        {
            // Draw a dark semi-transparent overlay over the whole screen
            renderer.DrawRect(new Rectangle(0, 0, Game!.ScreenWidth, Game.ScreenHeight), new Color(0, 0, 0, 150));

            // Panel dimensions
            int panelW = 500;
            int panelH = 150;
            int panelX = (Game.ScreenWidth - panelW) / 2;
            int panelY = (Game.ScreenHeight - panelH) / 2;

            // Draw outer panel (dark tone border)
            renderer.DrawRect(new Rectangle(panelX, panelY, panelW, panelH), new Color(40, 60, 90));
            // Draw inner panel (lighter tone background)
            renderer.DrawRect(new Rectangle(panelX + 4, panelY + 4, panelW - 8, panelH - 8), new Color(20, 20, 28));

            // Draw the prompt text centered horizontally
            string prompt = "Enter project name:";
            Vector2 textSize = _font.MeasureString(prompt);
            Vector2 textPos = new Vector2(panelX + (panelW - textSize.X) / 2, panelY + 20);
            renderer.SpriteBatch.DrawString(_font, prompt, textPos, Color.White);

            // Draw the input field
            _ProjectNameInput.Draw(renderer.SpriteBatch, _font!, _pixel2);
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


        if(tempFileName=="")
        {
            tempFileName = "NewProjectTest";
        }

        //Combines directory with projectname to make new project directory
        solutionDir = Path.Combine(solutionDir, tempFileName);

        //file test message for debugging
        //tempFileTestMessage = _testInput.Text;

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
