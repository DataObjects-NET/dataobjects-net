// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class StringRoughAdvancedConverter<T> :
    RoughAdvancedConverterBase,
    IAdvancedConverter<T, string>
  {
    string IAdvancedConverter<T, string>.Convert(T value)
    {
      return value.ToString();
    }


    // Constructors

    public StringRoughAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}