// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.21

using System;
using System.Diagnostics;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model.Resources;

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="ColumnInfo"/> instance.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("TypeName = {TypeName}, FieldName = {FieldName}, ColumnName = {ColumnName}, CultureInfo = {CultureInfo}")]
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
    /// Resolves this instance to <see cref="ColumnInfo"/> object within specified <paramref name="model"/>.
    /// </summary>
    /// <param name="model">Domain model.</param>
    public ColumnInfo Resolve(DomainModel model)
    {
      TypeInfo type;
      if (!model.Types.TryGetValue(TypeName, out type))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "type", TypeName));
      FieldInfo field;
      if (!type.Fields.TryGetValue(FieldName, out field))
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "field", FieldName));
      if (field.Column == null)
        throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "column", ColumnName));
      return field.Column;
    }

    /// <summary>
    /// Creates reference for <see cref="ColumnInfo"/>.
    /// </summary>
    public static implicit operator ColumnInfoRef (ColumnInfo columnInfo)
    {
      return new ColumnInfoRef(columnInfo);
    }

    #region Equality members, ==, !=

    /// <see cref="ClassDocTemplate.OperatorEq" copy="true" />
    public static bool operator !=(ColumnInfoRef x, ColumnInfoRef y)
    {
      return !Equals(x, y);
    }

    /// <see cref="ClassDocTemplate.OperatorNeq" copy="true" />
    public static bool operator ==(ColumnInfoRef x, ColumnInfoRef y)
    {
      return Equals(x, y);
    }

    /// <inheritdoc/>
    public bool Equals(ColumnInfoRef other)
    {
      if (ReferenceEquals(other, null))
        return false;
      return 
        FieldName==other.FieldName && TypeName==other.TypeName;
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
      return unchecked( FieldName.GetHashCode() + 29*TypeName.GetHashCode() );
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("{0}.{1} ({2})", TypeName, FieldName, ColumnName);
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