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
  internal class TestModelAssembly : IAssemblyDescriptor
  {    
    private readonly string modelNamespace;

    public string Name
    {
      get { return modelNamespace.Substring(0, modelNamespace.IndexOf("_")); }
    }

    public string Version
    {
      get { return modelNamespace.Substring(modelNamespace.IndexOf("_")); }
    }

    public IEnumerable<IUpgrader> GetUpgraders()
    {
      return Assembly.GetExecutingAssembly().GetTypes()
        .Where(type => 
          type.Name == modelNamespace &&
          typeof (IUpgrader).IsAssignableFrom(type))
        .Select(type => (IUpgrader) type.TypeInitializer.Invoke(null));
    }

    public TestModelAssembly(string modelNamespace)
    {
      this.modelNamespace = modelNamespace;
    }
  }
}