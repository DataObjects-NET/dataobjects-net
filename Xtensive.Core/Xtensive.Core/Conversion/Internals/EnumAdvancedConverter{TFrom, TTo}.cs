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
  internal class EnumAdvancedConverter<TFrom, TTo> : AdvancedConverterBase, 
    IAdvancedConverter<TFrom, TTo>
    where TFrom : struct
    where TTo : struct
  {
    private static readonly Converter<TFrom, TTo> converter = DelegateHelper.CreatePrimitiveCastDelegate<TFrom, TTo>(); 

    public virtual TTo Convert(TFrom value)
    {
      return converter(value);
    }

    public bool IsRough
    {
      get { return true; }
    }


    // Constructors

    public EnumAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}