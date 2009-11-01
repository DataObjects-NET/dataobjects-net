// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.23

using System;
using System.Globalization;

namespace Xtensive.Core.Conversion
{
  [Serializable]
  internal class DecimalAdvancedConverter :
    StrictAdvancedConverterBase<decimal>,
    IAdvancedConverter<decimal, string>
  {
    string IAdvancedConverter<decimal, string>.Convert(decimal value)
    {
      return System.Convert.ToString(value, CultureInfo.InvariantCulture);
    }


    // Constructors

    public DecimalAdvancedConverter(IAdvancedConverterProvider provider) : base(provider)
    {
    }
  }
}