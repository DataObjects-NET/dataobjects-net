// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class UInt16RoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<ushort, bool>
  {
    bool IAdvancedConverter<ushort, bool>.Convert(ushort value)
    {
      return Convert.ToBoolean(value);
    }


    // Constructors

    public UInt16RoughAdvancedConverter(IAdvancedConverterProvider provider) 
      : base(provider)
    {
    }
  }
}