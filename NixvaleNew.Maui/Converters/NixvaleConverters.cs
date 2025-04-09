using Nixvale.Core.Debug;
using System.Globalization;

namespace NixvaleNew.Maui.Converters;

public class BoolToStartStopConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "Stop Node" : "Start Node";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToStartStopCommandConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var viewModel = parameter as ViewModels.MainViewModel;
        return (bool)value ? viewModel.StopNodeCommand : viewModel.StartNodeCommand;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class LogLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var level = (LogLevel)value;
        return level switch
        {
            LogLevel.Trace => Colors.Gray,
            LogLevel.Debug => Colors.LightGray,
            LogLevel.Information => Colors.White,
            LogLevel.Warning => Colors.Yellow,
            LogLevel.Error => Colors.Red,
            LogLevel.Critical => Colors.DarkRed,
            _ => Colors.White
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 