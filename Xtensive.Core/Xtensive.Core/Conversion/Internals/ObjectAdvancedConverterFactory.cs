// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.03.08

using System;
using Xtensive.Reflection;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class ObjectAdvancedConverterFactory<TFrom> : 
    IAdvancedConverterFactory<TFrom>
  {
    private readonly IAdvancedConverterProvider provider;

    public IAdvancedConverter<TFrom, TTo> CreateForwardConverter<TTo>()
    {
      Type fromType = typeof (TFrom);
      Type toType = typeof (TTo);
      if (toType.IsAssignableFrom(fromType)) {
        Type[] genericArguments = new Type[] {fromType, toType};
        Type   genericType      = typeof (ObjectToBaseAdvancedConverter<,>);
        return genericType.Activate(genericArguments, provider) as IAdvancedConverter<TFrom, TTo>;
      }
      else if (fromType.IsAssignableFrom(toType)) {
        Type[] genericArguments = new Type[] {fromType, toType};
        Type   genericType      = typeof (ObjectToDescendantAdvancedConverter<,>);
        return genericType.Activate(genericArguments, provider) as IAdvancedConverter<TFrom, TTo>;
      }
      return null;
    }

    public IAdvancedConverter<TTo, TFrom> CreateBackwardConverter<TTo>()
    {
      Type fromType = typeof (TFrom);
      Type toType = typeof (TTo);
      if (fromType.IsAssignableFrom(toType)) {
        Type[] genericArguments = new Type[] {toType, fromType};
        Type   genericType      = typeof (ObjectToBaseAdvancedConverter<,>);
        return genericType.Activate(genericArguments, provider) as IAdvancedConverter<TTo, TFrom>;
      }
      else if (toType.IsAssignableFrom(fromType)) {
        Type[] genericArguments = new Type[] {toType, fromType};
        Type   genericType      = typeof (ObjectToDescendantAdvancedConverter<,>);
        return genericType.Activate(genericArguments, provider) as IAdvancedConverter<TTo, TFrom>;
      }
      return null;
    }


    // Constructors

    public ObjectAdvancedConverterFactory(IAdvancedConverterProvider provider)
    {
      this.provider = provider;
    }
  }
}