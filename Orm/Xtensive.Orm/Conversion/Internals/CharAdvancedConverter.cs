// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Roman Churakov
// Created:    2008.01.23

using System;

namespace Xtensive.Conversion
{
  internal class CharAdvancedConverter :
    StrictAdvancedConverterBase<char>,
    IAdvancedConverter<char, byte>,
    IAdvancedConverter<char, sbyte>,
    IAdvancedConverter<char, short>,
    IAdvancedConverter<char, ushort>,
    IAdvancedConverter<char, int>,
    IAdvancedConverter<char, uint>,
    IAdvancedConverter<char, long>,
    IAdvancedConverter<char, ulong>,
    IAdvancedConverter<char, float>,
    IAdvancedConverter<char, double>,
    IAdvancedConverter<char, decimal>,
    IAdvancedConverter<char, string>
  {
    byte IAdvancedConverter<char, byte>.Convert(char value) => System.Convert.ToByte(value);
    sbyte IAdvancedConverter<char, sbyte>.Convert(char value) => System.Convert.ToSByte(value);
    short IAdvancedConverter<char, short>.Convert(char value) => System.Convert.ToInt16(value);
    ushort IAdvancedConverter<char, ushort>.Convert(char value) => System.Convert.ToUInt16(value);
    int IAdvancedConverter<char, int>.Convert(char value) => System.Convert.ToInt32(value);
    uint IAdvancedConverter<char, uint>.Convert(char value) => System.Convert.ToUInt32(value);
    long IAdvancedConverter<char, long>.Convert(char value) => System.Convert.ToInt64(value);
    ulong IAdvancedConverter<char, ulong>.Convert(char value) => System.Convert.ToUInt64(value);
    float IAdvancedConverter<char, float>.Convert(char value) => System.Convert.ToSingle(System.Convert.ToUInt16(value));
    double IAdvancedConverter<char, double>.Convert(char value) => System.Convert.ToDouble(System.Convert.ToUInt16(value));
    decimal IAdvancedConverter<char, decimal>.Convert(char value) => System.Convert.ToDecimal(System.Convert.ToUInt16(value));
    string IAdvancedConverter<char, string>.Convert(char value) => value.ToString();


    // Constructors

    public CharAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}