// Copyright (C) 2008-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.ComponentModel;
using Xtensive.Reflection;

namespace Xtensive.Orm
{
  /// <summary>
  /// Provides a type converter to convert <see cref="UrlInfo"/> objects to and from other representations.
  /// </summary>
  public class UrlInfoConverter : TypeConverter
  {
    /// <inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
    {
      return sourceType == WellKnownTypes.String || base.CanConvertFrom(context, sourceType);
    }

    /// <inheritdoc/>
    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    {
      if (value is string) {
        return UrlInfo.Parse((string) value);
      }
      return base.ConvertFrom(context, culture, value);
    }

    /// <inheritdoc/>
    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, System.Type destinationType)
    {
      if (destinationType == WellKnownTypes.String) {
        return ((UrlInfo) value).Url;
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }
}