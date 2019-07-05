using System.IO;

namespace KubernetesExtension
{
    public class GetValueFromFile
    {
        public string GetValue(string fileName, string key)
        {
            string retval = string.Empty;

            foreach (var line in File.ReadAllLines(fileName))
            {
                if (line.Contains(key))
                {
                    retval = line.Substring(key.Length + 3);
                    break;
                }
            }

            return retval;
        }
    }
}