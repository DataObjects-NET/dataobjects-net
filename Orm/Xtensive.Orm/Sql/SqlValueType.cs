// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Sql
{
  /// <summary>
  /// Represents an SQL type with specific <see cref="Length"/>, <see cref="Scale"/> and <see cref="Precision"/>.
  /// </summary>
  [Serializable]
  public sealed class SqlValueType
    : IEquatable<SqlValueType>
  {
    /// <summary>
    /// Gets the <see cref="SqlType"/>.
    /// </summary>
    public SqlType Type { get; private set; }

    /// <summary>
    /// Gets the name of the type in case when <see cref="Type"/> has value <see cref="SqlType.Unknown"/>.
    /// </summary>
    public string TypeName { get; private set; }

    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    public int? Length { get; private set; }

    /// <summary>
    /// Gets the scale.
    /// </summary>
    public int? Scale { get; private set; }

    /// <summary>
    /// Gets the precision.
    /// </summary>
    public int? Precision { get; private set; }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = Type.GetHashCode();
        result = 29*result + Length.GetHashCode();
        result = 29*result + Scale.GetHashCode();
        result = 29*result + Precision.GetHashCode();
        return result;
      }
    }

    /// <inheritdoc/>
    public bool Equals(SqlValueType other)
    {
      if (ReferenceEquals(other, null))
        return false;
      return
        other.Type==Type &&
        other.TypeName==TypeName &&
        other.Length==Length &&
        other.Precision==Precision &&
        other.Scale==Scale;
    }
    
    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return Equals(obj as SqlValueType);
    }
    
    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(SqlValueType left, SqlValueType right)
    {
      if (ReferenceEquals(left, right))
        return true;
      if (ReferenceEquals(left, null))
        return false;
      if (ReferenceEquals(right, null))
        return false;
      return left.Equals(right);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="right">The right.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(SqlValueType left, SqlValueType right)
    {
      return !(left==right);
    }
    
    public static bool IsNumeric(SqlValueType valueType)
    {
      switch (valueType.Type) {
        case SqlType.UInt8:
        case SqlType.Decimal:
        case SqlType.Double:
        case SqlType.Float:
        case SqlType.Int16:
        case SqlType.Int32:
        case SqlType.Int64:
        case SqlType.Int8:
        case SqlType.UInt16:
        case SqlType.UInt32:
        case SqlType.UInt64:
          return true;
        default:
          return false;
      }
    }

    public static bool IsExactNumeric(SqlValueType valueType)
    {
      switch (valueType.Type) {
        case SqlType.UInt8:
        case SqlType.Decimal:
        case SqlType.Int16:
        case SqlType.Int32:
        case SqlType.Int64:
        case SqlType.Int8:
        case SqlType.UInt16:
        case SqlType.UInt32:
        case SqlType.UInt64:
          return true;
        default:
          return false;
      }
    }

    public override string ToString()
    {
      if (TypeName!=null)
        return TypeName;
      if (Length!=null)
        return string.Format("{0}({1})", Type, Length.Value);
      if (Precision!=null)
        return string.Format("{0}({1},{2})", Type, Precision.Value, Scale.Value);
      return Type.ToString();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    public SqlValueType(SqlType type)
      : this(type, null, null, null, null)
    {
    }
 
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="length">The length.</param>
    public SqlValueType(SqlType type, int length)
      : this(type, null, length, null, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="precision">The precision.</param>
    /// <param name="scale">The scale.</param>
    public SqlValueType(SqlType type, int precision, int scale)
      : this(type, null, null, precision, scale)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    public SqlValueType(string typeName)
      : this(SqlType.Unknown, typeName, null, null, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <param name="length">The length.</param>
    public SqlValueType(string typeName, int length)
      : this(SqlType.Unknown, typeName, length, null, null)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <param name="precision">The precision.</param>
    /// <param name="scale">The scale.</param>
    public SqlValueType(string typeName, int precision, int scale)
      : this(SqlType.Unknown, typeName, null, precision, scale)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="typeName">Name of the type.</param>
    /// <param name="length">The length.</param>
    /// <param name="precision">The precision.</param>
    /// <param name="scale">The scale.</param>
    public SqlValueType(SqlType type, string typeName, int? length, int? precision, int? scale)
    {
      if ((type==SqlType.Unknown)!=(typeName!=null))
        throw new ArgumentException(Strings.ExInvalidArgumentsNonNullTypeNameIsAllowedIfAndOnlyIfTypeEqualsSqlTypeUnknown);
      if (precision.HasValue && precision != 0 && length.HasValue && length != 0)
        throw new ArgumentException(Strings.ExInvalidArgumentsPrecisionAndLengthShouldNotBeUsedTogether);
      if (precision.HasValue!=scale.HasValue)
        throw new ArgumentException(Strings.ExInvalidArgumentsScaleAndPrecisionShouldBeUsedTogether);
      if (typeName!=null)
        ArgumentValidator.EnsureArgumentNotNullOrEmpty(typeName, "typeName");
      if (length!=null)
        ArgumentValidator.EnsureArgumentIsGreaterThan(length.Value, 0, "length");
      if (precision!=null)
        ArgumentValidator.EnsureArgumentIsInRange(scale.Value, 0, precision.Value, "scale");
      Type = type;
      TypeName = typeName;
      Length = length;
      Precision = precision;
      Scale = scale;
    }
  }
}
