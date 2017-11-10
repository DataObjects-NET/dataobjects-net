// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.21

using System;
using Xtensive.Conversion;

namespace Xtensive.Orm.Tests.Core.Conversion
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