// Copyright (C) 2020-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2020.03.26

using NUnit.Framework;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Xtensive.Orm.Tests.Issues.IssueJira0786_AggregatesProblem
{
  public sealed class MaxProcessingTest : AggregatesProblemTestBase
  {
    [Test]
    public void ByteFieldTest() => TestMax(i => i.ByteValue);

    [Test]
    public void SByteFieldTest() => TestMax(i => i.SByteValue);

    [Test]
    public void ShortFieldTest() => TestMax(i => i.ShortValue);

    [Test]
    public void UShortFieldTest() => TestMax(i => i.UShortValue);

    [Test]
    public void IntFieldTest() => TestMax(i => i.IntValue);

    [Test]
    public void UIntFieldTest() => TestMax(i => i.UIntValue);

    [Test]
    public void LongFieldTest() => TestMax(i => i.LongValue);

    [Test]
    public void ULongFieldTest() => TestMax(i => i.ULongValue);

    [Test]
    public void FloatFieldTest() => TestMax(i => i.FloatValue);

    [Test]
    public void DoubleFieldTest() => TestMax(i => i.DoubleValue1);

    [Test]
    public void DecimalFieldTest() => TestMax(i => i.DecimalValue);

    [Test]
    public void NullableByteFieldTest() => TestMax(i => i.NullableByteValue);

    [Test]
    public void NullableSByteFieldTest() => TestMax(i => i.NullableSByteValue);

    [Test]
    public void NullableShortFieldTest() => TestMax(i => i.NullableShortValue);

    [Test]
    public void NullableUShortFieldTest() => TestMax(i => i.NullableUShortValue);

    [Test]
    public void NullableIntFieldTest() => TestMax(i => i.NullableIntValue);

    [Test]
    public void NullableUIntFieldTest() => TestMax(i => i.NullableUIntValue);

    [Test]
    public void NullableLongFieldTest() => TestMax(i => i.NullableLongValue);

    [Test]
    public void NullableULongFieldTest() => TestMax(i => i.NullableULongValue);

    [Test]
    public void NullableFloatFieldTest() => TestMax(i => i.NullableFloatValue);

    [Test]
    public void NullableDoubleFieldTest() => TestMax(i => i.NullableDoubleValue1);

    [Test]
    public void NullableDecimalFieldTest() => TestMax(i => i.NullableDecimalValue);

    [Test]
    public void ByteFieldExpressionTest01() => TestMax(i => i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest02() =>
      TestMax(i => (short) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest03() =>
      TestMax(i => (short) i.ByteValue + (short) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest04() =>
      TestMax(i => (int) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest05() =>
      TestMax(i => (int) i.ByteValue + (int) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest06() =>
      TestMax(i => (long) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest07() =>
      TestMax(i => (long) i.ByteValue + (long) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest08() =>
      TestMax(i => (float) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest09() =>
      TestMax(i => (float) i.ByteValue + (float) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest10() =>
      TestMax(i => (double) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest11() =>
      TestMax(i => (double) i.ByteValue + (double) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest12() =>
      TestMax(i => (decimal) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest13() =>
      TestMax(i => (decimal) i.ByteValue + (decimal) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest14() => TestMax(i => i.ByteValue + i.IntValue);

    [Test]
    public void ByteFieldExpressionTest15() => TestMax(i => i.ByteValue + i.LongValue);

    [Test]
    public void ByteFieldExpressionTest16() => TestMax(i => i.ByteValue + i.FloatValue);

    [Test]
    public void ByteFieldExpressionTest17() => TestMax(i => i.ByteValue + i.DoubleValue1);

    [Test]
    public void ByteFieldExpressionTest18() =>
      TestMax(i => i.ByteValue + i.DecimalValue);

    [Test]
    public void SByteFieldExpressionTest01() =>
      TestMax(i => i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest02() =>
      TestMax(i => (short) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest03() =>
      TestMax(i => (short) i.SByteValue + (short) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest04() =>
      TestMax(i => (int) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest05() =>
      TestMax(i => (int) i.SByteValue + (int) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest06() =>
      TestMax(i => (long) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest07() =>
      TestMax(i => (long) i.SByteValue + (long) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest08() =>
      TestMax(i => (float) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest09() =>
      TestMax(i => (float) i.SByteValue + (float) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest10() =>
      TestMax(i => (double) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest11() =>
      TestMax(i => (double) i.SByteValue + (double) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest12() =>
      TestMax(i => (decimal) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest13() =>
      TestMax(i => (decimal) i.SByteValue + (decimal) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest14() => TestMax(i => i.SByteValue + i.ByteValue);

    [Test]
    public void SByteFieldExpressionTest15() => TestMax(i => i.SByteValue + i.IntValue);

    [Test]
    public void SByteFieldExpressionTest16() => TestMax(i => i.SByteValue + i.LongValue);

    [Test]
    public void SByteFieldExpressionTest17() => TestMax(i => i.SByteValue + i.FloatValue);

    [Test]
    public void SByteFieldExpressionTest18() => TestMax(i => i.SByteValue + i.DoubleValue1);

    [Test]
    public void SByteFieldExpressionTest19() => TestMax(i => i.SByteValue + i.DecimalValue);

    [Test]
    public void ShortFieldExpressionTest01() => TestMax(i => i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest02() =>
      TestMax(i => (int) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest03() =>
      TestMax(i => (int) i.ShortValue + (int) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest04() =>
      TestMax(i => (long) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest05() =>
      TestMax(i => (long) i.ShortValue + (long) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest06() =>
      TestMax(i => (float) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest07() =>
      TestMax(i => (float) i.ShortValue + (float) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest08() =>
      TestMax(i => (double) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest09() =>
      TestMax(i => (double) i.ShortValue + (double) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest10() =>
      TestMax(i => (decimal) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest11() =>
      TestMax(i => (decimal) i.ShortValue + (decimal) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest12() =>
      TestMax(i => i.ShortValue + i.ByteValue);

    [Test]
    public void ShortFieldExpressionTest13() =>
      TestMax(i => i.ShortValue + i.IntValue);

    [Test]
    public void ShortFieldExpressionTest14() =>
      TestMax(i => i.ShortValue + i.LongValue);

    [Test]
    public void ShortFieldExpressionTest15() =>
      TestMax(i => i.ShortValue + i.FloatValue);

    [Test]
    public void ShortFieldExpressionTest16() =>
      TestMax(i => i.ShortValue + i.DoubleValue1);

    [Test]
    public void ShortFieldExpressionTest17() =>
      TestMax(i => i.ShortValue + i.DecimalValue);

    [Test]
    public void UShortFieldExpressionTest01() =>
      TestMax(i => i.ShortValue + i.ShortValue);

    [Test]
    public void UShortFieldExpressionTest02() =>
      TestMax(i => (int) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest03() =>
      TestMax(i => (int) i.UShortValue + (int) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest04() =>
      TestMax(i => (long) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest05() =>
      TestMax(i => (long) i.UShortValue + (long) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest06() =>
      TestMax(i => (float) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest07() =>
      TestMax(i => (float) i.UShortValue + (float) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest08() =>
      TestMax(i => (double) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest09() =>
      TestMax(i => (double) i.UShortValue + (double) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest10() =>
      TestMax(i => (decimal) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest11() =>
      TestMax(i => (decimal) i.UShortValue + (decimal) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest12() => TestMax(i => i.UShortValue + i.ByteValue);

    [Test]
    public void UShortFieldExpressionTest13() => TestMax(i => i.UShortValue + i.IntValue);

    [Test]
    public void UShortFieldExpressionTest14() => TestMax(i => i.UShortValue + i.LongValue);

    [Test]
    public void UShortFieldExpressionTest15() => TestMax(i => i.UShortValue + i.FloatValue);

    [Test]
    public void UShortFieldExpressionTest16() =>
      TestMax(i => i.UShortValue + i.DoubleValue1);

    [Test]
    public void UShortFieldExpressionTest17() =>
      TestMax(i => i.UShortValue + i.DecimalValue);

    [Test]
    public void IntFieldExpressionTest01() =>
      TestMax(i => i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest02() =>
      TestMax(i => (long) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest03() =>
      TestMax(i => (long) i.IntValue * (long) i.IntValue);

    [Test]
    public void IntFieldExpressionTest04() =>
      TestMax(i => (float) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest05() =>
      TestMax(i => (float) i.IntValue * (float) i.IntValue);

    [Test]
    public void IntFieldExpressionTest06() =>
      TestMax(i => (double) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest07() =>
      TestMax(i => (double) i.IntValue * (double) i.IntValue);

    [Test]
    public void IntFieldExpressionTest08() =>
      TestMax(i => (decimal) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest09() =>
      TestMax(i => (decimal) i.IntValue * (decimal) i.IntValue);

    [Test]
    public void IntFieldExpressionTest10() => TestMax(i => i.IntValue * i.ByteValue);

    [Test]
    public void IntFieldExpressionTest11() => TestMax(i => i.IntValue * i.LongValue);

    [Test]
    public void IntFieldExpressionTest12() => TestMax(i => i.IntValue * i.FloatValue);

    [Test]
    public void IntFieldExpressionTest13() => TestMax(i => i.IntValue * i.DoubleValue1);

    [Test]
    public void IntFieldExpressionTest14() => TestMax(i => i.IntValue * i.DecimalValue);

    [Test]
    public void UIntFieldExpressionTest01() => TestMax(i => i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest02() =>
      TestMax(i => (long) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest03() =>
      TestMax(i => (long) i.UIntValue * (long) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest04() =>
      TestMax(i => (float) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest05() =>
      TestMax(i => (float) i.UIntValue * (float) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest06() =>
      TestMax(i => (double) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest07() =>
      TestMax(i => (double) i.UIntValue * (double) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest08() =>
      TestMax(i => (decimal) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest09() =>
      TestMax(i => (decimal) i.UIntValue * (decimal) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest10() => TestMax(i => i.UIntValue + i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest11() => TestMax(i => i.UIntValue * i.LongValue);

    [Test]
    public void UIntFieldExpressionTest12() => TestMax(i => i.UIntValue * i.FloatValue);

    [Test]
    public void UIntFieldExpressionTest13() => TestMax(i => i.UIntValue * i.DoubleValue1);

    [Test]
    public void UIntFieldExpressionTest14() => TestMax(i => i.UIntValue * i.DecimalValue);

    [Test]
    public void LongFieldExpressionTest01() => TestMax(i => i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest02() =>
      TestMax(i => (float) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest03() =>
      TestMax(i => (float) i.LongValue * (float) i.LongValue);

    [Test]
    public void LongFieldExpressionTest04() =>
      TestMax(i => (double) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest05() =>
      TestMax(i => (double) i.LongValue * (double) i.LongValue);

    [Test]
    public void LongFieldExpressionTest06() =>
      TestMax(i => (decimal) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest07() =>
      TestMax(i => (decimal) i.LongValue * (decimal) i.LongValue);

    [Test]
    public void LongFieldExpressionTest08() => TestMax(i => i.LongValue * i.FloatValue);

    [Test]
    public void LongFieldExpressionTest09() => TestMax(i => i.LongValue * i.DoubleValue1);

    [Test]
    public void LongFieldExpressionTest10() => TestMax(i => i.LongValue * i.DecimalValue);

    [Test]
    public void ULongFieldExpressionTest02() =>
      TestMax(i => (float) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest03() =>
      TestMax(i => (float) i.ULongValue * (float) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest04() =>
      TestMax(i => (double) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest05() =>
      TestMax(i => (double) i.ULongValue * (double) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest06() =>
      TestMax(i => (decimal) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest07() =>
      TestMax(i => (decimal) i.ULongValue * (decimal) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest08() => TestMax(i => i.ULongValue * i.FloatValue);

    [Test]
    public void ULongFieldExpressionTest09() => TestMax(i => i.ULongValue * i.DoubleValue1);

    [Test]
    public void ULongFieldExpressionTest10() => TestMax(i => i.ULongValue * i.DecimalValue);

    [Test]
    public void FloatFieldExpressionTest01() =>
      TestMaxWithAccuracy(i => i.FloatValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest02() =>
      TestMaxWithAccuracy(i => (double) i.FloatValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest03() =>
      TestMaxWithAccuracy(i => (double) i.FloatValue * (double) i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest04() =>
      TestMaxWithAccuracy(i => (decimal) i.FloatValue * (decimal) i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest05() =>
      TestMaxWithAccuracy(i => i.FloatValue * i.DoubleValue1, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest06() =>
      TestMaxWithAccuracy(i => (decimal) i.FloatValue * i.DecimalValue, FloatValueAccuracy);

    [Test]
    public void DoubleFieldExpressionTest01() =>
      TestMaxWithAccuracy(i => i.DoubleValue1 * i.DoubleValue1, DoubleValueAccuracy);

    [Test]
    public void DoubleFieldExpressionTest02() =>
      TestMaxWithAccuracy(i => (decimal) i.DoubleValue1 * i.DecimalValue, DoubleValueAccuracy);

    [Test]
    public void DecimalFieldExpressionTest() =>
      TestMax(i => i.DecimalValue * i.DecimalValue);

    [Test]
    public void NullableByteFieldExpressionTest01() =>
      TestMax(i => i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest02() =>
      TestMax(i => (short?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest03() =>
      TestMax(i => (short?) i.NullableByteValue + (short?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest04() =>
      TestMax(i => (int?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest05() =>
      TestMax(i => (int?) i.NullableByteValue + (int?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest06() =>
      TestMax(i => (long?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest07() =>
      TestMax(i => (long?) i.NullableByteValue + (long?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest08() =>
      TestMax(i => (float?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest09() =>
      TestMax(i => (float?) i.NullableByteValue + (float?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest10() =>
      TestMax(i => (double?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest11() =>
      TestMax(i => (double) i.ByteValue + (double) i.ByteValue);

    [Test]
    public void NullableByteFieldExpressionTest12() =>
      TestMax(i => (decimal?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest13() =>
      TestMax(i => (decimal?) i.NullableByteValue + (decimal?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest14() =>
      TestMax(i => i.NullableByteValue + i.NullableIntValue);

    [Test]
    public void NullableByteFieldExpressionTest15() =>
      TestMax(i => i.NullableByteValue + i.NullableLongValue);

    [Test]
    public void NullableByteFieldExpressionTest16() =>
      TestMax(i => i.NullableByteValue + i.NullableFloatValue);

    [Test]
    public void NullableByteFieldExpressionTest17() =>
      TestMax(i => i.NullableByteValue + i.NullableDoubleValue1);

    [Test]
    public void NullableByteFieldExpressionTest18() =>
      TestMax(i => i.NullableByteValue + i.NullableDecimalValue);

    [Test]
    public void NullableSByteFieldExpressionTest01() =>
      TestMax(i => i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest02() =>
      TestMax(i => (short?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest03() =>
      TestMax(i => (short?) i.NullableSByteValue + (short?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest04() =>
      TestMax(i => (int?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest05() =>
      TestMax(i => (int?) i.NullableSByteValue + (int?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest06() =>
      TestMax(i => (long?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest07() =>
      TestMax(i => (long?) i.NullableSByteValue + (long?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest08() =>
      TestMax(i => (float?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest09() =>
      TestMax(i => (float?) i.NullableSByteValue + (float?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest10() =>
      TestMax(i => (double?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest11() =>
      TestMax(i => (double?) i.NullableSByteValue + (double?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest12() =>
      TestMax(i => (decimal?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest13() =>
      TestMax(i => (decimal?) i.NullableSByteValue + (decimal?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest14() =>
      TestMax(i => i.NullableSByteValue + i.NullableByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest15() =>
      TestMax(i => i.NullableSByteValue + i.NullableIntValue);

    [Test]
    public void NullableSByteFieldExpressionTest16() =>
      TestMax(i => i.NullableSByteValue + i.NullableLongValue);

    [Test]
    public void NullableSByteFieldExpressionTest17() =>
      TestMax(i => i.NullableSByteValue + i.NullableFloatValue);

    [Test]
    public void NullableSByteFieldExpressionTest18() =>
      TestMax(i => i.NullableSByteValue + i.NullableDoubleValue1);

    [Test]
    public void NullableSByteFieldExpressionTest19() =>
      TestMax(i => i.NullableSByteValue + i.NullableDecimalValue);

    [Test]
    public void NullableShortFieldExpressionTest01() =>
      TestMax(i => i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest02() =>
      TestMax(i => (int?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest03() =>
      TestMax(i => (int?) i.NullableShortValue + (int?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest04() =>
      TestMax(i => (long?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest05() =>
      TestMax(i => (long?) i.NullableShortValue + (long?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest06() =>
      TestMax(i => (float?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest07() =>
      TestMax(i => (float?) i.NullableShortValue + (float?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest08() =>
      TestMax(i => (double?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest09() =>
      TestMax(i => (double?) i.NullableShortValue + (double?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest10() =>
      TestMax(i => (decimal?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest11() =>
      TestMax(i => (decimal?) i.NullableShortValue + (decimal?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest12() =>
      TestMax(i => i.NullableShortValue + i.NullableByteValue);

    [Test]
    public void NullableShortFieldExpressionTest13() =>
      TestMax(i => i.NullableShortValue + i.NullableIntValue);

    [Test]
    public void NullableShortFieldExpressionTest14() =>
      TestMax(i => i.NullableShortValue + i.NullableLongValue);

    [Test]
    public void NullableShortFieldExpressionTest15() =>
      TestMax(i => i.NullableShortValue + i.NullableFloatValue);

    [Test]
    public void NullableShortFieldExpressionTest16() =>
      TestMax(i => i.NullableShortValue + i.NullableDoubleValue1);

    [Test]
    public void NullableShortFieldExpressionTest17() =>
      TestMax(i => i.NullableShortValue + i.NullableDecimalValue);

    [Test]
    public void NullableUShortFieldExpressionTest01() =>
      TestMax(i => i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest02() =>
      TestMax(i => (int?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest03() =>
      TestMax(i => (int?) i.NullableUShortValue + (int?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest04() =>
      TestMax(i => (long?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest05() =>
      TestMax(i => (long?) i.NullableUShortValue + (long?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest06() =>
      TestMax(i => (float?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest07() =>
      TestMax(i => (float?) i.NullableUShortValue + (float?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest08() =>
      TestMax(i => (double?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest09() =>
      TestMax(i => (double?) i.NullableUShortValue + (double?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest10() =>
      TestMax(i => (decimal?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest11() =>
      TestMax(i => (decimal?) i.NullableUShortValue + (decimal?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest12() =>
      TestMax(i => i.NullableUShortValue + i.NullableByteValue);

    [Test]
    public void NullableUShortFieldExpressionTest13() =>
      TestMax(i => i.NullableUShortValue + i.NullableIntValue);

    [Test]
    public void NullableUShortFieldExpressionTest14() =>
      TestMax(i => i.NullableUShortValue + i.NullableLongValue);

    [Test]
    public void NullableUShortFieldExpressionTest15() =>
      TestMax(i => i.NullableUShortValue + i.NullableFloatValue);

    [Test]
    public void NullableUShortFieldExpressionTest16() =>
      TestMax(i => i.NullableUShortValue + i.NullableDoubleValue1);

    [Test]
    public void NullableUShortFieldExpressionTest17() =>
      TestMax(i => i.NullableUShortValue + i.NullableDecimalValue);

    [Test]
    public void NullableIntFieldExpressionTest01() =>
      TestMax(i => i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest02() =>
      TestMax(i => (long?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest03() =>
      TestMax(i => (long?) i.NullableIntValue * (long?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest04() =>
      TestMax(i => (float?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest05() =>
      TestMax(i => (float?) i.NullableIntValue * (float?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest06() =>
      TestMax(i => (double?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest07() =>
      TestMax(i => (double?) i.NullableIntValue * (double?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest08() =>
      TestMax(i => (decimal?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest09() =>
      TestMax(i => (decimal?) i.NullableIntValue * (decimal?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest10() =>
      TestMax(i => i.NullableIntValue * i.NullableByteValue);

    [Test]
    public void NullableIntFieldExpressionTest11() =>
      TestMax(i => i.NullableIntValue * i.NullableLongValue);

    [Test]
    public void NullableIntFieldExpressionTest12() =>
      TestMax(i => i.NullableIntValue * i.NullableFloatValue);

    [Test]
    public void NullableIntFieldExpressionTest13() =>
      TestMax(i => i.NullableIntValue * i.NullableDoubleValue1);

    [Test]
    public void NullableIntFieldExpressionTest14() =>
      TestMax(i => i.NullableIntValue * i.NullableDecimalValue);

    [Test]
    public void NullableUIntFieldExpressionTest01() =>
      TestMax(i => i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest02() =>
      TestMax(i => (long?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest03() =>
      TestMax(i => (long?) i.NullableUIntValue * (long?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest04() =>
      TestMax(i => (float?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest05() =>
      TestMax(i => (float?) i.NullableUIntValue * (float?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest06() =>
      TestMax(i => (double?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest07() =>
      TestMax(i => (double?) i.NullableUIntValue * (double?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest08() =>
      TestMax(i => (decimal?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest09() =>
      TestMax(i => (decimal?) i.NullableUIntValue * (decimal?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest10() =>
      TestMax(i => i.NullableUIntValue + i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest11() =>
      TestMax(i => i.NullableUIntValue * i.NullableLongValue);

    [Test]
    public void NullableUIntFieldExpressionTest12() =>
      TestMax(i => i.NullableUIntValue * i.NullableFloatValue);

    [Test]
    public void NullableUIntFieldExpressionTest13() =>
      TestMax(i => i.NullableUIntValue * i.NullableDoubleValue1);

    [Test]
    public void NullableUIntFieldExpressionTest14() =>
      TestMax(i => i.NullableUIntValue * i.NullableDecimalValue);

    [Test]
    public void NullableLongFieldExpressionTest01() =>
      TestMax(i => i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest02() =>
      TestMax(i => (float?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest03() =>
      TestMax(i => (float?) i.NullableLongValue * (float?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest04() =>
      TestMax(i => (double?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest05() =>
      TestMax(i => (double?) i.NullableLongValue * (double?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest06() =>
      TestMax(i => (decimal?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest07() =>
      TestMax(i => (decimal?) i.NullableLongValue * (decimal?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest08() =>
      TestMax(i => i.NullableLongValue * i.NullableFloatValue);

    [Test]
    public void NullableLongFieldExpressionTest09() =>
      TestMax(i => i.NullableLongValue * i.NullableDoubleValue1);

    [Test]
    public void NullableLongFieldExpressionTest10() =>
      TestMax(i => i.NullableLongValue * i.NullableDecimalValue);

    [Test]
    public void NullableULongFieldExpressionTest02() =>
      TestMax(i => (float?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest03() =>
      TestMax(i => (float?) i.NullableULongValue * (float?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest04() =>
      TestMax(i => (double?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest05() =>
      TestMax(i => (double?) i.NullableULongValue * (double?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest06() =>
      TestMax(i => (decimal?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest07() =>
      TestMax(i => (decimal?) i.NullableULongValue * (decimal?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest08() =>
      TestMax(i => i.NullableULongValue * i.NullableFloatValue);

    [Test]
    public void NullableULongFieldExpressionTest09() =>
      TestMax(i => i.NullableULongValue * i.NullableDoubleValue1);

    [Test]
    public void NullableULongFieldExpressionTest10() =>
      TestMax(i => i.NullableULongValue * i.NullableDecimalValue);

    [Test]
    public void NullableFloatFieldExpressionTest01() =>
      TestMaxWithAccuracy(i => i.NullableFloatValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest02() =>
      TestMaxWithAccuracy(i => (double?) i.NullableFloatValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest03() =>
      TestMaxWithAccuracy(i => (double?) i.NullableFloatValue * (double?) i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest04() =>
      TestMaxWithAccuracy(i => (decimal?) i.NullableFloatValue * (decimal?) i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest05() =>
      TestMaxWithAccuracy(i => i.NullableFloatValue * i.NullableDoubleValue1, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest06() =>
      TestMaxWithAccuracy(i => (decimal?) i.NullableFloatValue * i.NullableDecimalValue, FloatValueAccuracy);

    [Test]
    public void NullableDoubleFieldExpressionTest01() =>
      TestMaxWithAccuracy(i => i.NullableDoubleValue1 * i.NullableDoubleValue1, DoubleValueAccuracy);

    [Test]
    public void NullableDoubleFieldExpressionTest02() =>
      TestMaxWithAccuracy(i => (decimal?) i.NullableDoubleValue1 * i.NullableDecimalValue, DoubleValueAccuracy);

    [Test]
    public void NullableDecimalFieldExpressionTest() =>
      TestMax(i => i.NullableDecimalValue * i.NullableDecimalValue);


    private void TestMax<TResult>(Expression<Func<TestEntity, TResult>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Max(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Max(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Max(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Max()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Max(t => t)");
    }

    private void TestMaxWithAccuracy<TAccuracy>(Expression<Func<TestEntity, float>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Max(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Max(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Max(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Max()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Max(t => t)");
    }

    private void TestMaxWithAccuracy<TAccuracy>(Expression<Func<TestEntity, float?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Max(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Max(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Max(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Max()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Max(t => t)");
    }

    private void TestMaxWithAccuracy<TAccuracy>(Expression<Func<TestEntity, double>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Max(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Max(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Max(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Max()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Max(t => t)");
    }

    private void TestMaxWithAccuracy<TAccuracy>(Expression<Func<TestEntity, double?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Max(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Max(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Max(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Max()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Max(t => t)");
    }

    private void TestMaxWithAccuracy<TAccuracy>(Expression<Func<TestEntity, decimal>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Max(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Max(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Max(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Max()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Max(t => t)");
    }

    private void TestMaxWithAccuracy<TAccuracy>(Expression<Func<TestEntity, decimal?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Max(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Max(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Max(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Max()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Max(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Max(t => t)");
    }
  }
}