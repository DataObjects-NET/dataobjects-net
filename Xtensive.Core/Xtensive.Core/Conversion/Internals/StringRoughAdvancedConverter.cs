// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class StringRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<string, bool>,
    IAdvancedConverter<string, byte>,
    IAdvancedConverter<string, sbyte>,
    IAdvancedConverter<string, short>,
    IAdvancedConverter<string, ushort>,
    IAdvancedConverter<string, int>,
    IAdvancedConverter<string, uint>,
    IAdvancedConverter<string, long>,
    IAdvancedConverter<string, ulong>,
    IAdvancedConverter<string, float>,
    IAdvancedConverter<string, double>,
    IAdvancedConverter<string, decimal>,
    IAdvancedConverter<string, DateTime>,
    IAdvancedConverter<string, TimeSpan>,
    IAdvancedConverter<string, Guid>
  {
    bool IAdvancedConverter<string, bool>.Convert(string value)
    {
      return Boolean.Parse(value);
    }

    byte IAdvancedConverter<string, byte>.Convert(string value)
    {
      try {
        return Byte.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
      }
      catch (FormatException e) {
        if (value.Substring(0, 2).ToUpper().Equals("0X"))
          return Byte.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        throw;
      }
    }

    sbyte IAdvancedConverter<string, sbyte>.Convert(string value)
    {
      try {
        return SByte.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
      }
      catch (FormatException e) {
        if (value.Substring(0, 2).ToUpper().Equals("0X"))
          return SByte.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        throw;
      }
    }

    short IAdvancedConverter<string, short>.Convert(string value)
    {
      try {
        return short.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
      }
      catch (FormatException e) {
        if (value.Substring(0, 2).ToUpper().Equals("0X"))
          return short.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        throw;
      }
    }

    ushort IAdvancedConverter<string, ushort>.Convert(string value)
    {
      try {
        return ushort.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
      }
      catch (FormatException e) {
        if (value.Substring(0, 2).ToUpper().Equals("0X"))
          return ushort.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        throw;
      }
    }

    int IAdvancedConverter<string, int>.Convert(string value)
    {
      try {
        return int.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
      }
      catch (FormatException e) {
        if (value.Substring(0, 2).ToUpper().Equals("0X"))
          return int.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        throw;
      }
    }

    uint IAdvancedConverter<string, uint>.Convert(string value)
    {
      try {
        return uint.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
      }
      catch (FormatException e) {
        if (value.Substring(0, 2).ToUpper().Equals("0X"))
          return uint.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        throw;
      }
    }

    long IAdvancedConverter<string, long>.Convert(string value)
    {
      try {
        return long.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
      }
      catch (FormatException e) {
        if (value.Substring(0, 2).ToUpper().Equals("0X"))
          return long.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        throw;
      }
    }

    ulong IAdvancedConverter<string, ulong>.Convert(string value)
    {
      try {
        return ulong.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
      }
      catch (FormatException e) {
        if (value.Substring(0, 2).ToUpper().Equals("0X"))
          return ulong.Parse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        throw;
      }
    }

    float IAdvancedConverter<string, float>.Convert(string value)
    {
      return float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    double IAdvancedConverter<string, double>.Convert(string value)
    {
      return double.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    decimal IAdvancedConverter<string, decimal>.Convert(string value)
    {
      return decimal.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    DateTime IAdvancedConverter<string, DateTime>.Convert(string value)
    {
      string[] strings = {"yyyy/MM/dd hh:mm:ss.fffffff tt K "};
      try {
        return DateTime.ParseExact(value, strings, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      }
      catch (FormatException) {
        return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
      }
    }

    TimeSpan IAdvancedConverter<string, TimeSpan>.Convert(string value)
    {
      return TimeSpan.Parse(value);
    }

    Guid IAdvancedConverter<string, Guid>.Convert(string value)
    {
      return new Guid(value);
    }


    // Constructors

    public StringRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}