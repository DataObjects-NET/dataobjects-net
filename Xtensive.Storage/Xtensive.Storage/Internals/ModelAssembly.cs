// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal class ModelAssembly : IModelAssembly
  {
    private readonly Assembly assembly;

    public string AssemblyName
    {
      get { return assembly.GetName(false).Version.ToString(); }
    }

    public string ModelVersion
    {
      get { return assembly.GetName(false).Name; }
    }

    public IEnumerable<ISchemaUpgrader> GetUpgraders()
    {
      return assembly.GetTypes()
        .Where(type => typeof (ISchemaUpgrader).IsAssignableFrom(type))
        .Select(type => (ISchemaUpgrader) type.TypeInitializer.Invoke(null));
    }


    // Constructors

    public ModelAssembly(Assembly assembly)
    {
      this.assembly = assembly;
    }
  }
}