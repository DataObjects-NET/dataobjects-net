// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.16

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Metadata;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Reads and writes assembly versions to a storage.
  /// </summary>
  internal static class SchemaVersionAccessor
  {
    public static void SetSchemaVersion(string assemblyName, string version)
    {
      Assembly assembly = GetAssemblyObject(assemblyName);
      if (assembly==null)
        assembly = new Assembly (assemblyName) {Version = version};
      assembly.Version = version;
    }

    public static string GetSchemaVersion(string assemblyName)
    {
      Assembly assembly = GetAssemblyObject(assemblyName);  
      if (assembly==null)
        throw new InvalidOperationException("Schema version is not found.");
      return assembly.Version;
    }

    private static Assembly GetAssemblyObject(string assemblyName)
    {
      Key assemblyKey = Key.Create(typeof(Assembly), Tuple.Create(assemblyName));
      return assemblyKey.Resolve<Assembly>();
    }
  }
}