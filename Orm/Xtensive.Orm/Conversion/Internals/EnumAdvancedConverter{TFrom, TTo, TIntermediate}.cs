// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.15

using System;
using Xtensive.Reflection;

namespace Xtensive.Conversion
{
  internal class EnumAdvancedConverter<TFrom, TTo, TIntermediate> : AdvancedConverterBase,
    IAdvancedConverter<TFrom, TTo>
    where TFrom : struct
    where TTo : struct
    where TIntermediate : struct
  {
    private static readonly Converter<TIntermediate, TTo> OutputEnumConverter
      = typeof(TTo).IsEnum ? DelegateHelper.CreatePrimitiveCastDelegate<TIntermediate, TTo>() : null;

    private static readonly Converter<TFrom, TIntermediate> InputEnumConverter
      = typeof(TFrom).IsEnum ? DelegateHelper.CreatePrimitiveCastDelegate<TFrom, TIntermediate>() : null;

    private readonly AdvancedConverterStruct<TIntermediate, TTo> outputValueTypeAdvancedConverter;
    private readonly AdvancedConverterStruct<TFrom, TIntermediate> inputValueTypeAdvancedConverter;

    public bool IsRough => true;

    public virtual TTo Convert(TFrom value)
    {
      TIntermediate intermediate = InputEnumConverter is null ? inputValueTypeAdvancedConverter.Convert(value) : InputEnumConverter(value);
      return OutputEnumConverter is null ? outputValueTypeAdvancedConverter.Convert(intermediate) : OutputEnumConverter(intermediate);
    }


    // Constructors

    public EnumAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
      var toType = typeof(TTo);
      var fromType = typeof(TFrom);
      if (!toType.IsEnum && !fromType.IsEnum)
        throw new InvalidOperationException();
      if (!fromType.IsEnum)
        inputValueTypeAdvancedConverter = provider.GetConverter<TFrom, TIntermediate>();
      if (!toType.IsEnum)
        outputValueTypeAdvancedConverter = provider.GetConverter<TIntermediate, TTo>();
    }
  }
}