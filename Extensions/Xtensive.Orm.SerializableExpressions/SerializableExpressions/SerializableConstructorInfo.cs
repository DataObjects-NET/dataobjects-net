// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.12

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Xtensive.Orm.SerializableExpressions
{
  /// <summary>
  /// A serializable representation of <see cref="ConstructorInfo"/>.
  /// </summary>
  [DataContract]
  public sealed class SerializableConstructorInfo
  {
    [DataMember, JsonInclude]
    public string DeclaredType;

    [DataMember, JsonInclude]
    public string Ctor;

    #region Cast operators

    /// <summary>
    /// Implicit conversion of <see cref="ConstructorInfo"/> to <see cref="SerializableConstructorInfo"/>.
    /// </summary>
    /// <param name="ctor">The type to create reference for.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator SerializableConstructorInfo(ConstructorInfo ctor)
    {
      if (ctor == null)
        return null;
      return new SerializableConstructorInfo {
        DeclaredType = ctor.DeclaringType.AssemblyQualifiedName,
        Ctor = ctor.ToString()
      };
    }

    /// <summary>
    /// Implicit conversion of <see cref="SerializableConstructorInfo"/> to <see cref="ConstructorInfo"/>.
    /// </summary>
    /// <param name="reference">The serializable version to convert.</param>
    /// <returns>The result of conversion.</returns>
    public static implicit operator ConstructorInfo(SerializableConstructorInfo reference)
    {
      if (reference == null)
        return null;

      var name = reference.Ctor;
      var ctor = Type.GetType(reference.DeclaredType).GetConstructors().First(m => m.ToString() == name);
      return ctor;
    }

    #endregion
  }
}