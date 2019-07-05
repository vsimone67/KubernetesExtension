using Helm.Release;
using Newtonsoft.Json;

namespace Kubernetes
{
    public class HelmConnection : ProcessBase
    {
        protected T GetHelmApi<T>(string command)
        {
            RunHelmCommand(command);

            return JsonConvert.DeserializeObject<T>(json);
        }

        protected void GetHelmApi(string command)
        {
            RunHelmCommand(command);
        }

        protected void RunHelmCommand(string command)
        {
            RunCommand("helm.exe", command);
        }

        public HelmReleases GetAllHelmReleases()
        {
            return GetHelmApi<HelmReleases>("list --deployed --output json");
        }

        public HelmReleases GetHelmReleases(string @namespace)
        {
            return GetHelmApi<HelmReleases>($"list --deployed --namespace {@namespace} --output json");
        }

        public void DeleteHelmReleases(string deployment)
        {
            GetHelmApi($"delete --purge {deployment}");
        }
    }
}