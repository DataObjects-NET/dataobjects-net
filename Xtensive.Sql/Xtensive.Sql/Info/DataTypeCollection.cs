// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Represents a collection of <see cref="DataTypeInfo"/> objects that describes all data types existing in database.
  /// </summary>
  public class DataTypeCollection : LockableBase, IEnumerable<DataTypeInfo>
  {
    private DataTypeInfo boolean;
    private DataTypeInfo int8;
    private DataTypeInfo uint8;
    private DataTypeInfo int16;
    private DataTypeInfo uint16;
    private DataTypeInfo int32;
    private DataTypeInfo uint32;
    private DataTypeInfo int64;
    private DataTypeInfo uint64;
    private DataTypeInfo @decimal;
    private DataTypeInfo @float;
    private DataTypeInfo @double;
    private DataTypeInfo dateTime;
    private DataTypeInfo interval;
    private DataTypeInfo @char;
    private DataTypeInfo varChar;
    private DataTypeInfo varCharMax;
    private DataTypeInfo binary;
    private DataTypeInfo varBinary;
    private DataTypeInfo varBinaryMax;
    private DataTypeInfo guid;
    
    private readonly Dictionary<string, DataTypeInfo> nativeTypes =
      new Dictionary<string, DataTypeInfo>(32, StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<SqlType, DataTypeInfo> sqlTypes =
      new Dictionary<SqlType, DataTypeInfo>(32);


    /// <summary>
    /// Gets the <see cref="Xtensive.Sql.Info.DataTypeInfo"/> by the specified native type name.
    /// </summary>
    /// <value>The <see cref="DataTypeInfo"/> instance.</value>
    public DataTypeInfo this[string nativeType] {
      get {
        DataTypeInfo result;
        nativeTypes.TryGetValue(nativeType, out result);
        return result;
      }
    }

    /// <summary>
    /// Gets the <see cref="Xtensive.Sql.Info.DataTypeInfo"/> by the specified <see cref="SqlType"/>.
    /// </summary>
    /// <value>The <see cref="DataTypeInfo"/> instance.</value>
    public DataTypeInfo this[SqlType sqlType] {
      get {
        DataTypeInfo result;
        sqlTypes.TryGetValue(sqlType, out result);
        return result;
      }
    }

    /// <summary>
    /// Boolean (bit).
    /// </summary>
    public DataTypeInfo Boolean {
      get { return boolean; }
      set {
        OnSetting(boolean);
        boolean = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Signed Byte (8 bit integer).
    /// </summary>
    public DataTypeInfo Int8 {
      get { return int8; }
      set {
        OnSetting(int8);
        int8 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Unsigned byte.
    /// </summary>
    public DataTypeInfo UInt8 {
      get { return uint8; }
      set {
        OnSetting(uint8);
        uint8 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Small integer (16 bit integer).
    /// </summary>
    public DataTypeInfo Int16 {
      get { return int16; }
      set {
        OnSetting(int16);
        int16 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Unsigned small integer (word).
    /// </summary>
    public DataTypeInfo UInt16 {
      get { return uint16; }
      set {
        OnSetting(uint16);
        uint16 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Integer (32 bit integer).
    /// </summary>
    public DataTypeInfo Int32 {
      get { return int32; }
      set {
        OnSetting(int32);
        int32 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Unsigned integer.
    /// </summary>
    public DataTypeInfo UInt32
    {
      get { return uint32; }
      set
      {
        OnSetting(uint32);
        uint32 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Long integer (64 bit integer).
    /// </summary>
    public DataTypeInfo Int64 {
      get { return int64; }
      set {
        OnSetting(int64);
        int64 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Unsigned long integer.
    /// </summary>
    public DataTypeInfo UInt64 {
      get { return uint64; }
      set {
        OnSetting(uint64);
        uint64 = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Numeric data type with fixed precision and scale.
    /// </summary>
    public DataTypeInfo Decimal {
      get { return @decimal; }
      set {
        OnSetting(@decimal);
        @decimal = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Floating point number data from –3.40E + 38 through 3.40E + 38. 
    /// Storage size is 4 bytes.
    /// </summary>
    public DataTypeInfo Float {
      get { return @float; }
      set {
        OnSetting(@float);
        @float = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Floating point number data from - 1.79E + 308 through 1.79E + 308.
    /// Storage size is 8 bytes.
    /// </summary>
    public DataTypeInfo Double {
      get { return @double; }
      set {
        OnSetting(@double);
        @double = value;
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
    public DataTypeInfo DateTime {
      get { return dateTime; }
      set {
        OnSetting(dateTime);
        dateTime = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// A representation of the interval data type.
    /// </summary>
    public DataTypeInfo Interval
    {
      get { return interval; }
      set {
        OnSetting(interval);
        interval = value;
        OnSet(value);
      }
    }
    
    /// <summary>
    /// Fixed-length Unicode character data of n characters. 
    /// n must be a value from 1 through 4,000. Storage size is two times n bytes. 
    /// The SQL-92 synonyms for nchar are national char and national character.
    /// </summary>
    public DataTypeInfo Char {
      get { return @char; }
      set {
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
    public DataTypeInfo VarChar {
      get { return varChar; }
      set {
        OnSetting(varChar);
        varChar = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length Unicode data with a maximum length of 230 - 1 (1,073,741,823) 
    /// characters. Storage size, in bytes, is two times the number of characters entered. 
    /// </summary>
    public DataTypeInfo VarCharMax {
      get { return varCharMax; }
      set {
        OnSetting(varCharMax);
        varCharMax = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Fixed-length binary data of n bytes. n must be a value from 1 through 8,000. 
    /// Storage size is n+4 bytes. 
    /// </summary>
    public DataTypeInfo Binary {
      get { return binary; }
      set {
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
    public DataTypeInfo VarBinary {
      get { return varBinary; }
      set {
        OnSetting(varBinary);
        varBinary = value;
        OnSet(value);
      }
    }

    /// <summary>
    /// Variable-length binary data from 0 through 231-1 (2,147,483,647) bytes. 
    /// </summary>
    public DataTypeInfo VarBinaryMax
    {
      get { return varBinaryMax; }
      set {
        OnSetting(varBinaryMax);
        varBinaryMax = value;
        OnSet(value);
      }
    }
    
    /// <summary>
    /// A globally unique identifier (GUID). 
    /// </summary>
    public DataTypeInfo Guid
    {
      get { return guid; }
      set {
        OnSetting(guid);
        guid = value;
        OnSet(value);
      }
    }

    private void OnSetting(DataTypeInfo oldValue)
    {
      this.EnsureNotLocked();
      if (oldValue==null)
        return;
      sqlTypes.Remove(oldValue.Type);
      foreach (var type in oldValue.NativeTypes)
        nativeTypes.Remove(type);
    }

    private void OnSet(DataTypeInfo newValue)
    {
      if (newValue==null)
        return;
      sqlTypes[newValue.Type] = newValue;
      foreach (var type in newValue.NativeTypes)
        nativeTypes.Add(type, newValue);
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
      yield return int8;
      yield return uint8;
      yield return int16;
      yield return uint16;
      yield return int32;
      yield return uint32;
      yield return int64;
      yield return uint64;
      yield return @decimal;
      yield return @float;
      yield return @double;
      yield return dateTime;
      yield return @char;
      yield return varChar;
      yield return varCharMax;
      yield return binary;
      yield return varBinary;
      yield return varBinaryMax;
      yield return guid;
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
