namespace Kubernetes.Windows.Converters
{
    using KubeClient.Models;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Data;

    public class ServiceStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var service = value as ServiceV1;
            var result = new ObservableCollection<string>();

            if (service.Status.LoadBalancer.Ingress.Any())
            {
                result.Add($"Cluster IP: {service.Status.LoadBalancer.Ingress.First().Ip}");
                result.Add($"HostName: {service.Status.LoadBalancer.Ingress.First().Hostname}");
            }

            result.Add($"Service Type: {service.Spec.Type}");
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}