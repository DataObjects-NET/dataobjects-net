// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.24

using System;
using System.Globalization;
using Xtensive.Core;

using System.Text;
using Xtensive.Reflection;
using Xtensive.Modelling.Validation;

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
    /// Gets the culture.
    /// </summary>
    public CultureInfo Culture { get; private set; }

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
    public object NativeType { get; private set;  }

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
      var clone = new StorageTypeInfo(Type, IsNullable, NativeType);
      clone.Length = Length;
      clone.Culture = Culture;
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
      if (ReferenceEquals(null, other))
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
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (StorageTypeInfo))
        return false;
      return Equals((StorageTypeInfo) obj);
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
        if (Culture!=null)
          result = (result * 397) ^ Culture.GetHashCode();
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
      if (Culture!=null)
        sb.Append(Strings.Comma).Append(string.Format(
          Strings.PropertyPairFormat, Strings._Culture, Culture));
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


    #region Constructors

    private StorageTypeInfo()
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="nativeType">The native type.</param>
    public StorageTypeInfo(Type type, object nativeType)
      : this(type, type.IsClass || type.IsNullable(), nativeType)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="length">The length.</param>
    /// <param name="nativeType">The native type.</param>
    public StorageTypeInfo(Type type, int? length, object nativeType)
      : this(type, type.IsClass || type.IsNullable(), length, nativeType)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="length">The length.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="nativeType">The native type.</param>
    public StorageTypeInfo(Type type, int? length, CultureInfo culture, object nativeType)
      : this(type, type.IsClass || type.IsNullable(), length, culture, nativeType)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="length">The length.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="precision">The precision.</param>
    /// <param name="nativeType">The native type.</param>
    public StorageTypeInfo(Type type, int? length, int? scale, int? precision, object nativeType)
      : this(type, type.IsClass || type.IsNullable(), length, scale, precision, nativeType)
    {
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="isNullable">Indicates whether type is nullable.</param>
    /// <param name="nativeType">The native type.</param>
    public StorageTypeInfo(Type type, bool isNullable, object nativeType)
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
    /// <param name="isNullable">Indicates whether type is nullable.</param>
    /// <param name="length">The length.</param>
    /// <param name="nativeType">The native type.</param>
    public StorageTypeInfo(Type type, bool isNullable, int? length, object nativeType)
      : this(type, isNullable, nativeType)
    {
      Length = length;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="isNullable">Indicates whether type is nullable.</param>
    /// <param name="length">The length.</param>
    /// <param name="culture">The culture.</param>
    /// <param name="nativeType">The native type.</param>
    public StorageTypeInfo(Type type, bool isNullable, int? length, CultureInfo culture, object nativeType)
      : this(type, isNullable, length, nativeType)
    {
      ArgumentValidator.EnsureArgumentNotNull(culture, "culture");
      Culture = culture;
    }

    /// <summary>
    /// 	Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">Underlying data type.</param>
    /// <param name="isNullable">Indicates whether type is nullable.</param>
    /// <param name="length">The length.</param>
    /// <param name="scale">The scale.</param>
    /// <param name="precision">The precision.</param>
    /// <param name="nativeType">The native type.</param>
    public StorageTypeInfo(Type type, bool isNullable, int? length, int? scale, int? precision, object nativeType)
      : this(type, isNullable, length, nativeType)
    {
      Scale = scale;
      Precision = precision;
    }

    #endregion
  }
}
