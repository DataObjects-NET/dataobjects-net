// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Resources;

namespace Xtensive.Sql.Dom
{
  [Serializable]
  public class SqlValueType : SqlType
  {
    private SqlDataType dataType;
    private int size;
    private short scale;
    private short precision;

    public SqlDataType DataType {
      get {
        return dataType;
      }
    }

    public int Size {
      get {
        return size;
      }
    }

    public short Scale {
      get {
        return scale;
      }
    }

    public short Precision {
      get {
        return precision;
      }
    }

    public override object Clone()
    {
      return new SqlValueType(dataType, size, precision, scale);
    }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      if (!(obj is SqlValueType))
        return false;
      SqlValueType value = obj as SqlValueType;
      return value.DataType==dataType && value.Size==size && value.Precision==precision && value.Scale==scale;
    }

    public override int GetHashCode()
    {
      unchecked {
        int result = dataType.GetHashCode();
        result = 29*result + size;
        result = 29*result + scale;
        result = 29*result + precision;
        return result;
      }
    }

    public static bool operator ==(SqlValueType left, SqlValueType right)
    {
      if ((object)left!=null)
        return left.Equals(right);
      else if ((object)right!=null)
        return false;
      else
        return true;
    }

    public static bool operator !=(SqlValueType left, SqlValueType right)
    {
      if ((object)left!=null)
        return !left.Equals(right);
      else if ((object)right!=null)
        return true;
      else
        return false;
    }

    public SqlValueType(SqlDataType dataType)
    {
      this.dataType = dataType;
    }
    
    public SqlValueType(SqlDataType dataType, int size)
    {
      if (!(dataType==SqlDataType.Char ||
          dataType==SqlDataType.VarChar ||
          dataType==SqlDataType.Binary ||
          dataType==SqlDataType.VarBinary ||
          dataType==SqlDataType.AnsiChar ||
          dataType==SqlDataType.AnsiVarChar))
        size = 0;
      if (size<0)
        throw new ArgumentException(Strings.ExSizeShouldBeNotNegativeValue, "size");
      
      this.dataType = dataType;
      this.size = size;
    }
    
    public static bool IsNumeric(SqlValueType valueType)
    {
      switch (valueType.DataType) {
        case SqlDataType.Byte:
        case SqlDataType.Decimal:
        case SqlDataType.Double:
        case SqlDataType.Float:
        case SqlDataType.Int16:
        case SqlDataType.Int32:
        case SqlDataType.Int64:
        case SqlDataType.Money:
        case SqlDataType.SByte:
        case SqlDataType.SmallMoney:
        case SqlDataType.UInt16:
        case SqlDataType.UInt32:
        case SqlDataType.UInt64:
          return true;
        default:
          return false;
      }
    }

    public static bool IsExactNumeric(SqlValueType valueType)
    {
      switch (valueType.DataType) {
        case SqlDataType.Byte:
        case SqlDataType.Decimal:
        case SqlDataType.Int16:
        case SqlDataType.Int32:
        case SqlDataType.Int64:
        case SqlDataType.Money:
        case SqlDataType.SByte:
        case SqlDataType.SmallMoney:
        case SqlDataType.UInt16:
        case SqlDataType.UInt32:
        case SqlDataType.UInt64:
          return true;
        default:
          return false;
      }
    }
    
    internal SqlValueType(SqlDataType dataType, int size, short precision, short scale)
    {
      if (dataType!=SqlDataType.Float && dataType!=SqlDataType.Decimal && dataType!=SqlDataType.Double) {
        scale = 0;
        precision = 0;
      }
      else if (dataType!=SqlDataType.Decimal)
        scale = 0;

      this.dataType = dataType;
      this.size = size;
      this.scale = scale;
      this.precision = precision;
    }

    public SqlValueType(SqlDataType dataType, short precision, short scale)
    {
      if (scale<0)
        throw new ArgumentException(Strings.ExScaleShouldBeNonNegativeValue, "scale");
      if (precision<0)
        throw new ArgumentException(Strings.ExPrecisionShouldBeNonNegativeValue, "precision");
      if (scale>precision)
        throw new ArgumentException(Strings.ExTheScaleMustBeLessThanOrEqualToPrecision);
      this.dataType = dataType;
      this.scale = scale;
      this.precision = precision;
    }

  }
}
