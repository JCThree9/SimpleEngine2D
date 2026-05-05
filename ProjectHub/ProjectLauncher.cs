using System.Diagnostics;

namespace ProjectHub;

public static class ProjectLauncher
{
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
