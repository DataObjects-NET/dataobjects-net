// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.09

using System;

using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Reference to <see cref="TypeInfo"/> with the specified degree of accuracy.
  /// </summary>
  [Serializable]
  public struct TypeReference
  {
    /// <summary>
    /// Gets or sets the referenced type.
    /// </summary>
    public TypeInfo Type { get; private set; }

    /// <summary>
    /// Gets or sets the type reference accuracy.
    /// </summary>
    public TypeReferenceAccuracy Accuracy { get; private set; }


    // Constructor

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The referenced type.</param>
    /// <param name="accuracy">The type reference accuracy.</param>
    public TypeReference(TypeInfo type, TypeReferenceAccuracy accuracy)
      : this()
    {
      Type = type;
      Accuracy = accuracy;
    }
  }
}