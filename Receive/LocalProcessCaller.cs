using System.Diagnostics;

public static class LocalProcessCaller
{
    public static string GetResultFromCall(string processName, string methodName)
    {
        Process proc = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                Arguments = methodName,
                FileName = processName,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            }
        };

        proc.Start();

        string output = proc.StandardOutput.ReadToEnd();

        proc.WaitForExit();

        return output;
    }
}
