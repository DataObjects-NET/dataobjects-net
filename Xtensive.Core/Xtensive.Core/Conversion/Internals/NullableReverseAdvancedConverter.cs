// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.13

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class NullableReverseAdvancedConverter<TFrom, TTo> : WrappingAdvancedConverter<TFrom, TFrom, TTo?, TTo>
    where TTo : struct
  {
    /// <inheritdoc/>
    public override TTo? Convert(TFrom value)
    {
      return BaseConverter.Convert(value);
    }


    // Constructors

    public NullableReverseAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}