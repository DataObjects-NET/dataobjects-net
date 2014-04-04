// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.04.02

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Represents a set of collections that contain data types supported by a particular DBMS.
  /// </summary>
  public class CollectionsSupportedTypes
  {
    /// <summary>
    /// Gets a read-only hash set containing all supported integer types.
    /// </summary>
    public readonly ReadOnlyHashSet<Type> SupportedIntegerTypes;

    /// <summary>
    /// Gets a read-only hash set containing all supported numeric types.
    /// </summary>
    public readonly ReadOnlyHashSet<Type> SupportedNumericTypes;

    /// <summary>
    /// Gets a read-only hash set containing all supported primitive types.
    /// </summary>
    public readonly ReadOnlyHashSet<Type> SupportedPrimitiveTypes;

    /// <summary>
    /// Gets a read-only hash set containing all supported nullable types.
    /// </summary>
    public readonly ReadOnlyHashSet<Type> SupportedNullableTypes;

    /// <summary>
    /// Gets a read-only hash set containing all supported primitive and nullable types.
    /// </summary>
    public readonly ReadOnlyHashSet<Type> SupportedPrimitiveAndNullableTypes;

    /// <summary>
    /// Gets a read-only hash set containing all supported specialized types.
    /// </summary>
    public readonly ReadOnlyHashSet<Type> SupportedSpecializedTypes; 

    /// <summary>
    /// Adds the specified type to the list of supported integer types in a specific database version of the DBMS.
    /// </summary>
    /// <param name="type">The type to add.</param>
    public void AddSupportedIntegerType(Type type)
    {
      if (!SupportedIntegerTypes.Contains(type))
        SupportedIntegerTypes.Add(type);
    }

    public CollectionsSupportedTypes(HashSet<Type> supportedIntegerTypes, HashSet<Type> supportedNumericTypes,
      HashSet<Type> supportedPrimitiveTypes, HashSet<Type> supportedNullableTypes, HashSet<Type> supportedPrimitiveAndNullableTypes,
      HashSet<Type> supportedSpecializedTypes)
    {
      SupportedIntegerTypes = new ReadOnlyHashSet<Type>(supportedIntegerTypes, true);
      SupportedNumericTypes = new ReadOnlyHashSet<Type>(supportedNumericTypes, true);
      SupportedPrimitiveTypes = new ReadOnlyHashSet<Type>(supportedPrimitiveTypes, true);
      SupportedNullableTypes = new ReadOnlyHashSet<Type>(supportedNullableTypes, true);
      SupportedPrimitiveAndNullableTypes = new ReadOnlyHashSet<Type>(supportedPrimitiveAndNullableTypes, true);
      SupportedSpecializedTypes = new ReadOnlyHashSet<Type>(supportedSpecializedTypes, true);
    }
  }
}
