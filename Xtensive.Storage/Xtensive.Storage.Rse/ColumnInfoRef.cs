// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.21

using System;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Resources;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="ColumnInfo"/> instance.
  /// </summary>
  [Serializable]
  public sealed class ColumnInfoRef: IEquatable<ColumnInfoRef>
  {
    /// <summary>
    /// Gets type name of reflecting <see cref="TypeInfo"/>.
    /// </summary>
    public string TypeName { get; private set; }

    /// <summary>
    /// Gets name of the <see cref="FieldInfo"/>.
    /// </summary>
    public string FieldName { get; private set; }

    /// <summary>
    /// Gets name of the <see cref="ColumnInfo"/>.
    /// </summary>
    public string ColumnName { get; private set; }

    /// <summary>
    /// Gets <see cref="CultureInfo"/> info of the <see cref="ColumnInfo"/>.
    /// </summary>
    public CultureInfo CultureInfo { get; private set; }

    /// <summary>
    /// Resolves this instance to <see cref="ColumnInfo"/> object within specified <paramref name="domainInfo"/>.
    /// </summary>
    /// <param name="domainInfo">Domain information.</param>
    public ColumnInfo Resolve(DomainInfo domainInfo)
    {
      TypeInfo type;
      if (!domainInfo.Types.TryGetValue(TypeName, out type))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "type", TypeName));
      FieldInfo field;
      if (!type.Fields.TryGetValue(FieldName, out field))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "field", FieldName));
      ColumnInfo column = field.Column;
      if (column == null)
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "column", ColumnName));
      return column;
    }

    /// <summary>
    /// Creates reference for <see cref="ColumnInfo"/>.
    /// </summary>
    public static implicit operator ColumnInfoRef (ColumnInfo columnInfo)
    {
      return new ColumnInfoRef(columnInfo);
    }

    /// <summary>
    ///   Equality operator. Returns <see langword="true"/> if arguments are equal; otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="x">First.</param>
    /// <param name="y">Second</param>
    public static bool operator !=(ColumnInfoRef x, ColumnInfoRef y)
    {
      return !Equals(x, y);
    }

    /// <summary>
    ///   Inequality operator. Returns <see langword="false"/> if arguments are equal; otherwise <see langword="true"/>.
    /// </summary>
    /// <param name="x">First.</param>
    /// <param name="y">Second</param>
    public static bool operator ==(ColumnInfoRef x, ColumnInfoRef y)
    {
      return Equals(x, y);
    }

    /// <inheritdoc/>
    public bool Equals(ColumnInfoRef other)
    {
      if (ReferenceEquals(other,null))
        return false;
      return Equals(FieldName, other.FieldName) && Equals(TypeName, other.TypeName);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as ColumnInfoRef);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return FieldName.GetHashCode() + 29*TypeName.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("TypeName: {1}, FieldName: {0}, ColumnName: {2}", TypeName, FieldName, ColumnName);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="columnInfo">The <see cref="ColumnInfo"/> instance.</param>
    public ColumnInfoRef(ColumnInfo columnInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(columnInfo, "columnInfo");
      TypeName = columnInfo.Field.DeclaringType.Name;
      FieldName = columnInfo.Field.Name;
      ColumnName = columnInfo.Name;
      CultureInfo = columnInfo.CultureInfo;
    }


    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="typeName">Column type name.</param>
    /// <param name="columnName">Column name.</param>
    /// <param name="cultureInfo">The culture info.</param>
    public ColumnInfoRef(string typeName, string columnName, CultureInfo cultureInfo)
    {
      TypeName = typeName;
      FieldName = columnName;
      ColumnName = columnName;
      CultureInfo = cultureInfo;
    }
  }
}