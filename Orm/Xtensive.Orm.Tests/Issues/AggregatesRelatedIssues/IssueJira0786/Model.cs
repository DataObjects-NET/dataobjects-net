// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2020.03.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Orm.Tests.Issues.IssueJira0786_AggregatesProblem
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public byte ByteValue { get; set; }

    [Field]
    public sbyte SByteValue { get; set; }

    [Field]
    public short ShortValue { get; set; }

    [Field]
    public ushort UShortValue { get; set; }

    [Field]
    public int IntValue { get; set; }

    [Field]
    public uint UIntValue { get; set; }

    [Field]
    public long LongValue { get; set; }

    [Field]
    public ulong ULongValue { get; set; }

    [Field]
    public float FloatValue { get; set; }

    [Field]
    public double DoubleValue1 { get; set; }

    [Field]
    public double DoubleValue2 { get; set; }

    [Field(Precision = 20, Scale = 1)]
    public decimal DecimalValue { get; set; }

    [Field]
    public byte? NullableByteValue { get; set; }

    [Field]
    public sbyte? NullableSByteValue { get; set; }

    [Field]
    public short? NullableShortValue { get; set; }

    [Field]
    public ushort? NullableUShortValue { get; set; }

    [Field]
    public int? NullableIntValue { get; set; }

    [Field]
    public uint? NullableUIntValue { get; set; }

    [Field]
    public long? NullableLongValue { get; set; }

    [Field]
    public ulong? NullableULongValue { get; set; }

    [Field]
    public float? NullableFloatValue { get; set; }

    [Field]
    public double? NullableDoubleValue1 { get; set; }

    [Field]
    public double? NullableDoubleValue2 { get; set; }

    [Field(Precision = 20, Scale = 1)]
    public decimal? NullableDecimalValue { get; set; }

    public TestEntity(Session session)
      : base(session)
    {
    }
  }
}