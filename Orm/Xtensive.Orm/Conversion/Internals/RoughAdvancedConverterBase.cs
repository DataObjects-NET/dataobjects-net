// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.08

using System;

namespace Xtensive.Conversion
{
  internal abstract class RoughAdvancedConverterBase: AdvancedConverterBase
  {
    /// <summary>
    /// Gets <see langword="true"/> if converter is rough, otherwise gets <see langword="false"/>.
    /// </summary>
    public bool IsRough => true;


    // Constructors

    protected RoughAdvancedConverterBase(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}