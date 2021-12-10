// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2007.09.21

using System;
using System.Diagnostics;
using System.Globalization;
using Xtensive.Core;



namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Loosely-coupled reference that describes <see cref="ColumnInfo"/> instance.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("TypeName = {TypeName}, FieldName = {FieldName}, ColumnName = {ColumnName}, CultureInfo = {CultureInfo}")]
  public readonly struct ColumnInfoRef: IEquatable<ColumnInfoRef>
  {
    /// <summary>
    /// Gets type name of reflecting <see cref="TypeInfo"/>.
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Gets name of the <see cref="FieldInfo"/>.
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// Gets name of the <see cref="ColumnInfo"/>.
    /// </summary>
    public string ColumnName { get; }

    /// <summary>
    /// Gets <see cref="CultureInfo"/> info of the <see cref="ColumnInfo"/>.
    /// </summary>
    public CultureInfo CultureInfo { get; }

    /// <summary>
    /// Resolves this instance to <see cref="ColumnInfo"/> object within specified <paramref name="model"/>.
    /// </summary>
    /// <param name="model">Domain model.</param>
    public ColumnInfo Resolve(DomainModel model) =>
      !model.Types.TryGetValue(TypeName, out var type)
        ? throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "type", TypeName))
        : !type.Fields.TryGetValue(FieldName, out var field)
          ? throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "field", FieldName))
          : field.Column ?? throw new InvalidOperationException(string.Format(Strings.ExCouldNotResolveXYWithinDomain, "column", ColumnName));

    /// <summary>
    /// Creates reference for <see cref="ColumnInfo"/>.
    /// </summary>
    public static implicit operator ColumnInfoRef(ColumnInfo columnInfo) => 
      new ColumnInfoRef(columnInfo);

    #region Equality members, ==, !=

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator !=(in ColumnInfoRef x, in ColumnInfoRef y) => !x.Equals(y);

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="x">The x.</param>
    /// <param name="y">The y.</param>
    /// <returns>
    /// The result of the operator.
    /// </returns>
    public static bool operator ==(in ColumnInfoRef x, in ColumnInfoRef y) => x.Equals(y);

    /// <inheritdoc/>
    public bool Equals(ColumnInfoRef other) =>
        FieldName == other.FieldName && TypeName == other.TypeName;

    /// <inheritdoc/>
    public override bool Equals(object obj) =>
      obj is ColumnInfoRef other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() =>
      HashCode.Combine(FieldName, TypeName);

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
      $"{TypeName}.{FieldName} ({ColumnName})";


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this struct.
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
    ///   Initializes a new instance of this struct.
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
