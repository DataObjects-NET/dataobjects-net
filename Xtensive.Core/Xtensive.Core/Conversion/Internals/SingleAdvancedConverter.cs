// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.23

using System;
using System.Globalization;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class SingleAdvancedConverter :
    StrictAdvancedConverterBase<float>,
    IAdvancedConverter<float, string>
  {
    string IAdvancedConverter<float, string>.Convert(float value)
    {
      // G9 gives accurate and convenient representation of float
      return value.ToString("G9", CultureInfo.InvariantCulture);
    }


    // Constructors

    public SingleAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}