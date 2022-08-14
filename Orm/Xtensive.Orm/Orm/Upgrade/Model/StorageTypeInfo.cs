// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using System.Globalization;
using Xtensive.Core;

using System.Text;
using Xtensive.Reflection;
using Xtensive.Modelling.Validation;
using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade.Model
{
  /// <summary>
  /// Type of table column.
  /// </summary>
  [Serializable]
  public sealed class StorageTypeInfo : IEquatable<StorageTypeInfo>,
    IValidatable,
    ICloneable
  {
    /// <summary>
    /// Gets the <see cref="StorageTypeInfo"/> with undefined type.
    /// </summary>
    public static StorageTypeInfo Undefined { get { return new StorageTypeInfo(); } }

    /// <summary>
    /// Gets a value indicating whether type is undefined.
    /// </summary>
    public bool IsTypeUndefined { get { return Type==null; } }

    /// <summary>
    /// Gets the type of the data.
    /// </summary>
    public Type Type { get;  private set; }

    /// <summary>
    /// Indicates whether <see cref="Type"/> is nullable.
    /// </summary>
    public bool IsNullable { get; private set; }

    /// <summary>
    /// Gets the length.
    /// </summary>
    public int? Length { get;  private set; }

    /// <summary>
    /// Gets the scale.
    /// </summary>
    public int? Scale { get; private set; }

    /// <summary>
    /// Gets the precision.
    /// </summary>
    public int? Precision { get; private set; }

    /// <summary>
    /// Gets the native type.
    /// </summary>
    public SqlValueType NativeType { get; private set;  }

    /// <inheritdoc/>
    public void Validate()
    {
      // TODO: Implement
    }

    #region Implementation of ICloneable

    /// <inheritdoc/>
    public object Clone()
    {
      if (IsTypeUndefined)
        return Undefined;
      var clone = new StorageTypeInfo(Type, NativeType, IsNullable);
      clone.Length = Length;
      clone.Scale = Scale;
      clone.Precision = Precision;
      return clone;
    }

    #endregion

    #region Equality members

    /// <inheritdoc/>
    public bool Equals(StorageTypeInfo other)
    {
      if (IsTypeUndefined)
        return false;
      if (other is null)
        return false;
      if (ReferenceEquals(this, other))
        return true;
      if (other.IsTypeUndefined)
        return false;
      var isEqual =
        other.Type==Type &&
        other.IsNullable==IsNullable &&
        other.Scale==Scale &&
        other.Precision==Precision &&
        other.Length==Length;

      return isEqual;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (IsTypeUndefined)
        return false;
      if (obj is null)
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      return obj is StorageTypeInfo otherStorageTypeInfo && Equals(otherStorageTypeInfo);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = (Type!=null ? Type.GetHashCode() : 0);
        result = (result * 397) ^ (IsNullable ? 1 : 0);
        if (Length.HasValue)
          result = (result * 397) ^ Length.Value;
        if (Scale.HasValue)
          result = (result * 397) ^ Scale.Value;
        if (Precision.HasValue)
          result = (result * 397) ^ Precision.Value;
        return result;
      }
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(StorageTypeInfo left, StorageTypeInfo right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(StorageTypeInfo left, StorageTypeInfo right)
    {
      return !Equals(left, right);
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      if (IsTypeUndefined)
        return "Type is undefined.";

      var sb = new StringBuilder();
      var type = Type;
      if (type.IsNullable())
        type = type.GetGenericArguments()[0];
      sb.Append(string.Format(Strings.PropertyPairFormat, Strings.Type, type.GetShortName()));
      if (IsNullable)
        sb.Append(Strings.NullableMark);
      sb.Append(Strings.Comma).Append(string.Format(
        Strings.PropertyPairFormat, Strings.Length, Length.HasValue ? Length.Value.ToString() : "null"));
      if (Scale > 0)
        sb.Append(Strings.Comma).Append(string.Format(
          Strings.PropertyPairFormat, Strings.Scale, Scale));
      if (Precision > 0)
        sb.Append(Strings.Comma).Append(string.Format(
          Strings.PropertyPairFormat, Strings.Precision, Precision));
      if (NativeType!=null)
        sb.Append(Strings.Comma).Append(string.Format(
          Strings.PropertyPairFormat, Strings.NativeType, NativeType));
      return sb.ToString();
    }

    // Constructors

    private StorageTypeInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="nativeType">The native type.</param>
    public StorageTypeInfo(Type type, SqlValueType nativeType)
      : this(type, nativeType, type.IsClass || type.IsNullable())
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="nativeType">The native type.</param>
    /// <param name="length">The length.</param>
    public StorageTypeInfo(Type type, SqlValueType nativeType, int? length)
      : this(type, nativeType, type.IsClass || type.IsNullable(), length)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="nativeType">The native type.</param>
    /// <param name="length">The length.</param>
    /// <param name="precision">The precision.</param>
    /// <param name="scale">The scale.</param>
    public StorageTypeInfo(Type type, SqlValueType nativeType, int? length, int? precision, int? scale)
      : this(type, nativeType, type.IsClass || type.IsNullable(), length, precision, scale)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="nativeType">The native type.</param>
    /// <param name="isNullable">Indicates whether type is nullable.</param>
    public StorageTypeInfo(Type type, SqlValueType nativeType, bool isNullable)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (isNullable && type.IsValueType && !type.IsNullable())
        ArgumentValidator.EnsureArgumentIsInRange(true, false, false, "isNullable");
      Type = type;
      IsNullable = isNullable;
      NativeType = nativeType;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="nativeType">The native type.</param>
    /// <param name="isNullable">Indicates whether type is nullable.</param>
    /// <param name="length">The length.</param>
    public StorageTypeInfo(Type type, SqlValueType nativeType, bool isNullable, int? length)
      : this(type, nativeType, isNullable)
    {
      Length = length;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="nativeType">The native type.</param>
    /// <param name="isNullable">Indicates whether type is nullable.</param>
    /// <param name="length">The length.</param>
    /// <param name="precision">The precision.</param>
    /// <param name="scale">The scale.</param>
    public StorageTypeInfo(Type type, SqlValueType nativeType, bool isNullable, int? length, int? precision, int? scale)
      : this(type, nativeType, isNullable, length)
    {
      Scale = scale;
      Precision = precision;
    }
  }
}
