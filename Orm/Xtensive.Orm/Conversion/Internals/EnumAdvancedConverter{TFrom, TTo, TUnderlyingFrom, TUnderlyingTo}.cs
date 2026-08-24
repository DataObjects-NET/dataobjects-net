// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.14

using System;
using Xtensive.Reflection;

namespace Xtensive.Conversion
{
  internal class EnumAdvancedConverter<TFrom, TTo, TUnderlyingFrom, TUnderlyingTo> : AdvancedConverterBase, 
    IAdvancedConverter<TFrom, TTo>
    where TFrom : struct
    where TTo : struct
    where TUnderlyingFrom : struct
    where TUnderlyingTo : struct
  {
    private static readonly Converter<TFrom, TUnderlyingFrom> IntermediateConverter1 = DelegateHelper.CreatePrimitiveCastDelegate<TFrom, TUnderlyingFrom>();
    private static readonly Converter<TUnderlyingTo, TTo> OutputConverter = DelegateHelper.CreatePrimitiveCastDelegate<TUnderlyingTo, TTo>();

    private readonly AdvancedConverterStruct<TUnderlyingFrom, TUnderlyingTo> intermediateConverter2;

    public bool IsRough => true;

    public TTo Convert(TFrom value) => OutputConverter(intermediateConverter2.Convert(IntermediateConverter1(value)));


    // Constructors

    public EnumAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      // Checking types
      var toType = typeof (TTo);
      var fromType = typeof (TFrom);
      if (!toType.IsEnum && !fromType.IsEnum)
        throw new InvalidOperationException();
      intermediateConverter2 = provider.GetConverter<TUnderlyingFrom, TUnderlyingTo>();
    }
  }
}