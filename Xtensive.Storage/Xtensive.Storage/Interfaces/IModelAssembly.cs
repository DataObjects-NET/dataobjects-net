// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.09

using System.Collections.Generic;

namespace Xtensive.Storage
{
  internal interface IModelAssembly
  {
    string AssemblyName { get;}

    string ModelVersion { get;}

    IEnumerable<ISchemaUpgrader> GetUpgraders();
  }
}