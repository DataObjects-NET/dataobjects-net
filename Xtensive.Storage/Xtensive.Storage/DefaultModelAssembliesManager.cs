// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.09

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage
{
  [Serializable]
  internal class DefaultModelAssembliesManager : IModelAssembliesManager
  {
    public List<IModelAssembly> GetModelAssemblies(IEnumerable<Type> types)
    {
      return types
        .Select(type => type.Assembly).Distinct()
        .Select(assembly => new ModelAssembly(assembly)).Cast<IModelAssembly>()
        .ToList();
    }
  }
}