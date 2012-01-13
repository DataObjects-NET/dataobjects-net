// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.21

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Provides a <see cref="ValueRange{T}"/>s for standard .Net struct types.
  /// </summary>
  public partial class ValueRange
  {
    /// <summary>
    /// Standard value range for <see cref="bool"/>.
    /// </summary>
    public static readonly ValueRange<bool> Bool = new ValueRange<bool>(false, true);
    /// <summary>
    /// Standard value range for <see cref="char"/>.
    /// </summary>
    public static readonly ValueRange<char> Char = new ValueRange<char>(char.MinValue, char.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="sbyte"/>.
    /// </summary>
    public static readonly ValueRange<sbyte> SByte = new ValueRange<sbyte>(sbyte.MinValue, sbyte.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="short"/>.
    /// </summary>
    public static readonly ValueRange<short> Int16 = new ValueRange<short>(short.MinValue, short.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="int"/>.
    /// </summary>
    public static readonly ValueRange<int> Int32 = new ValueRange<int>(int.MinValue, int.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="long"/>.
    /// </summary>
    public static readonly ValueRange<long> Int64 = new ValueRange<long>(long.MinValue, long.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="byte"/>.
    /// </summary>
    public static readonly ValueRange<byte> Byte = new ValueRange<byte>(byte.MinValue, byte.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="ushort"/>.
    /// </summary>
    public static readonly ValueRange<ushort> UInt16 = new ValueRange<ushort>(ushort.MinValue, ushort.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="uint"/>.
    /// </summary>
    public static readonly ValueRange<uint> UInt32 = new ValueRange<uint>(uint.MinValue, uint.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="ulong"/>.
    /// </summary>
    public static readonly ValueRange<ulong> UInt64 = new ValueRange<ulong>(ulong.MinValue, ulong.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="float"/>.
    /// </summary>
    public static readonly ValueRange<float> Float = new ValueRange<float>(float.MinValue, float.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="double"/>.
    /// </summary>
    public static readonly ValueRange<double> Double = new ValueRange<double>(double.MinValue, double.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="decimal"/>.
    /// </summary>
    public static readonly ValueRange<decimal> Decimal = new ValueRange<decimal>(decimal.MinValue, decimal.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="System.DateTime"/>
    /// </summary>
    public static readonly ValueRange<System.DateTime> DateTime =
      new ValueRange<System.DateTime>(System.DateTime.MinValue, System.DateTime.MaxValue);
    /// <summary>
    /// Standard value range for <see cref="System.TimeSpan"/>
    /// </summary>
    public static readonly ValueRange<System.TimeSpan> TimeSpan =
      new ValueRange<System.TimeSpan>(System.TimeSpan.MinValue, System.TimeSpan.MaxValue);
  }
}