// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
  public sealed class TypeInfoRef : IEquatable<TypeInfoRef>
  {
    private const string ToStringFormat = "Type '{0}'";

    /// <summary>
    /// Name of the type.
    /// </summary>
    public string TypeName { get; private set; }

    /// <summary>
    /// Resolves this instance to <see cref="TypeInfo"/> object within specified <paramref name="model"/>.
    /// </summary>
    /// <param name="model">Domain model.</param>
    public TypeInfo Resolve(DomainModel model)
    {
      TypeInfo type;
      if (!model.Types.TryGetValue(TypeName, out type))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "type", TypeName));
      return type;
    }

    /// <summary>
    /// Creates reference for <see cref="TypeInfo"/>.
    /// </summary>
    public static implicit operator TypeInfoRef (TypeInfo typeInfo)
    {
      return new TypeInfoRef(typeInfo);
    }

    #region Equality members, ==, !=

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(TypeInfoRef x, TypeInfoRef y)
    {
      return !Equals(x, y);
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(TypeInfoRef x, TypeInfoRef y)
    {
      return Equals(x, y);
    }

    
    public bool Equals(TypeInfoRef other)
    {
      if (ReferenceEquals(other, null))
        return false;
      return 
        TypeName==other.TypeName;
    }


    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>
    ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as TypeInfoRef);
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      return unchecked( TypeName.GetHashCode() );
    }

    #endregion


    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return string.Format(ToStringFormat, TypeName);
    }


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