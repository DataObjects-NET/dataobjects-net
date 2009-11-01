// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;

namespace Xtensive.Core.Conversion
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