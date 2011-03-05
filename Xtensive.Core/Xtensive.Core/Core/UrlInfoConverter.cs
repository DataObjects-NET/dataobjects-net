// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System.ComponentModel;

namespace Xtensive.Core
{
  /// <summary>
  /// Provides a type converter to convert <see cref="UrlInfo"/> objects to and from other representations.
  /// </summary>
  public class UrlInfoConverter : TypeConverter
  {
    /// <inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
    {
      return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
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
      if (destinationType == typeof(string)) {
        return ((UrlInfo) value).Url;
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }
}