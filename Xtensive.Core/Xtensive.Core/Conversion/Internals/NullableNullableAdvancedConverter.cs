// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.13

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class NullableNullableAdvancedConverter<TFrom, TTo> : WrappingAdvancedConverter<TFrom?, TFrom, TTo?, TTo>
    where TFrom : struct
    where TTo : struct
  {
    /// <inheritdoc/>
    public override TTo? Convert(TFrom? value)
    {
      if (value.HasValue)
        return BaseConverter.Convert(value.Value);
      else
        return null;
    }


    // Constructors

    public NullableNullableAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}