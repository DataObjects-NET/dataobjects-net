// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class SByteRoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<sbyte, bool>
  {
    bool IAdvancedConverter<sbyte, bool>.Convert(sbyte value)
    {
      return Convert.ToBoolean(value);
    }


    // Constructors

    public SByteRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}