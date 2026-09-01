// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="Type"/>.
  /// </summary>
  [DataContract]
  public sealed class SerializableType
  {
    [DataMember, JsonInclude]
    public string AssemblyQualifiedName;

    #region Cast operators

    /// <summary>
    /// Implicit conversion of <see cref="Type"/> to <see cref="SerializableType"/>.
    /// </summary>
    /// <param name="type">The type to create reference for.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator SerializableType(Type type)
    {
      return new SerializableType { AssemblyQualifiedName = type.AssemblyQualifiedName };
    }

    /// <summary>
    /// Implicit conversion of <see cref="SerializableType"/> to <see cref="Type"/>.
    /// </summary>
    /// <param name="reference">The serializable version to convert.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator Type(SerializableType reference)
    {
      if (reference?.AssemblyQualifiedName is null)
        return null;
      return Type.GetType(reference.AssemblyQualifiedName);
    }

    #endregion
  }
}