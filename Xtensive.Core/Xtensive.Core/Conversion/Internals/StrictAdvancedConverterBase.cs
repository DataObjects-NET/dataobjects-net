// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal abstract class StrictAdvancedConverterBase<TFrom>: AdvancedConverterBase,  
    IAdvancedConverter<TFrom, TFrom>
  {
    /// <inheritdoc/>
    public bool IsRough
    {
      get { return false; }
    }

    /// <inheritdoc/>
    public TFrom Convert(TFrom value)
    {
      return value;
    }


    // Constructors

    protected StrictAdvancedConverterBase(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}