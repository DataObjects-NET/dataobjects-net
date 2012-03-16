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
  internal class DoubleAdvancedConverter :
    StrictAdvancedConverterBase<double>,
    IAdvancedConverter<double, string>
  {
    string IAdvancedConverter<double, string>.Convert(double value)
    {
      // G17 gives accurate and convenient representation of float
      return value.ToString("G17", CultureInfo.InvariantCulture);
    }


    // Constructors

    public DoubleAdvancedConverter(IAdvancedConverterProvider provider)
      : base(provider)
    {
    }
  }
}