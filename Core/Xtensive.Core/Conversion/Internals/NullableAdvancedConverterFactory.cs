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
  internal class NullableAdvancedConverterFactory<TFrom> : IAdvancedConverterFactory<TFrom>
  {
    private IAdvancedConverterProvider provider;

    public IAdvancedConverter<TFrom, TTo> CreateForwardConverter<TTo>()
    {
      bool fromIsNullable = TypeHelper.IsNullable<TFrom>();
      bool toIsNullable = TypeHelper.IsNullable<TTo>();
      if (fromIsNullable && toIsNullable) {
        try {
          return
            typeof (NullableNullableAdvancedConverter<,>).Activate(
              new Type[] { typeof(TFrom).GetGenericArguments()[0], typeof(TTo).GetGenericArguments()[0] }, provider)
              as IAdvancedConverter<TFrom, TTo>;
        }
        catch {
          return null;
        }
      }
      else if (fromIsNullable) {
        return typeof (NullableForwardAdvancedConverter<,>).Activate(
          new Type[] { typeof(TFrom).GetGenericArguments()[0], typeof(TTo) }, provider)
          as IAdvancedConverter<TFrom, TTo>;
      }
      else if (toIsNullable) {
        return typeof (NullableReverseAdvancedConverter<,>).Activate(
          new Type[] { typeof(TFrom), typeof(TTo).GetGenericArguments()[0] }, provider)
          as IAdvancedConverter<TFrom, TTo>;
      }
      return null;
    }

    public IAdvancedConverter<TTo, TFrom> CreateBackwardConverter<TTo>()
    {
      bool fromIsNullable = TypeHelper.IsNullable<TFrom>();
      if (fromIsNullable) {
        return typeof (NullableReverseAdvancedConverter<,>).Activate(
          new Type[] { typeof(TTo), typeof(TFrom).GetGenericArguments()[0] }, provider)
          as IAdvancedConverter<TTo, TFrom>;
      }
      return null;
    }


    // Constructors

    public NullableAdvancedConverterFactory(IAdvancedConverterProvider provider)
    {
      this.provider = provider;
    }
  }
}