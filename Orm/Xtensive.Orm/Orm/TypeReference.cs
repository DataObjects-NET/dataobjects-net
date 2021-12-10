// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
  public readonly struct TypeReference
  {
    /// <summary>
    /// Gets or sets the referenced type.
    /// </summary>
    public TypeInfo Type { get; }

    /// <summary>
    /// Gets or sets the type reference accuracy.
    /// </summary>
    public TypeReferenceAccuracy Accuracy { get; }


    // Constructor

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The referenced type.</param>
    /// <param name="accuracy">The type reference accuracy.</param>
    public TypeReference(TypeInfo type, TypeReferenceAccuracy accuracy)
    {
      Type = type;
      Accuracy = accuracy;
    }
  }
}
