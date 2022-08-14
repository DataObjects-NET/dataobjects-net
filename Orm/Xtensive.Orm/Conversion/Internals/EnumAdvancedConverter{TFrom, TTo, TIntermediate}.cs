// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.15

using System;
using Xtensive.Reflection;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class EnumAdvancedConverter<TFrom, TTo, TIntermediate> : AdvancedConverterBase,
    IAdvancedConverter<TFrom, TTo>
    where TFrom : struct
    where TTo : struct
    where TIntermediate : struct
  {
    private static readonly Converter<TIntermediate, TTo> outputEnumConverter = typeof(TTo).IsEnum ? DelegateHelper.CreatePrimitiveCastDelegate<TIntermediate, TTo>() : null;
    private static readonly Converter<TFrom, TIntermediate> inputEnumConverter = typeof(TFrom).IsEnum ? DelegateHelper.CreatePrimitiveCastDelegate<TFrom, TIntermediate>() : null;

    private readonly AdvancedConverterStruct<TIntermediate, TTo> outputValueTypeAdvancedConverter;
    private readonly AdvancedConverterStruct<TFrom, TIntermediate> inputValueTypeAdvancedConverter;

    public virtual TTo Convert(TFrom value)
    {
      TIntermediate intermediate = inputEnumConverter==null ? inputValueTypeAdvancedConverter.Convert(value) : inputEnumConverter(value);
      return outputEnumConverter==null ? outputValueTypeAdvancedConverter.Convert(intermediate) : outputEnumConverter(intermediate);
    }

    public bool IsRough
    {
      get { return true; }
    }


    // Constructors

    public EnumAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      Type toType = typeof (TTo);
      Type fromType = typeof (TFrom);
      if (!toType.IsEnum && !fromType.IsEnum)
        throw new InvalidOperationException();
      if (!typeof (TFrom).IsEnum)
        inputValueTypeAdvancedConverter = provider.GetConverter<TFrom, TIntermediate>();
      if (!typeof (TTo).IsEnum)
        outputValueTypeAdvancedConverter = provider.GetConverter<TIntermediate, TTo>();
    }
  }
}