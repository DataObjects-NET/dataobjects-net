// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.03.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class ObjectToBaseAdvancedConverter<TFrom, TTo> : AdvancedConverterBase, 
    IAdvancedConverter<TFrom, TTo>
    where TFrom : TTo
  {
    public virtual TTo Convert(TFrom value)
    {
      return value;
    }

    public bool IsRough
    {
      get { return false; }
    }


    // Constructors

    public ObjectToBaseAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}