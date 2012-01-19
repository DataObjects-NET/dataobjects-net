// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.13

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class NullableForwardAdvancedConverter<TFrom, TTo> : WrappingAdvancedConverter<TFrom?, TFrom, TTo, TTo>
    where TFrom : struct
  {
    private static readonly bool toIsValueType = typeof (TTo).IsValueType;

    /// <inheritdoc/>
    public override TTo Convert(TFrom? value)
    {
      if (toIsValueType && !value.HasValue)
        throw new ArgumentNullException("value");
      return BaseConverter.Convert(value.GetValueOrDefault());
    }


    // Constructors

    public NullableForwardAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}