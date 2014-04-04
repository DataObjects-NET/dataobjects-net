// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.03

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Sql.Info
{
  public sealed class CollectionsSupportedTypesBuilder
  {
    private readonly HashSet<Type> supportedIntegerTypes = new HashSet<Type>();
    private readonly HashSet<Type> supportedNumericTypes = new HashSet<Type>();
    private readonly HashSet<Type> supportedPrimitiveTypes = new HashSet<Type>();
    private readonly HashSet<Type> supportedSpecializedTypes = new HashSet<Type>();

    /// <summary>
    /// Adds the specified type to the list of supported integer types.
    /// </summary>
    /// <param name="type">The type to add.</param>
    public void AddIntegerType(Type type)
    {
      supportedIntegerTypes.Add(type);
    }

    /// <summary>
    /// Adds the specified type to the list of supported numeric types.
    /// </summary>
    /// <param name="type">The type to add.</param>
    public void AddNumericTypes(Type type)
    {
      supportedNumericTypes.Add(type);
    }

    /// <summary>
    /// Adds the specified type to the list of supported primitive types.
    /// </summary>
    /// <param name="type">The type to add.</param>
    public void AddPrimitiveTypes(Type type)
    {
      supportedPrimitiveTypes.Add(type);
    }

    /// <summary>
    /// Adds the specified type to the list of supported specialized types.
    /// </summary>
    /// <param name="type">The type to add.</param>
    public void AddSpecializedTypes(Type type)
    {
      supportedSpecializedTypes.Add(type);
    }

    #region Helper methods for the formation final a hash sets

    /// <summary>
    /// Gets a read-only hash set containing all supported numeric types.
    /// </summary>
    /// <returns>Hash set containing all supported numeric types.</returns>
    private HashSet<Type> GetSupportedNumericTypes()
    {
      return supportedNumericTypes.Concat(supportedIntegerTypes).ToHashSet();
    }

    /// <summary>
    /// Gets a read-only hash set containing all supported primitive types.
    /// </summary>
    /// <returns>Hash set containing all supported primitive types.</returns>
    private HashSet<Type> GetSupportedPrimitiveTypes()
    {
      return supportedPrimitiveTypes.Concat(supportedNumericTypes).ToHashSet();
    }

    /// <summary>
    /// Gets a read-only hash set containing all supported nullable types.
    /// </summary>
    /// <returns>Hash set containing all supported nullable types.</returns>
    private HashSet<Type> GetSupportedNullableTypes()
    {
      return supportedPrimitiveTypes.Select(type => type.ToNullable()).ToHashSet();
    }

    /// <summary>
    /// Gets a read-only hash set containing all supported primitive and nullable types.
    /// </summary>
    /// <returns>Hash set containing all supported primitive and nullable types.</returns>
    private HashSet<Type> GetSupportedPrimitiveAndNullableTypes()
    {
      return supportedPrimitiveTypes
        .Union(GetSupportedNullableTypes())
        .ToHashSet();
    }

    #endregion

    public CollectionsSupportedTypes Build()
    {
      return new CollectionsSupportedTypes(supportedIntegerTypes,
        GetSupportedNumericTypes(),
        GetSupportedPrimitiveTypes(),
        GetSupportedNullableTypes(),
        GetSupportedPrimitiveAndNullableTypes(),
        supportedSpecializedTypes);
    }

    // Constructors

    public CollectionsSupportedTypesBuilder()
    {
    }
  }
}
