using System.Diagnostics;

namespace ProjectHub;

/// <summary>
/// Launches a .NET game project in a new process.
/// </summary>
public static class ProjectLauncher
{
    /// <summary>
    /// Launch the game project at the given directory path.
    /// </summary>
    public static void Launch(string projectPath)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "run",
                    WorkingDirectory = projectPath,
                    UseShellExecute = false
                }
            };
            process.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to launch project at '{projectPath}': {ex.Message}");
        }
    }
}
