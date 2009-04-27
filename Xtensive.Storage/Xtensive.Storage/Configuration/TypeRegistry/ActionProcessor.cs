// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.23

using System;
using System.Collections.Generic;
using Xtensive.Core.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Configuration.TypeRegistry
{
  /// <summary>
  /// <see cref="Xtensive.Storage.Configuration.TypeRegistry"/> <see cref="TypeRegistration"/> processor.
  /// </summary>
  [Serializable]
  internal abstract class ActionProcessor
  {
    /// <summary>
    /// Gets the base interface.
    /// </summary>
    public abstract Type BaseInterface { get; }

    /// <summary>
    /// Gets base type.
    /// </summary>
    public abstract Type BaseType { get; }

    /// <summary>
    /// Processes the specified action in the specified context.
    /// </summary>
    /// <param name="typeRegistration">The action.</param>
    /// <param name="context">The context.</param>
    public virtual void Process(Context context, TypeRegistration typeRegistration)
    {
      IList<Type> types;

      // Find all types from the assembly that are subsclasses of Persistent and are not generic type definitions.
      if (typeRegistration.Namespace.IsNullOrEmpty())
        types =
          AssemblyHelper.FindTypes(typeRegistration.Assembly,
            BaseType,
            (type, filterCriteria) => !type.IsGenericTypeDefinition && (type.IsSubclassOf(BaseType)));
      else
        // The same as above plus namespace filter
        types =
          AssemblyHelper.FindTypes(typeRegistration.Assembly,
            BaseType,
            (type, filterCriteria) => !type.IsGenericTypeDefinition &&
              (type.IsSubclassOf(BaseType) &&
                type.FullName.StartsWith(typeRegistration.Namespace + ".")));


      // Processing all found types
      foreach (Type type in types) {
        ProcessType(context, type);
      }
    }

    protected abstract void ProcessType(Context context, Type type);
  }
}