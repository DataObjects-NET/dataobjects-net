// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Represents a collection of <see cref="DataTypeInfo"/> objects that describes all data types existing in database.
  /// </summary>
  public class DataTypeCollection : LockableBase, IEnumerable<DataTypeInfo>
  {
    private RangeDataTypeInfo<bool> boolean;
    private IntegerDataTypeInfo<sbyte> sByte;
    private IntegerDataTypeInfo<byte> @byte;
    private IntegerDataTypeInfo<short> int16;
    private IntegerDataTypeInfo<ushort> uInt16;
    private IntegerDataTypeInfo<int> int32;
    private IntegerDataTypeInfo<uint> uInt32;
    private IntegerDataTypeInfo<long> int64;
    private IntegerDataTypeInfo<ulong> uInt64;
    private FractionalDataTypeInfo<decimal> @decimal;
    private FractionalDataTypeInfo<float> @float;
    private FractionalDataTypeInfo<double> @double;
    private FractionalDataTypeInfo<decimal> smallMoney;
    private FractionalDataTypeInfo<decimal> money;
    private RangeDataTypeInfo<DateTime> smallDateTime;
    private RangeDataTypeInfo<DateTime> dateTime;
    private StreamDataTypeInfo ansiChar;
    private StreamDataTypeInfo ansiVarChar;
    private StreamDataTypeInfo ansiText;
    private StreamDataTypeInfo ansiVarCharMax;
    private StreamDataTypeInfo @char;
    private StreamDataTypeInfo varChar;
    private StreamDataTypeInfo text;
    private StreamDataTypeInfo varCharMax;
    private StreamDataTypeInfo binary;
    private StreamDataTypeInfo varBinary;
    private StreamDataTypeInfo image;
    private StreamDataTypeInfo varBinaryMax;
    private StreamDataTypeInfo variant;
    private StreamDataTypeInfo timeStamp;
    private StreamDataTypeInfo guid;
    private StreamDataTypeInfo xml;
    private RangeDataTypeInfo<TimeSpan> interval;

    private Dictionary<string, DataTypeInfo> nativeTypes = new Dictionary<string, DataTypeInfo>(32, StringComparer.OrdinalIgnoreCase);
    private Dictionary<SqlDataType, DataTypeInfo> sqlTypes = new Dictionary<SqlDataType, DataTypeInfo>(32);


    /// <summary>
    /// Gets the <see cref="Xtensive.Sql.Common.DataTypeInfo"/> by the specified native type name.
    /// </summary>
    /// <value>The <see cref="DataTypeInfo"/> instance.</value>
    public DataTypeInfo this[string nativeType]
    {
      get
      {
        DataTypeInfo result;
        nativeTypes.TryGetValue(nativeType, out result);
        return result;
      }
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Sql.Common.DataTypeInfo"/> by the specified <see cref="SqlDataType"/>.
    /// </summary>
    /// <value>The <see cref="DataTypeInfo"/> instance.</value>
    public DataTypeInfo this[SqlDataType sqlType]
    {
      get
      {
        DataTypeInfo result;
        sqlTypes.TryGetValue(sqlType, out result);
        return result;
      }
    }

    /// <summary>
    /// Boolean (bit).
    /// </summary>
    public RangeDataTypeInfo<bool> Boolean
    {
      get { return boolean; }
      set
      {
        OnSetting(boolean);
        boolean = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Signed Byte (8 bit integer).
    /// </summary>
    public IntegerDataTypeInfo<sbyte> SByte
    {
      get { return sByte; }
      set
      {
        OnSetting(sByte);
        sByte = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Unsigned byte.
    /// </summary>
    public IntegerDataTypeInfo<byte> Byte
    {
      get { return @byte; }
      set
      {
        OnSetting(@byte);
        @byte = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Small integer (16 bit integer).
    /// </summary>
    public IntegerDataTypeInfo<short> Int16
    {
      get { return int16; }
      set
      {
        OnSetting(int16);
        int16 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Unsigned small integer (word).
    /// </summary>
    public IntegerDataTypeInfo<ushort> UInt16
    {
      get { return uInt16; }
      set
      {
        OnSetting(uInt16);
        uInt16 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Integer (32 bit integer).
    /// </summary>
    public IntegerDataTypeInfo<int> Int32
    {
      get { return int32; }
      set
      {
        OnSetting(int32);
        int32 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Unsigned integer.
    /// </summary>
    public IntegerDataTypeInfo<uint> UInt32
    {
      get { return uInt32; }
      set
      {
        OnSetting(uInt32);
        uInt32 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Long integer (64 bit integer).
    /// </summary>
    public IntegerDataTypeInfo<long> Int64
    {
      get { return int64; }
      set
      {
        OnSetting(int64);
        int64 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Unsigned long integer.
    /// </summary>
    public IntegerDataTypeInfo<ulong> UInt64
    {
      get { return uInt64; }
      set
      {
        OnSetting(uInt64);
        uInt64 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Numeric data type with fixed precision and scale.
    /// </summary>
    public FractionalDataTypeInfo<decimal> Decimal
    {
      get { return @decimal; }
      set
      {
        OnSetting(@decimal);
        @decimal = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Floating point number data from –3.40E + 38 through 3.40E + 38. 
    /// Storage size is 4 bytes.
    /// </summary>
    public FractionalDataTypeInfo<float> Float
    {
      get { return @float; }
      set
      {
        OnSetting(@float);
        @float = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Floating point number data from - 1.79E + 308 through 1.79E + 308.
    /// Storage size is 8 bytes.
    /// </summary>
    public FractionalDataTypeInfo<double> Double
    {
      get { return @double; }
      set
      {
        OnSetting(@double);
        @double = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Monetary data values from - 214,748.3648 through +214,748.3647, 
    /// with accuracy to a ten-thousandth of a monetary unit. 
    /// Storage size is 4 bytes. 
    /// </summary>
    public FractionalDataTypeInfo<decimal> SmallMoney
    {
      get { return smallMoney; }
      set
      {
        OnSetting(smallMoney);
        smallMoney = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Monetary data values from -2^63 (-922,337,203,685,477.5808) through
    /// 2^63 - 1 (+922,337,203,685,477.5807), with accuracy to a ten-thousandth of a monetary unit. 
    /// Storage size is 8 bytes.
    /// </summary>
    public FractionalDataTypeInfo<decimal> Money
    {
      get { return money; }
      set
      {
        OnSetting(money);
        money = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Date and time data from January 1, 1900, through June 6, 2079, 
    /// with accuracy to the minute. smalldatetime values with 29.998 seconds 
    /// or lower are rounded down to the nearest minute; values with 29.999 
    /// seconds or higher are rounded up to the nearest minute.
    /// Storage size is 4 bytes. 
    /// </summary>
    public RangeDataTypeInfo<DateTime> SmallDateTime
    {
      get { return smallDateTime; }
      set
      {
        OnSetting(smallDateTime);
        smallDateTime = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Date and time data from January 1, 1753 through December 31, 9999, 
    /// to an accuracy of one three-hundredth of a second (equivalent to 3.33 
    /// milliseconds or 0.00333 seconds). Values are rounded to increments 
    /// of .000, .003, or .007 seconds.
    /// Storage size is 8 bytes. 
    /// </summary>
    public RangeDataTypeInfo<DateTime> DateTime
    {
      get { return dateTime; }
      set
      {
        OnSetting(dateTime);
        dateTime = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Fixed-length non-Unicode character data with length of n bytes. 
    /// n must be a value from 1 through 8,000. Storage size is n bytes. 
    /// The SQL-92 synonym for char is character.
    /// </summary>
    public StreamDataTypeInfo AnsiChar
    {
      get { return ansiChar; }
      set
      {
        OnSetting(ansiChar);
        ansiChar = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length non-Unicode character data with length of n bytes. 
    /// n must be a value from 1 through 8,000. Storage size is the actual 
    /// length in bytes of the data entered, not n bytes. The data entered 
    /// can be 0 characters in length. The SQL-92 synonyms for varchar are 
    /// char varying or character varying.
    /// </summary>
    public StreamDataTypeInfo AnsiVarChar
    {
      get { return ansiVarChar; }
      set
      {
        OnSetting(ansiVarChar);
        ansiVarChar = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length non-Unicode data in the code page of the server and 
    /// with a maximum length of 231-1 (2,147,483,647) characters.
    /// </summary>
    public StreamDataTypeInfo AnsiText
    {
      get { return ansiText; }
      set
      {
        OnSetting(ansiText);
        ansiText = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length non-Unicode data in the code page of the server and 
    /// with a maximum length of 231-1 (2,147,483,647) characters.
    /// </summary>
    public StreamDataTypeInfo AnsiVarCharMax
    {
      get { return ansiVarCharMax; }
      set
      {
        OnSetting(ansiVarCharMax);
        ansiVarCharMax = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Fixed-length Unicode character data of n characters. 
    /// n must be a value from 1 through 4,000. Storage size is two times n bytes. 
    /// The SQL-92 synonyms for nchar are national char and national character.
    /// </summary>
    public StreamDataTypeInfo Char
    {
      get { return @char; }
      set
      {
        OnSetting(@char);
        @char = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length Unicode character data of n characters. 
    /// n must be a value from 1 through 4,000. Storage size, in bytes, is two times 
    /// the number of characters entered. The data entered can be 0 characters in length. 
    /// The SQL-92 synonyms for nvarchar are national char varying and national character varying.
    /// </summary>
    public StreamDataTypeInfo VarChar
    {
      get { return varChar; }
      set
      {
        OnSetting(varChar);
        varChar = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length Unicode data with a maximum length of 230 - 1 (1,073,741,823) 
    /// characters. Storage size, in bytes, is two times the number of characters entered. 
    /// The SQL-92 synonym for ntext is national text.
    /// </summary>
    public StreamDataTypeInfo Text
    {
      get { return text; }
      set
      {
        OnSetting(text);
        text = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length Unicode data with a maximum length of 230 - 1 (1,073,741,823) 
    /// characters. Storage size, in bytes, is two times the number of characters entered. 
    /// </summary>
    public StreamDataTypeInfo VarCharMax
    {
      get { return varCharMax; }
      set
      {
        OnSetting(varCharMax);
        varCharMax = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Fixed-length binary data of n bytes. n must be a value from 1 through 8,000. 
    /// Storage size is n+4 bytes. 
    /// </summary>
    public StreamDataTypeInfo Binary
    {
      get { return binary; }
      set
      {
        OnSetting(binary);
        binary = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length binary data of n bytes. n must be a value from 1 through 8,000. 
    /// Storage size is the actual length of the data entered + 4 bytes, not n bytes. 
    /// The data entered can be 0 bytes in length. 
    /// The SQL-92 synonym for varbinary is binary varying.
    /// </summary>
    public StreamDataTypeInfo VarBinary
    {
      get { return varBinary; }
      set
      {
        OnSetting(varBinary);
        varBinary = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length binary data from 0 through 231-1 (2,147,483,647) bytes. 
    /// </summary>
    public StreamDataTypeInfo Image
    {
      get { return image; }
      set
      {
        OnSetting(image);
        image = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length binary data from 0 through 231-1 (2,147,483,647) bytes. 
    /// </summary>
    public StreamDataTypeInfo VarBinaryMax
    {
      get { return varBinaryMax; }
      set
      {
        OnSetting(varBinaryMax);
        varBinaryMax = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// A data type that stores values of various SQL Server-supported data types, 
    /// except text, ntext, image, timestamp, and sql_variant. 
    /// </summary>
    public StreamDataTypeInfo Variant
    {
      get { return variant; }
      set
      {
        OnSetting(variant);
        variant = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Data type that exposes automatically generated binary numbers, which are 
    /// guaranteed to be unique within a database. timestamp is used typically as 
    /// a mechanism for version-stamping table rows. The storage size is 8 bytes.
    /// </summary>
    public StreamDataTypeInfo TimeStamp
    {
      get { return timeStamp; }
      set
      {
        OnSetting(timeStamp);
        timeStamp = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// A globally unique identifier (GUID). 
    /// </summary>
    public StreamDataTypeInfo Guid
    {
      get { return guid; }
      set
      {
        OnSetting(guid);
        guid = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// A parsed representation of an XML document or fragment.
    /// </summary>
    public StreamDataTypeInfo Xml
    {
      get { return xml; }
      set
      {
        OnSetting(xml);
        xml = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// A representation of the interval data type.
    /// </summary>
    public RangeDataTypeInfo<TimeSpan> Interval
    {
      get { return interval; }
      set
      {
        OnSetting(interval);
        interval = value;
        OnSet(value);
      }
    }

    private void OnSetting(DataTypeInfo value)
    {
      this.EnsureNotLocked();
      if (value == null)
        return;
      sqlTypes.Remove(value.SqlType);
      string[] types = value.GetNativeTypes();
      for (int i = 0, count = types.Length; i < count; i++)
        nativeTypes.Remove(types[i]);
    }

    private void OnSet(DataTypeInfo value)
    {
      if (value==null)
        return;
      sqlTypes[value.SqlType] = value;
      string[] types = value.GetNativeTypes();
      for (int i = 0, count = types.Length; i < count; i++)
        nativeTypes.Add(types[i], value);
    }

    #region IEnumerable<DataTypeInfo> Members

    ///<summary>
    ///Returns an enumerator that iterates through the collection.
    ///</summary>
    ///
    ///<returns>
    ///A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
    ///</returns>
    IEnumerator<DataTypeInfo> IEnumerable<DataTypeInfo>.GetEnumerator()
    {
      yield return boolean;
      yield return sByte;
      yield return @byte;
      yield return int16;
      yield return uInt16;
      yield return int32;
      yield return uInt32;
      yield return int64;
      yield return uInt64;
      yield return @decimal;
      yield return @float;
      yield return @double;
      yield return smallMoney;
      yield return money;
      yield return smallDateTime;
      yield return dateTime;
      yield return ansiChar;
      yield return ansiVarChar;
      yield return ansiText;
      yield return ansiVarCharMax;
      yield return @char;
      yield return varChar;
      yield return text;
      yield return varCharMax;
      yield return binary;
      yield return varBinary;
      yield return image;
      yield return varBinaryMax;
      yield return variant;
      yield return timeStamp;
      yield return guid;
      yield return xml;
      yield break;
    }

    #endregion

    #region IEnumerable Members

    ///<summary>
    ///Returns an enumerator that iterates through a collection.
    ///</summary>
    ///
    ///<returns>
    ///An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.
    ///</returns>
    public IEnumerator GetEnumerator()
    {
      return ((IEnumerable<DataTypeInfo>)this).GetEnumerator();
    }

    #endregion
  }
}
