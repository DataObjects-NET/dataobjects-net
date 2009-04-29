// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal class AssemblyDescriptor : IAssemblyDescriptor
  {
    private readonly Assembly assembly;

    public string Name
    {
      get { return assembly.GetName(false).Name; }
    }

    public string Version
    {
      get { return assembly.GetName(false).Version.ToString(); }
    }

    public IEnumerable<IUpgrader> GetUpgraders()
    {
      return assembly.GetTypes()
        .Where(type => typeof (IUpgrader).IsAssignableFrom(type))
        .Select(type => (IUpgrader) type.TypeInitializer.Invoke(null));
    }


    // Constructors

    public AssemblyDescriptor(Assembly assembly)
    {
      this.assembly = assembly;
    }
  }
}