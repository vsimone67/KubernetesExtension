using KubernetesExtension;
using System.Diagnostics;

namespace Kubernetes
{
    public class ProcessBase
    {
        protected string json;

        public void RunCommand(string process, string command)
        {
            json = string.Empty;

            Utils.RunProcess(process, command, "", true, GetJsonInfo);
        }

        protected void GetJsonInfo(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                json += e.Data;
            }
        }
    }
}