// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql
{
  /// <summary>
  /// Enumerates supported SQL column types.
  /// </summary>
  [Serializable]
  public struct SqlType
  {
    public readonly string Name;

    /// <summary>
    /// An unknown type.
    /// </summary>
    public static readonly SqlType Unknown = new SqlType("Unknown");

    #region Numeric

    /// <summary>
    /// Boolean.
    /// </summary>
    public static readonly SqlType Boolean = new SqlType("Boolean");
    /// <summary>
    /// Signed byte (8 bit).
    /// </summary>
    public static readonly SqlType Int8 = new SqlType("Int8");
    /// <summary>
    /// Unsigned byte (8 bit).
    /// </summary>
    public static readonly SqlType UInt8 = new SqlType("UInt8");
    /// <summary>
    /// Small integer (16 bit).
    /// </summary>
    public static readonly SqlType Int16 = new SqlType("Int16");
    /// <summary>
    /// Unsigned small integer (16 bit).
    /// </summary>
    public static readonly SqlType UInt16 = new SqlType("UInt16");
    /// <summary>
    /// Integer (32 bit).
    /// </summary>
    public static readonly SqlType Int32 = new SqlType("Int32");
    /// <summary>
    /// Unsigned integer (32 bit).
    /// </summary>
    public static readonly SqlType UInt32 = new SqlType("UInt32");
    /// <summary>
    /// Long integer (64 bit).
    /// </summary>
    public static readonly SqlType Int64 = new SqlType("Int64");
    /// <summary>
    /// Unsigned long (64 bit).
    /// </summary>
    public static readonly SqlType UInt64 = new SqlType("UInt64");
    /// <summary>
    /// Numeric data type with fixed precision and scale.
    /// </summary>
    public static readonly SqlType Decimal = new SqlType("Decimal");

    #endregion

    #region Real

    /// <summary>
    /// Floating point number data from –3.40E + 38 through 3.40E + 38. 
    /// Storage size is 4 bytes.
    /// </summary>
    public static readonly SqlType Float = new SqlType("Float");
    /// <summary>
    /// Floating point number data from - 1.79E + 308 through 1.79E + 308.
    /// Storage size is 8 bytes.
    /// </summary>
    public static readonly SqlType Double = new SqlType("Double");

    #endregion

    #region DateTime

    /// <summary>
    /// Date and time data from January 1, 1753 through December 31, 9999, 
    /// to an accuracy of one three-hundredth of a second (equivalent to 3.33 
    /// milliseconds or 0.00333 seconds). Values are rounded to increments 
    /// of .000, .003, or .007 seconds.
    /// Storage size is 8 bytes. 
    /// </summary>
    public static readonly SqlType DateTime = new SqlType("DateTime");

    /// <summary>
    /// Date and time data from January 1,1 A.D. through December 31, 9999 A.D., 
    /// to an accuracy of 100 nanoseconds.
    /// Storage size is 8 to 10 bytes. 
    /// </summary>
    public static readonly SqlType DateTimeOffset = new SqlType("DateTimeOffset");

    /// <summary>
    /// Datetime interval.
    /// </summary>
    public static readonly SqlType Interval = new SqlType("Interval");

    #endregion

    #region String

    /// <summary>
    /// Fixed-length Unicode character data of n characters. 
    /// n must be a value from 1 through 4,000. Storage size is two times n bytes. 
    /// The SQL-92 synonyms for nchar are national char and national character.
    /// </summary>
    public static readonly SqlType Char = new SqlType("Char");
    /// <summary>
    /// Variable-length Unicode character data of n characters. 
    /// n must be a value from 1 through 4,000. Storage size, in bytes, is two times 
    /// the number of characters entered. The data entered can be 0 characters in length. 
    /// The SQL-92 synonyms for nvarchar are national char varying and national character varying.
    /// </summary>
    public static readonly SqlType VarChar = new SqlType("VarChar");
   
    /// <summary>
    /// Variable-length Unicode data with a maximum length of 2^30 - 1 (1,073,741,823) 
    /// characters. Storage size, in bytes, is two times the number of characters entered. 
    /// </summary>
    public static readonly SqlType VarCharMax = new SqlType("VarCharMax");

    #endregion

    #region Binary

    /// <summary>
    /// Fixed-length binary data of n bytes. n must be a value from 1 through 8,000. 
    /// Storage size is n+4 bytes. 
    /// </summary>
    public static readonly SqlType Binary = new SqlType("Binary");

    /// <summary>
    /// Variable-length binary data of n bytes. n must be a value from 1 through 8,000. 
    /// Storage size is the actual length of the data entered + 4 bytes, not n bytes. 
    /// The data entered can be 0 bytes in length. 
    /// The SQL-92 synonym for varbinary is binary varying.
    /// </summary>
    public static readonly SqlType VarBinary = new SqlType("VarBinary");
    
    /// <summary>
    /// <para>Variable-length binary data from 0 through 2^31-1 (2,147,483,647) bytes.</para>
    /// </summary>
    public static readonly SqlType VarBinaryMax = new SqlType("VarBinaryMax");

    #endregion

    #region Other

    /// <summary>
    /// A globally unique identifier (GUID). 
    /// </summary>
    public static readonly SqlType Guid = new SqlType("Guid");

    #endregion

    #region Override the comparison operator

    /// <summary>
    /// Implements the equality operator.
    /// </summary>
    /// <param name="left">The first argument.</param>
    /// <param name="right">The second argument.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(SqlType left, SqlType right)
    {
      return left.Equals(right);
    }

    /// <summary>
    /// Implements the inequality operator.
    /// </summary>
    /// <param name="left">The first argument.</param>
    /// <param name="right">The second argument.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(SqlType left, SqlType right)
    {
      return !(left==right);
    }

    #endregion

    // Constructors

    public SqlType(string name)
    {
      Name = name;
    }
  }
}