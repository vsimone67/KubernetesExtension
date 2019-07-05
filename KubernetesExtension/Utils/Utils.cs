using System;
using System.Diagnostics;

namespace KubernetesExtension
{
    public static class Utils
    {
        public static Process RunProcess(string path, string arguments = "", string workingDirectory = "", bool wait = false,
                                                          DataReceivedEventHandler onOutput = null, DataReceivedEventHandler onError = null,
                                                          EventHandler onExit = null)
        {
            var p = new Process();
            p.StartInfo.FileName = path;
            p.StartInfo.WorkingDirectory = workingDirectory;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.CreateNoWindow = true;
            if (onOutput != null)
            {
                p.OutputDataReceived += onOutput;
            }
            if (onError != null)
            {
                p.ErrorDataReceived += onError;
            }
            if (onExit != null)
            {
                p.EnableRaisingEvents = true;
                p.Exited += onExit;
            }

            p.Start();
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            if (wait)
            {
                p.WaitForExit();
            }
            return p;
        }
    }
}