// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.03.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class ObjectToDescendantAdvancedConverter<TFrom, TTo> : AdvancedConverterBase, 
    IAdvancedConverter<TFrom, TTo>
    where TTo : TFrom
  {
    public virtual TTo Convert(TFrom value)
    {
      return (TTo)value;
    }

    public bool IsRough
    {
      get { return false; }
    }


    // Constructors

    public ObjectToDescendantAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}