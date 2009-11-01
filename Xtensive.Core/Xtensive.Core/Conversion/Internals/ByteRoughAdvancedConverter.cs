// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Core.Conversion
{
  [Serializable]
  internal class ByteRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<byte, bool>
  {
    bool IAdvancedConverter<byte, bool>.Convert(byte value)
    {
      return System.Convert.ToBoolean(value);
    }


    // Constructors

    public ByteRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}