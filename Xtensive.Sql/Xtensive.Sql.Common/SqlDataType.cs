// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Enumerates supported SQL column types.
  /// </summary>
  public enum SqlDataType
  {
    /// <summary>
    /// An unknown type.
    /// </summary>
    Unknown = 0,

    // Numeric

    /// <summary>
    /// Boolean (bit).
    /// </summary>
    Boolean = 1,
    /// <summary>
    /// Signed byte (8 bit integer).
    /// </summary>
    SByte = 2,
    /// <summary>
    /// Unsigned byte.
    /// </summary>
    Byte = 3,
    /// <summary>
    /// Small integer (16 bit integer).
    /// </summary>
    Int16 = 4,
    /// <summary>
    /// Unsigned small integer (word).
    /// </summary>
    UInt16 = 5,
    /// <summary>
    /// Integer (32 bit integer).
    /// </summary>
    Int32 = 6,
    /// <summary>
    /// Unsigned integer.
    /// </summary>
    UInt32 = 7,
    /// <summary>
    /// Long integer (64 bit integer).
    /// </summary>
    Int64 = 8,
    /// <summary>
    /// Unsigned long integer.
    /// </summary>
    UInt64 = 9,
    /// <summary>
    /// Numeric data type with fixed precision and scale.
    /// </summary>
    Decimal = 10,

    // Real

    /// <summary>
    /// Floating point number data from –3.40E + 38 through 3.40E + 38. 
    /// Storage size is 4 bytes.
    /// </summary>
    Float = 11,
    /// <summary>
    /// Floating point number data from - 1.79E + 308 through 1.79E + 308.
    /// Storage size is 8 bytes.
    /// </summary>
    Double = 12,

    // Money

    /// <summary>
    /// Monetary data values from - 214,748.3648 through +214,748.3647, 
    /// with accuracy to a ten-thousandth of a monetary unit. 
    /// Storage size is 4 bytes. 
    /// </summary>
    SmallMoney = 13,
    /// <summary>
    /// Monetary data values from -2^63 (-922,337,203,685,477.5808) through
    /// 2^63 - 1 (+922,337,203,685,477.5807), with accuracy to a ten-thousandth of a monetary unit. 
    /// Storage size is 8 bytes.
    /// </summary>
    Money = 14,

    // DateTime

    /// <summary>
    /// Date and time data from January 1, 1900, through June 6, 2079, 
    /// with accuracy to the minute. smalldatetime values with 29.998 seconds 
    /// or lower are rounded down to the nearest minute; values with 29.999 
    /// seconds or higher are rounded up to the nearest minute.
    /// Storage size is 4 bytes. 
    /// </summary>
    SmallDateTime = 15,
    /// <summary>
    /// Date and time data from January 1, 1753 through December 31, 9999, 
    /// to an accuracy of one three-hundredth of a second (equivalent to 3.33 
    /// milliseconds or 0.00333 seconds). Values are rounded to increments 
    /// of .000, .003, or .007 seconds.
    /// Storage size is 8 bytes. 
    /// </summary>
    DateTime = 16,

    // String

    /// <summary>
    /// Fixed-length non-Unicode character data with length of n bytes. 
    /// n must be a value from 1 through 8,000. Storage size is n bytes. 
    /// The SQL-92 synonym for char is character.
    /// </summary>
    AnsiChar = 17,
    /// <summary>
    /// Variable-length non-Unicode character data with length of n bytes. 
    /// n must be a value from 1 through 8,000. Storage size is the actual 
    /// length in bytes of the data entered, not n bytes. The data entered 
    /// can be 0 characters in length. The SQL-92 synonyms for varchar are 
    /// char varying or character varying.
    /// </summary>
    AnsiVarChar = 18,
    /// <summary>
    /// <para>Variable-length non-Unicode data in the code page of the server and 
    /// with a maximum length of 2^31-1 (2,147,483,647) characters.</para>
    /// <para>The same as <see cref="AnsiVarCharMax"/>.</para>
    /// </summary>
    AnsiText = 20,
//    AnsiText = 19,
    /// <summary>
    /// <para>Variable-length non-Unicode data in the code page of the server and 
    /// with a maximum length of 2^31-1 (2,147,483,647) characters.</para>
    /// <para>The same as <see cref="AnsiText"/>.</para>
    /// </summary>
    AnsiVarCharMax = 20,
    /// <summary>
    /// Fixed-length Unicode character data of n characters. 
    /// n must be a value from 1 through 4,000. Storage size is two times n bytes. 
    /// The SQL-92 synonyms for nchar are national char and national character.
    /// </summary>
    Char = 21,
    /// <summary>
    /// Variable-length Unicode character data of n characters. 
    /// n must be a value from 1 through 4,000. Storage size, in bytes, is two times 
    /// the number of characters entered. The data entered can be 0 characters in length. 
    /// The SQL-92 synonyms for nvarchar are national char varying and national character varying.
    /// </summary>
    VarChar = 22,
    /// <summary>
    /// <para>Variable-length Unicode data with a maximum length of 2^30 - 1 (1,073,741,823) 
    /// characters. Storage size, in bytes, is two times the number of characters entered. 
    /// The SQL-92 synonym for ntext is national text.</para>
    /// <para>The same as <see cref="VarCharMax"/>.</para>
    /// </summary>
    Text = 24,
//    Text = 23,
    /// <summary>
    /// Variable-length Unicode data with a maximum length of 2^30 - 1 (1,073,741,823) 
    /// characters. Storage size, in bytes, is two times the number of characters entered. 
    /// <para>The same as <see cref="Text"/>.</para>
    /// </summary>
    VarCharMax = 24,

    // Binary

    /// <summary>
    /// Fixed-length binary data of n bytes. n must be a value from 1 through 8,000. 
    /// Storage size is n+4 bytes. 
    /// </summary>
    Binary = 25,
    /// <summary>
    /// Variable-length binary data of n bytes. n must be a value from 1 through 8,000. 
    /// Storage size is the actual length of the data entered + 4 bytes, not n bytes. 
    /// The data entered can be 0 bytes in length. 
    /// The SQL-92 synonym for varbinary is binary varying.
    /// </summary>
    VarBinary = 26,
    /// <summary>
    /// <para>Variable-length binary data from 0 through 2^31-1 (2,147,483,647) bytes.</para>
    /// <para>The same as <see cref="VarBinaryMax"/>.</para>
    /// </summary>
//    Image = 27,
    Image = 28,
    /// <summary>
    /// <para>Variable-length binary data from 0 through 2^31-1 (2,147,483,647) bytes.</para>
    /// <para>The same as <see cref="Image"/>.</para>
    /// </summary>
    VarBinaryMax = 28,

    // Other

    /// <summary>
    /// A data type that stores values of various SQL Server-supported data types, 
    /// except text, ntext, image, timestamp, and sql_variant. 
    /// </summary>
    Variant = 29,
    /// <summary>
    /// Data type that exposes automatically generated binary numbers, which are 
    /// guaranteed to be unique within a database. timestamp is used typically as 
    /// a mechanism for version-stamping table rows. The storage size is 8 bytes.
    /// </summary>
    TimeStamp = 30,
    /// <summary>
    /// A globally unique identifier (GUID). 
    /// </summary>
    Guid = 31,
    /// <summary>
    /// A parsed representation of an XML document or fragment.
    /// </summary>
    Xml = 32,
    /// <summary>
    /// Datetime interval.
    /// </summary>
    Interval = 33,
  }
}