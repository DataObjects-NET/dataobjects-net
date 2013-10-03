// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Collections
{
  /// <summary>
  /// Abstract base class for <see cref="ITypeRegistrationProcessor"/> implementation.
  /// </summary>
  [Serializable]
  public abstract class TypeRegistrationProcessorBase : ITypeRegistrationProcessor
  {
    /// <summary>
    /// Gets base type.
    /// </summary>
    public abstract Type BaseType { get; }

    /// <summary>
    /// Processes the specified registration in the specified registration context.
    /// </summary>
    /// <param name="registry">The type registry.</param>
    /// <param name="registration">The action.</param>
    public virtual void Process(TypeRegistry registry, TypeRegistration registration)
    {
      var types =
        registration.Type==null
          ? FindTypes(registration.Assembly, BaseType, (type, typeFilter) => IsAcceptable(registration, type))
          : EnumerableUtils.One(registration.Type).Where(t => IsAcceptable(registration, t));
      foreach (var type in types)
        Process(registry, registration, type);
    }

    /// <summary>
    /// Processes the single type registration.
    /// </summary>
    /// <param name="registry">The type registry.</param>
    /// <param name="registration">The registration.</param>
    /// <param name="type">The type.</param>
    protected virtual void Process(TypeRegistry registry, TypeRegistration registration, Type type)
    {
      registry.Register(type);
    }

    /// <summary>
    /// Determines whether the specified type is acceptable for registration.
    /// </summary>
    /// <param name="registration">The currently processed registration.</param>
    /// <param name="type">The type to check.</param>
    /// <returns>
    ///   <see langword="true"/> if the specified type is acceptable for registration;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsAcceptable(TypeRegistration registration, Type type)
    {
      string ns = registration.Namespace;
      return type.IsSubclassOf(BaseType) && (ns.IsNullOrEmpty() || (type.FullName.IndexOf(ns + ".", StringComparison.InvariantCulture) >= 0));
    }

    private static IList<Type> FindTypes(Assembly assembly, Type baseType, TypeFilter filter)
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