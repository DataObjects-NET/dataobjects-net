// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.14

using System;
using Xtensive.Reflection;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class EnumAdvancedConverter<TFrom, TTo, TUnderlyingFrom, TUnderlyingTo> : AdvancedConverterBase, 
    IAdvancedConverter<TFrom, TTo>
    where TFrom : struct
    where TTo : struct
    where TUnderlyingFrom : struct
    where TUnderlyingTo : struct
  {
    private static readonly Converter<TFrom, TUnderlyingFrom> intermediateConverter1 = DelegateHelper.CreatePrimitiveCastDelegate<TFrom, TUnderlyingFrom>();
    private static readonly Converter<TUnderlyingTo, TTo> outputConverter = DelegateHelper.CreatePrimitiveCastDelegate<TUnderlyingTo, TTo>();
    private readonly AdvancedConverterStruct<TUnderlyingFrom, TUnderlyingTo> intermediateConverter2;


    public TTo Convert(TFrom value)
    {
      return outputConverter(intermediateConverter2.Convert(intermediateConverter1(value)));
    }

    public bool IsRough
    {
      get { return true; }
    }


    // Constructors

    public EnumAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      // Checking types
      Type toType = typeof (TTo);
      Type fromType = typeof (TFrom);
      if (!toType.IsEnum && !fromType.IsEnum)
        throw new InvalidOperationException();
      intermediateConverter2 = provider.GetConverter<TUnderlyingFrom, TUnderlyingTo>();
    }
  }
}