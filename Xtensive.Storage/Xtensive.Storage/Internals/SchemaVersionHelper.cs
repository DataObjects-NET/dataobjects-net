// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.13

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Storage.Building.Internals;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Contains schema versions related methods.
  /// </summary>
  internal static class SchemaVersionHelper
  {    
    public static Dictionary<Assembly, string> GetSchemaVersions(IEnumerable<Assembly> assemblies)
    {
      var result = new Dictionary<Assembly, string>();      
      foreach (var assembly in assemblies) {
        string assemblyName = assembly.GetName(false).Name;        
        string schemaVersion = SchemaVersionAccessor.GetSchemaVersion(assemblyName);
        result.Add(assembly, schemaVersion);
      }
      return result;
    }

    public static void SetInitialSchemaVersion(IEnumerable<Type> types)
    {
     var assemblies = AssemblyHelper.GetAssemblies(types);
      foreach (var assembly in assemblies) {
        string assemblyName = assembly.GetName(false).Name;
        string assemblyVersion = assembly.GetName(false).Version.ToString();
        SchemaVersionAccessor.SetSchemaVersion(assemblyName, assemblyVersion);
      }
    } 

    public static void CheckSchemaVersionIsActual(IEnumerable<Type> types)
    {
      var assemblies = AssemblyHelper.GetAssemblies(types);
      foreach (var assembly in assemblies) {
        string assemblyName = assembly.GetName(false).Name;
        string assemblyVersion = assembly.GetName(false).Version.ToString();
        string schemaVersion = SchemaVersionAccessor.GetSchemaVersion(assemblyName);

        if (schemaVersion!=assemblyVersion)
          throw new InvalidOperationException(string.Format(
            Strings.ActualSchemaVersionOfAssemblyXIsExpectedToBeYButCurrentlyIsZ, 
            assemblyName, assemblyVersion, schemaVersion));
      }
    }
  }
}