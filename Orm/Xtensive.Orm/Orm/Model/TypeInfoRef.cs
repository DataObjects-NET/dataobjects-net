// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using System;
using System.Diagnostics;



namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="TypeInfo"/> instance.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("TypeName = {TypeName}")]
  public readonly struct TypeInfoRef : IEquatable<TypeInfoRef>
  {
    /// <summary>
    /// Name of the type.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Resolves this instance to <see cref="TypeInfo"/> object within specified <paramref name="model"/>.
    /// </summary>
    /// <param name="model">Domain model.</param>
    public TypeInfo Resolve(DomainModel model) =>
      model.Types.TryGetValue(TypeName, out var type)
        ? type
        : throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "type", TypeName));

    /// <summary>
    /// Creates reference for <see cref="TypeInfo"/>.
    /// </summary>
    public static implicit operator TypeInfoRef (TypeInfo typeInfo) => new TypeInfoRef(typeInfo);

    #region Equality members, ==, !=

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(TypeInfoRef x, TypeInfoRef y) => !Equals(x, y);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(TypeInfoRef x, TypeInfoRef y) => Equals(x, y);

    /// <inheritdoc/>
    public bool Equals(TypeInfoRef other) =>
        TypeName == other.TypeName;

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      obj is TypeInfoRef other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => TypeName.GetHashCode();

    #endregion

    /// <inheritdoc/>
    public override string ToString() => $"Type '{TypeName}'";


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="typeInfo"><see cref="TypeInfo"/> object to make reference for.</param>
    public TypeInfoRef(TypeInfo typeInfo)
    {
      TypeName = typeInfo.Name;      
    }
  }
}
