// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.09.21

using System;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="ColumnInfo"/> instance.
  /// </summary>
  [Serializable]
  public sealed class ColumnInfoRef: IEquatable<ColumnInfoRef>
  {
    private readonly string columnName;
    private readonly string fieldName;
    private readonly string typeName;
    private readonly CultureInfo cultureInfo;

    /// <summary>
    /// Gets type name of reflecting <see cref="TypeInfo"/>.
    /// </summary>
    public string TypeName
    {
      get { return typeName; }
    }
  
    /// <summary>
    /// Gets name of the <see cref="FieldInfo"/>.
    /// </summary>
    public string FieldName
    {
      get { return fieldName; }
    }

    /// <summary>
    /// Gets name of the <see cref="ColumnInfo"/>.
    /// </summary>
    public string ColumnName
    {
      get { return columnName; }
    }

    /// <summary>
    /// Gets <see cref="CultureInfo"/> info of the <see cref="ColumnInfo"/>.
    /// </summary>
    public CultureInfo CultureInfo
    {
      get { return cultureInfo; }
    }

    /// <summary>
    /// Creates reference for <see cref="ColumnInfo"/>.
    /// </summary>
    public static implicit operator ColumnInfoRef (ColumnInfo columnInfo)
    {
      return new ColumnInfoRef(columnInfo);
    }

    public static bool operator !=(ColumnInfoRef x, ColumnInfoRef y)
    {
      return !Equals(x, y);
    }

    public static bool operator ==(ColumnInfoRef x, ColumnInfoRef y)
    {
      return Equals(x, y);
    }

    public bool Equals(ColumnInfoRef other)
    {
      if (ReferenceEquals(other,null))
        return false;
      return Equals(fieldName, other.fieldName) && Equals(typeName, other.typeName);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj))
        return true;
      return Equals(obj as ColumnInfoRef);
    }

    public override int GetHashCode()
    {
      return fieldName.GetHashCode() + 29*typeName.GetHashCode();
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="ColumnInfoRef"/> structure.
    /// </summary>
    /// <param name="columnInfo"><see cref="ColumnInfo"/> instance.</param>
    public ColumnInfoRef(ColumnInfo columnInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(columnInfo, "columnInfo");
      typeName = columnInfo.Field.DeclaringType.Name;
      fieldName = columnInfo.Field.Name;
      columnName = columnInfo.Name;
      cultureInfo = columnInfo.CultureInfo;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ColumnInfoRef"/> structure.
    /// </summary>
    /// <param name="typeName"></param>
    /// <param name="columnName"></param>
    public ColumnInfoRef(string typeName, string columnName, CultureInfo cultureInfo)
    {
      this.typeName = typeName;
      fieldName = columnName;
      this.columnName = columnName;
      this.cultureInfo = cultureInfo;
    }
  }
}