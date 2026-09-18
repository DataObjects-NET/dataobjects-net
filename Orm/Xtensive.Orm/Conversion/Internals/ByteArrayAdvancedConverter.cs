// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;

namespace Xtensive.Conversion
{
  internal class ByteArrayAdvancedConverter :
    StrictAdvancedConverterBase<byte[]>,
    IAdvancedConverter<byte[], string>,
    IAdvancedConverter<string, byte[]>
  {
    string IAdvancedConverter<byte[], string>.Convert(byte[] value)
      => (value is null)
        ? null
        : (value.Length == 0)
          ? string.Empty
          : $"0x{System.Convert.ToHexString(value)}";

    byte[] IAdvancedConverter<string, byte[]>.Convert(string value)
    {
      if (value == null)
        return null;
      if (string.IsNullOrEmpty(value))
        return Array.Empty<byte>();

      if (!value.StartsWith("0x"))
        throw new ArgumentException("Bad format", value);

      var usefulPart = value.AsSpan().Slice(2);
      return System.Convert.FromHexString(usefulPart);
    }

    // Constructors

    public ByteArrayAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}