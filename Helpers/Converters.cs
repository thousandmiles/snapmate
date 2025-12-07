using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SnapMate.Helpers;

/// <summary>
/// Converts boolean values to <see cref="Visibility"/> enumeration.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
    return boolValue ? Visibility.Visible : Visibility.Collapsed;
   }
        return Visibility.Collapsed;
  }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }
}

/// <summary>
/// Inverts a boolean value.
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
return !boolValue;
        }
        return false;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
   return !boolValue;
        }
   return false;
    }
}

/// <summary>
/// Converts null values to <see cref="Visibility.Visible"/> and non-null to <see cref="Visibility.Collapsed"/>.
/// Used to show empty state UI when no data is present.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
 public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts null values to <see cref="Visibility.Collapsed"/> and non-null to <see cref="Visibility.Visible"/>.
/// Used to show content only when data is available.
/// </summary>
public class NotNullToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
   throw new NotImplementedException();
    }
}
