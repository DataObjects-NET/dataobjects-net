// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.15

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xtensive.Storage.Building.Internals
{
  internal static class AssemblyHelper
  {
    public static IEnumerable<Assembly> GetAssemblies(IEnumerable<Type> types)
    {
      return types.Select(type => type.Assembly).Distinct();
    }
  }
}