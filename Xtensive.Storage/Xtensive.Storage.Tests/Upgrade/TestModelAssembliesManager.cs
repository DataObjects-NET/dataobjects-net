// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.09

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Xtensive.Storage.Tests.Upgrade
{
  [Serializable]
  internal class TestModelAssembliesManager : IModelAssembliesManager
  {
    public List<IModelAssembly> GetModelAssemblies(IEnumerable<Type> types)
    {
      return Assembly.GetExecutingAssembly().GetTypes()
        .Select(type => type.Namespace)
        .Where(ns => ns!=null && ns.Contains("Model_"))
        .Select(ns => new TestModelAssembly(ns))
        .Cast<IModelAssembly>()
        .ToList();
    }
  }
}