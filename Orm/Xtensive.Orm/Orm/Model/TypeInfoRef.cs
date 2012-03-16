// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;


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

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    public static bool operator !=(TypeInfoRef x, TypeInfoRef y)
    {
      return !Equals(x, y);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    public static bool operator ==(TypeInfoRef x, TypeInfoRef y)
    {
      return Equals(x, y);
    }

    /// <inheritdoc/>
    public bool Equals(TypeInfoRef other)
    {
      if (ReferenceEquals(other, null))
        return false;
      return 
        TypeName==other.TypeName;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as TypeInfoRef);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return unchecked( TypeName.GetHashCode() );
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, TypeName);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="typeInfo"><see cref="TypeInfo"/> object to make reference for.</param>
    public TypeInfoRef(TypeInfo typeInfo)
    {
      TypeName = typeInfo.Name;      
    }
  }
}