// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using System;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="FieldInfo"/> instance.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("TypeName = {TypeName}, FieldName = {FieldName}")]
  public sealed class FieldInfoRef : IEquatable<FieldInfoRef>
  {
    private const string ToStringFormat = "Field '{0}.{1}'";

    /// <summary>
    /// Name of the type.
    /// </summary>
    public TypeInfoRef TypeRef { get; private set; }

    /// <summary>
    /// Name of the field.
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// Resolves this instance to <see cref="FieldInfo"/> object within specified <paramref name="model"/>.
    /// </summary>
    /// <param name="model">Domain model.</param>
    public FieldInfo Resolve(DomainModel model)
    {
      var type = TypeRef.Resolve(model);
      FieldInfo field;
      if (!type.Fields.TryGetValue(FieldName, out field))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "field", FieldName));
      return field;
    }

    /// <summary>
    /// Creates reference for <see cref="TypeInfo"/>.
    /// </summary>
    public static implicit operator FieldInfoRef(FieldInfo fieldInfo)
    {
      return new FieldInfoRef(fieldInfo);
    }

    #region Equality members, ==, !=

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    public static bool operator !=(FieldInfoRef x, FieldInfoRef y)
    {
      return !Equals(x, y);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    public static bool operator ==(FieldInfoRef x, FieldInfoRef y)
    {
      return Equals(x, y);
    }

    /// <inheritdoc/>
    public bool Equals(FieldInfoRef other)
    {
      if (ReferenceEquals(other, null))
        return false;
      return 
        TypeRef==other.TypeRef
        && FieldName==other.FieldName;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as FieldInfoRef);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return unchecked( FieldName.GetHashCode() + 29*TypeRef.GetHashCode() );
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(ToStringFormat, TypeRef.TypeName, FieldName);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="fieldInfo"><see cref="FieldInfo"/> object to make reference for.</param>
    public FieldInfoRef(FieldInfo fieldInfo)
    {
      TypeRef = new TypeInfoRef(fieldInfo.ReflectedType);
      FieldName = fieldInfo.Name;
    }
  }
}