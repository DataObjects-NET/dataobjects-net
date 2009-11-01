// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;
using Xtensive.Core.Conversion;

namespace Xtensive.Core.Tests.Conversion
{
  public class TestConverterProvider : AdvancedConverterProvider
  {
    public TestConverterProvider()
    {
      Type t = typeof(StringAdvancedConverter);
      AddHighPriorityLocation(t.Assembly, t.Namespace, true);
    }
  }
}