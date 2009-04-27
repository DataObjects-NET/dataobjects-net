// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.08

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Reflection
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
  }
}