// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.14

using System;
using Xtensive.Reflection;

namespace Xtensive.Conversion
{
  internal class EnumAdvancedConverterFactory<TFrom> : IAdvancedConverterFactory<TFrom>
  {
    private readonly IAdvancedConverterProvider provider;

    public IAdvancedConverter<TFrom, TTo> CreateForwardConverter<TTo>()
    {
      var fromType = typeof (TFrom);
      var toType = typeof (TTo);
      if (fromType.IsEnum) {
        var fromUnderlyingType = Enum.GetUnderlyingType(fromType);
        Type[] genericArguments;
        Type genericType;
        if (toType.IsEnum) {
          Type toUnderlyingType = Enum.GetUnderlyingType(toType);
          if (fromUnderlyingType==toUnderlyingType) {
            genericArguments = [fromType, toType, fromUnderlyingType];
            genericType = typeof (EnumAdvancedConverter<,,>);
          }
          else {
            genericArguments = [fromType, toType, fromUnderlyingType, toUnderlyingType];
            genericType = typeof (EnumAdvancedConverter<,,,>);
          }
        }
        else {
          if (toType==fromUnderlyingType) {
            genericArguments = [fromType, toType];
            genericType = typeof (EnumAdvancedConverter<,>);
          }
          else {
            genericArguments = [fromType, toType, fromUnderlyingType];
            genericType = typeof (EnumAdvancedConverter<,,>);
          }
        }
        return genericType.Activate(genericArguments, provider) as IAdvancedConverter<TFrom, TTo>;
      }
      return null;
    }

    public IAdvancedConverter<TTo, TFrom> CreateBackwardConverter<TTo>()
    {
      var fromType = typeof (TFrom);
      var toType = typeof (TTo);
      if (fromType.IsEnum) {
        var fromUnderlyingType = Enum.GetUnderlyingType(fromType);
        Type[] genericArguments;
        Type genericType;
        if (toType==fromUnderlyingType) {
          genericArguments = [toType, fromType];
          genericType = typeof (EnumAdvancedConverter<,>);
        }
        else {
          genericArguments = [toType, fromType, fromUnderlyingType];
          genericType = typeof (EnumAdvancedConverter<,,>);
        }
        object result = genericType.Activate(genericArguments, provider);
        return result as IAdvancedConverter<TTo, TFrom>;
      }
      return null;
    }


    // Constructors

    public EnumAdvancedConverterFactory(IAdvancedConverterProvider provider)
    {
      this.provider = provider;
    }
  }
}