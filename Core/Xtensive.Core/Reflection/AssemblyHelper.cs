// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.08

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Resources;

namespace Xtensive.Reflection
{
  /// <summary>
  /// <see cref="Assembly"/> related helper \ extension methods.
  /// </summary>
  public static class AssemblyHelper
  {
    /// <summary>
    /// Finds all dependent assemblies in current <see cref="AppDomain"/>.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <returns>A <see cref="IList{T}"/> of all dependent assemblies.</returns>
    public static IList<Assembly> FindDependentAssemblies(this Assembly assembly)
    {
      ArgumentValidator.EnsureArgumentNotNull(assembly, "assembly");

      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      List<Assembly> dependants = new List<Assembly>(assemblies.Length);
      dependants.Add(assembly);

      for (int asmIndex = 0; asmIndex < assemblies.Length; asmIndex++) {
        AssemblyName[] references = assemblies[asmIndex].GetReferencedAssemblies();
        for (int refIndex = 0; refIndex < references.Length; refIndex++)
          if (references[refIndex].FullName == assembly.FullName)
            dependants.Add(assemblies[asmIndex]);
      }

      return dependants;
    }

    /// <summary>
    /// Finds the types that are subclasses of the specified type and are located within the specified assembly.
    /// The <paramref name="filter"/> is optional additional parameter.
    /// </summary>
    /// <param name="assembly">The assembly.</param>
    /// <param name="baseType">The base type.</param>
    /// <param name="filter">The filter.</param>
    /// <returns>A <see cref="IList{T}"/> of all found types.</returns>
    /// <exception cref="InvalidOperationException">Something went wrong :(</exception>
    public static IList<Type> FindTypes(this Assembly assembly, Type baseType, TypeFilter filter)
    {
      ArgumentValidator.EnsureArgumentNotNull(assembly, "assembly");
      ArgumentValidator.EnsureArgumentNotNull(baseType, "baseType");

      Type[] allTypes;
      try {
        allTypes = assembly.GetTypes();
      }
      catch (Exception e) {
        throw new InvalidOperationException(
          string.Format(Strings.ExCouldNotLoadTypesFromAssembly, assembly.FullName),
          e);
      }

      List<Type> types = new List<Type>(allTypes.Length);

      for (int index = 0; index < allTypes.Length; index++) {
        Type type = allTypes[index];

        if (type != baseType && !(type.IsSubclassOf(baseType) || (baseType.IsInterface && baseType.IsAssignableFrom(type))))
          continue;

        if (baseType.IsAssignableFrom(type)) {
          if (filter != null && !filter(type, null))
            continue;
          types.Add(type);
        }
      }
      return types;
    }

    /// <summary>
    /// Loads the extension assembly.
    /// This method replaces a short name of a calling assembly.
    /// with a <paramref name="extensionAssemblyName"/>,
    /// then loads assembly with such name.
    /// </summary>
    /// <param name="extensionAssemblyName">Name of the extension assembly.</param>
    /// <returns>Loaded assembly.</returns>
    public static Assembly LoadExtensionAssembly(string extensionAssemblyName)
    {
      ArgumentValidator.EnsureArgumentNotNull(extensionAssemblyName, "extensionAssemblyName");
      var mainAssemblyRef = Assembly.GetCallingAssembly().GetName();
      var extensionAssemblyFullName = mainAssemblyRef.FullName.Replace(mainAssemblyRef.Name, extensionAssemblyName);
      return Assembly.Load(extensionAssemblyFullName);
    }
  }
}