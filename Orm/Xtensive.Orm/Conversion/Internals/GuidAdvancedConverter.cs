// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Roman Churakov
// Created:    2008.01.23

using System;

namespace Xtensive.Conversion
{
  [Serializable]
  internal class GuidAdvancedConverter :
    StrictAdvancedConverterBase<Guid>,
    IAdvancedConverter<Guid, string>
  {
    string IAdvancedConverter<Guid, string>.Convert(Guid value)
    {
      return value.ToString();
    }


    // Constructors

    public GuidAdvancedConverter(IAdvancedConverterProvider provider) : base(provider)
    {
    }
  }
}