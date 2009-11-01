// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Core.Conversion
{
  [Serializable]
  internal class Int16RoughAdvancedConverter :
    RoughAdvancedConverterBase,
    IAdvancedConverter<short, bool>
  {
    bool IAdvancedConverter<short, bool>.Convert(short value)
    {
      return System.Convert.ToBoolean(value);
    }


    // Constructors

    public Int16RoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}