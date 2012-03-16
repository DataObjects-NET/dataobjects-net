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
  internal class EnumAdvancedConverterFactory<TFrom> : IAdvancedConverterFactory<TFrom>
  {
    private readonly IAdvancedConverterProvider provider;

    public IAdvancedConverter<TFrom, TTo> CreateForwardConverter<TTo>()
    {
      Type fromType = typeof (TFrom);
      Type toType = typeof (TTo);
      if (fromType.IsEnum) {
        Type fromUnderlyingType = Enum.GetUnderlyingType(fromType);
        Type[] genericArguments;
        Type genericType;
        if (toType.IsEnum) {
          Type toUnderlyingType = Enum.GetUnderlyingType(toType);
          if (fromUnderlyingType==toUnderlyingType) {
            genericArguments = new Type[] {fromType, toType, fromUnderlyingType};
            genericType = typeof (EnumAdvancedConverter<,,>);
          }
          else {
            genericArguments = new Type[] {fromType, toType, fromUnderlyingType, toUnderlyingType};
            genericType = typeof (EnumAdvancedConverter<,,,>);
          }
        }
        else {
          if (toType==fromUnderlyingType) {
            genericArguments = new Type[] {fromType, toType};
            genericType = typeof (EnumAdvancedConverter<,>);
          }
          else {
            genericArguments = new Type[] {fromType, toType, fromUnderlyingType};
            genericType = typeof (EnumAdvancedConverter<,,>);
          }
        }
        return genericType.Activate(genericArguments, provider) as IAdvancedConverter<TFrom, TTo>;
      }
      return null;
    }

    public IAdvancedConverter<TTo, TFrom> CreateBackwardConverter<TTo>()
    {
      Type fromType = typeof (TFrom);
      Type toType = typeof (TTo);
      if (fromType.IsEnum) {
        Type fromUnderlyingType = Enum.GetUnderlyingType(fromType);
        Type[] genericArguments;
        Type genericType;
        if (toType==fromUnderlyingType) {
          genericArguments = new Type[] {toType, fromType};
          genericType = typeof (EnumAdvancedConverter<,>);
        }
        else {
          genericArguments = new Type[] {toType, fromType, fromUnderlyingType};
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