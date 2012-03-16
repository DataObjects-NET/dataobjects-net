// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class StringAdvancedConverter :
    StrictAdvancedConverterBase<string>
  {
    // Constructors

    public StringAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}