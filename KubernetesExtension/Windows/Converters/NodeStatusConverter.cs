namespace Kubernetes.Windows.Converters
{
    using Kubernetes.ExtentionMethods;
    using Kubernetes.Models;
    using System;
    using System.Collections.ObjectModel;
    using System.Windows.Data;

    public class NodeStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var status = value as NodeInfo;
            var result = new ObservableCollection<string>();
            result.Add($"Ip Address: {status.IpAddress}");
            result.Add($"Running {status.IsRunning.ToYesNoString()}");
            result.Add($"CIDR {status.Cidr}");

            if (!status.IsRunning)
                result.Add($"Status Message {status.StatusMessage}");

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}