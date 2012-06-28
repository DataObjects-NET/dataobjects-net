// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Represents a collection of <see cref="DataTypeInfo"/> objects that describes all data types existing in database.
  /// </summary>
  public class DataTypeCollection : LockableBase, IEnumerable<DataTypeInfo>
  {
    private readonly Dictionary<string, DataTypeInfo> nativeTypes =
      new Dictionary<string, DataTypeInfo>(32, StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<SqlType, DataTypeInfo> sqlTypes =
      new Dictionary<SqlType, DataTypeInfo>(32);


    /// <summary>
    /// Gets the <see cref="Xtensive.Sql.Info.DataTypeInfo"/> by the specified native type name.
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
    /// Gets the <see cref="Xtensive.Sql.Info.DataTypeInfo"/> by the specified <see cref="SqlType"/>.
    /// </summary>
    /// <value>The <see cref="DataTypeInfo"/> instance.</value>
    public DataTypeInfo this[SqlType sqlType]
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
    public DataTypeInfo Boolean { get; set; }

    /// <summary>
    /// Signed Byte (8 bit integer).
    /// </summary>
    public DataTypeInfo Int8 { get; set; }

    /// <summary>
    /// Unsigned byte.
    /// </summary>
    public DataTypeInfo UInt8 { get; set; }

    /// <summary>
    /// Small integer (16 bit integer).
    /// </summary>
    public DataTypeInfo Int16 { get; set; }

    /// <summary>
    /// Unsigned small integer (word).
    /// </summary>
    public DataTypeInfo UInt16 { get; set; }

    /// <summary>
    /// Integer (32 bit integer).
    /// </summary>
    public DataTypeInfo Int32 { get; set; }

    /// <summary>
    /// Unsigned integer.
    /// </summary>
    public DataTypeInfo UInt32 { get; set; }

    /// <summary>
    /// Long integer (64 bit integer).
    /// </summary>
    public DataTypeInfo Int64 { get; set; }

    /// <summary>
    /// Unsigned long integer.
    /// </summary>
    public DataTypeInfo UInt64 { get; set; }

    /// <summary>
    /// Numeric data type with fixed precision and scale.
    /// </summary>
    public DataTypeInfo Decimal { get; set; }

    /// <summary>
    /// Floating point number data from –3.40E + 38 through 3.40E + 38. 
    /// Storage size is 4 bytes.
    /// </summary>
    public DataTypeInfo Float { get; set; }

    /// <summary>
    /// Floating point number data from - 1.79E + 308 through 1.79E + 308.
    /// Storage size is 8 bytes.
    /// </summary>
    public DataTypeInfo Double { get; set; }

    /// <summary>
    /// Date and time data from January 1, 1753 through December 31, 9999, 
    /// to an accuracy of one three-hundredth of a second (equivalent to 3.33 
    /// milliseconds or 0.00333 seconds). Values are rounded to increments 
    /// of .000, .003, or .007 seconds.
    /// Storage size is 8 bytes. 
    /// </summary>
    public DataTypeInfo DateTime { get; set; }

    /// <summary>
    /// A representation of the interval data type.
    /// </summary>
    public DataTypeInfo Interval { get; set; }

    /// <summary>
    /// Fixed-length Unicode character data of n characters. 
    /// n must be a value from 1 through 4,000. Storage size is two times n bytes. 
    /// The SQL-92 synonyms for nchar are national char and national character.
    /// </summary>
    public DataTypeInfo Char { get; set; }

    /// <summary>
    /// Variable-length Unicode character data of n characters. 
    /// n must be a value from 1 through 4,000. Storage size, in bytes, is two times 
    /// the number of characters entered. The data entered can be 0 characters in length. 
    /// The SQL-92 synonyms for nvarchar are national char varying and national character varying.
    /// </summary>
    public DataTypeInfo VarChar { get; set; }

    /// <summary>
    /// Variable-length Unicode data with a maximum length of 230 - 1 (1,073,741,823) 
    /// characters. Storage size, in bytes, is two times the number of characters entered. 
    /// </summary>
    public DataTypeInfo VarCharMax { get; set; }

    /// <summary>
    /// Fixed-length binary data of n bytes. n must be a value from 1 through 8,000. 
    /// Storage size is n+4 bytes. 
    /// </summary>
    public DataTypeInfo Binary { get; set; }

    /// <summary>
    /// Variable-length binary data of n bytes. n must be a value from 1 through 8,000. 
    /// Storage size is the actual length of the data entered + 4 bytes, not n bytes. 
    /// The data entered can be 0 bytes in length. 
    /// The SQL-92 synonym for varbinary is binary varying.
    /// </summary>
    public DataTypeInfo VarBinary { get; set; }

    /// <summary>
    /// Variable-length binary data from 0 through 231-1 (2,147,483,647) bytes. 
    /// </summary>
    public DataTypeInfo VarBinaryMax { get; set; }

    /// <summary>
    /// A globally unique identifier (GUID). 
    /// </summary>
    public DataTypeInfo Guid { get; set; }

    /// <summary>
    /// Geometry type. 
    /// </summary>
    public DataTypeInfo Geometry { get; set; }

    /// <summary>
    /// Geography type. 
    /// </summary>
    public DataTypeInfo Geography { get; set; }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);

      foreach (DataTypeInfo item in this) {
        if (item==null)
          continue;
        sqlTypes[item.Type] = item;
        foreach (var type in item.NativeTypes)
          nativeTypes.Add(type, item);
      }
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
      yield return Boolean;
      yield return Int8;
      yield return UInt8;
      yield return Int16;
      yield return UInt16;
      yield return Int32;
      yield return UInt32;
      yield return Int64;
      yield return UInt64;
      yield return Decimal;
      yield return Float;
      yield return Double;
      yield return DateTime;
      yield return Char;
      yield return VarChar;
      yield return VarCharMax;
      yield return Binary;
      yield return VarBinary;
      yield return VarBinaryMax;
      yield return Guid;
      yield return Interval;
      yield return Geometry;
      yield return Geography;
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
      return ((IEnumerable<DataTypeInfo>) this).GetEnumerator();
    }

    #endregion
  }
}