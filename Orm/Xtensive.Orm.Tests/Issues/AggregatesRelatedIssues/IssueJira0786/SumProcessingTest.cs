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
  public sealed class SumProcessingTest : AggregatesProblemTestBase
  {
    [Test]
    public void ByteFieldTest() => TestSum(i => i.ByteValue);

    [Test]
    public void SByteFieldTest() => TestSum(i => i.SByteValue);

    [Test]
    public void ShortFieldTest() => TestSum(i => i.ShortValue);

    [Test]
    public void UShortFieldTest() => TestSum(i => i.UShortValue);

    [Test]
    public void IntFieldTest() => TestSum(i => i.IntValue);

    [Test]
    public void UIntFieldTest() => TestSum(i => i.UIntValue);

    [Test]
    public void LongFieldTest() => TestSum(i => i.LongValue);

    [Test]
    public void FloatFieldTest() => TestSum(i => i.FloatValue);

    [Test]
    public void DoubleFieldTest() => TestSum(i => i.DoubleValue1);

    [Test]
    public void DecimalFieldTest() => TestSum(i => i.DecimalValue);

    [Test]
    public void NullableByteFieldTest() => TestSum(i => i.NullableByteValue);

    [Test]
    public void NullableSByteFieldTest() => TestSum(i => i.NullableSByteValue);

    [Test]
    public void NullableShortFieldTest() => TestSum(i => i.NullableShortValue);

    [Test]
    public void NullableUShortFieldTest() => TestSum(i => i.NullableUShortValue);

    [Test]
    public void NullableIntFieldTest() => TestSum(i => i.NullableIntValue);

    [Test]
    public void NullableUIntFieldTest() => TestSum(i => i.NullableUIntValue);

    [Test]
    public void NullableLongFieldTest() => TestSum(i => i.NullableLongValue);

    [Test]
    public void NullableFloatFieldTest() =>
      TestSumWithAccuracy(i => i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableDoubleFieldTest() => TestSum(i => i.NullableDoubleValue1);

    [Test]
    public void NullableDecimalFieldTest() => TestSum(i => i.NullableDecimalValue);

    [Test]
    public void ByteFieldExpressionTest01() => TestSum(i => i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest02() =>
      TestSum(i => (short) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest03() =>
      TestSum(i => (short) i.ByteValue + (short) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest04() =>
      TestSum(i => (int) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest05() =>
      TestSum(i => (int) i.ByteValue + (int) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest06() =>
      TestSum(i => (long) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest07() =>
      TestSum(i => (long) i.ByteValue + (long) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest08() =>
      TestSumWithAccuracy(i => (float) i.ByteValue + i.ByteValue, FloatValueAccuracy);

    [Test]
    public void ByteFieldExpressionTest09() =>
      TestSumWithAccuracy(i => (float) i.ByteValue + (float) i.ByteValue, FloatValueAccuracy);

    [Test]
    public void ByteFieldExpressionTest10() =>
      TestSum(i => (double) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest11() =>
      TestSum(i => (double) i.ByteValue + (double) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest12() =>
      TestSum(i => (decimal) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest13() =>
      TestSum(i => (decimal) i.ByteValue + (decimal) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest14() => TestSum(i => i.ByteValue + i.IntValue);

    [Test]
    public void ByteFieldExpressionTest15() => TestSum(i => i.ByteValue + i.LongValue);

    [Test]
    public void ByteFieldExpressionTest16() =>
      TestSumWithAccuracy(i => i.ByteValue + i.FloatValue, FloatValueAccuracy);

    [Test]
    public void ByteFieldExpressionTest17() => TestSum(i => i.ByteValue + i.DoubleValue1);

    [Test]
    public void ByteFieldExpressionTest18() => TestSum(i => i.ByteValue + i.DecimalValue);

    [Test]
    public void SByteFieldExpressionTest01() => TestSum(i => i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest02() =>
      TestSum(i => (short) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest03() =>
      TestSum(i => (short) i.SByteValue + (short) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest04() =>
      TestSum(i => (int) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest05() =>
      TestSum(i => (int) i.SByteValue + (int) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest06() =>
      TestSum(i => (long) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest07() =>
      TestSum(i => (long) i.SByteValue + (long) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest08() =>
      TestSumWithAccuracy(i => (float) i.SByteValue + i.SByteValue, FloatValueAccuracy);

    [Test]
    public void SByteFieldExpressionTest09() =>
      TestSumWithAccuracy(i => (float) i.SByteValue + (float) i.SByteValue, FloatValueAccuracy);

    [Test]
    public void SByteFieldExpressionTest10() =>
      TestSum(i => (double) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest11() =>
      TestSum(i => (double) i.SByteValue + (double) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest12() =>
      TestSum(i => (decimal) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest13() =>
      TestSum(i => (decimal) i.SByteValue + (decimal) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest14() => TestSum(i => i.SByteValue + i.ByteValue);

    [Test]
    public void SByteFieldExpressionTest15() => TestSum(i => i.SByteValue + i.IntValue);

    [Test]
    public void SByteFieldExpressionTest16() => TestSum(i => i.SByteValue + i.LongValue);

    [Test]
    public void SByteFieldExpressionTest17() =>
      TestSumWithAccuracy(i => i.SByteValue + i.FloatValue, FloatValueAccuracy);

    [Test]
    public void SByteFieldExpressionTest18() => TestSum(i => i.SByteValue + i.DoubleValue1);

    [Test]
    public void SByteFieldExpressionTest19() => TestSum(i => i.SByteValue + i.DecimalValue);

    [Test]
    public void ShortFieldExpressionTest01() => TestSum(i => i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest02() =>
      TestSum(i => (int) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest03() =>
      TestSum(i => (int) i.ShortValue + (int) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest04() =>
      TestSum(i => (long) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest05() =>
      TestSum(i => (long) i.ShortValue + (long) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest06() =>
      TestSumWithAccuracy(i => (float) i.ShortValue + i.ShortValue, FloatValueAccuracy);

    [Test]
    public void ShortFieldExpressionTest07() =>
      TestSumWithAccuracy(i => (float) i.ShortValue + (float) i.ShortValue, FloatValueAccuracy);

    [Test]
    public void ShortFieldExpressionTest08() =>
      TestSum(i => (double) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest09() =>
      TestSum(i => (double) i.ShortValue + (double) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest10() =>
      TestSum(i => (decimal) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest11() =>
      TestSum(i => (decimal) i.ShortValue + (decimal) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest12() => TestSum(i => i.ShortValue + i.ByteValue);

    [Test]
    public void ShortFieldExpressionTest13() => TestSum(i => i.ShortValue + i.IntValue);

    [Test]
    public void ShortFieldExpressionTest14() => TestSum(i => i.ShortValue + i.LongValue);

    [Test]
    public void ShortFieldExpressionTest15() =>
      TestSumWithAccuracy(i => i.ShortValue + i.FloatValue, FloatValueAccuracy);

    [Test]
    public void ShortFieldExpressionTest16() => TestSum(i => i.ShortValue + i.DoubleValue1);

    [Test]
    public void ShortFieldExpressionTest17() => TestSum(i => i.ShortValue + i.DecimalValue);

    [Test]
    public void UShortFieldExpressionTest01() => TestSum(i => i.ShortValue + i.ShortValue);

    [Test]
    public void UShortFieldExpressionTest02() =>
      TestSum(i => (int) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest03() =>
      TestSum(i => (int) i.UShortValue + (int) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest04() =>
      TestSum(i => (long) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest05() =>
      TestSum(i => (long) i.UShortValue + (long) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest06() =>
      TestSum(i => (float) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest07() =>
      TestSumWithAccuracy(i => (float) i.UShortValue + (float) i.UShortValue, FloatValueAccuracy);

    [Test]
    public void UShortFieldExpressionTest08() =>
      TestSum(i => (double) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest09() =>
      TestSum(i => (double) i.UShortValue + (double) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest10() =>
      TestSum(i => (decimal) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest11() =>
      TestSum(i => (decimal) i.UShortValue + (decimal) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest12() => TestSum(i => i.UShortValue + i.ByteValue);

    [Test]
    public void UShortFieldExpressionTest13() => TestSum(i => i.UShortValue + i.IntValue);

    [Test]
    public void UShortFieldExpressionTest14() => TestSum(i => i.UShortValue + i.LongValue);

    [Test]
    public void UShortFieldExpressionTest15() =>
      TestSumWithAccuracy(i => i.UShortValue + i.FloatValue, FloatValueAccuracy);

    [Test]
    public void UShortFieldExpressionTest16() =>
      TestSum(i => i.UShortValue + i.DoubleValue1);

    [Test]
    public void UShortFieldExpressionTest17() =>
      TestSum(i => i.UShortValue + i.DecimalValue);

    [Test]
    public void IntFieldExpressionTest01() => TestSum(i => i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest02() =>
      TestSum(i => (long) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest03() =>
      TestSum(i => (long) i.IntValue * (long) i.IntValue);

    [Test]
    public void IntFieldExpressionTest04() =>
      TestSumWithAccuracy(i => (float) i.IntValue * i.IntValue, FloatValueAccuracy);

    [Test]
    public void IntFieldExpressionTest05() =>
      TestSumWithAccuracy(i => (float) i.IntValue * (float) i.IntValue, FloatValueAccuracy);

    [Test]
    public void IntFieldExpressionTest06() =>
      TestSum(i => (double) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest07() =>
      TestSum(i => (double) i.IntValue * (double) i.IntValue);

    [Test]
    public void IntFieldExpressionTest08() =>
      TestSum(i => (decimal) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest09() =>
      TestSum(i => (decimal) i.IntValue * (decimal) i.IntValue);

    [Test]
    public void IntFieldExpressionTest10() => TestSum(i => i.IntValue * i.ByteValue);

    [Test]
    public void IntFieldExpressionTest11() => TestSum(i => i.IntValue * i.LongValue);

    [Test]
    public void IntFieldExpressionTest12() =>
      TestSumWithAccuracy(i => i.IntValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void IntFieldExpressionTest13() => TestSum(i => i.IntValue * i.DoubleValue1);

    [Test]
    public void IntFieldExpressionTest14() => TestSum(i => i.IntValue * i.DecimalValue);

    [Test]
    public void UIntFieldExpressionTest01() => TestSum(i => i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest02() =>
      TestSum(i => (long) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest03() =>
      TestSum(i => (long) i.UIntValue * (long) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest04() =>
      TestSumWithAccuracy(i => (float) i.UIntValue * i.UIntValue, FloatValueAccuracy);

    [Test]
    public void UIntFieldExpressionTest05() =>
      TestSumWithAccuracy(i => (float) i.UIntValue * (float) i.UIntValue, FloatValueAccuracy);

    [Test]
    public void UIntFieldExpressionTest06() =>
      TestSum(i => (double) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest07() =>
      TestSum(i => (double) i.UIntValue * (double) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest08() =>
      TestSum(i => (decimal) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest09() =>
      TestSum(i => (decimal) i.UIntValue * (decimal) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest10() => TestSum(i => i.UIntValue + i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest11() => TestSum(i => i.UIntValue * i.LongValue);

    [Test]
    public void UIntFieldExpressionTest12() =>
      TestSumWithAccuracy(i => i.UIntValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void UIntFieldExpressionTest13() => TestSum(i => i.UIntValue * i.DoubleValue1);

    [Test]
    public void UIntFieldExpressionTest14() => TestSum(i => i.UIntValue * i.DecimalValue);

    [Test]
    public void LongFieldExpressionTest01() => TestSum(i => i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest02() =>
      TestSumWithAccuracy(i => (float) i.LongValue * i.LongValue, FloatValueAccuracy);

    [Test]
    public void LongFieldExpressionTest03() =>
      TestSumWithAccuracy(i => (float) i.LongValue * (float) i.LongValue, FloatValueAccuracy);

    [Test]
    public void LongFieldExpressionTest04() =>
      TestSum(i => (double) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest05() =>
      TestSum(i => (double) i.LongValue * (double) i.LongValue);

    [Test]
    public void LongFieldExpressionTest06() =>
      TestSum(i => (decimal) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest07() =>
      TestSum(i => (decimal) i.LongValue * (decimal) i.LongValue);

    [Test]
    public void LongFieldExpressionTest08() =>
      TestSumWithAccuracy(i => i.LongValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void LongFieldExpressionTest09() => TestSum(i => i.LongValue * i.DoubleValue1);

    [Test]
    public void LongFieldExpressionTest10() => TestSum(i => i.LongValue * i.DecimalValue);

    [Test]
    public void ULongFieldExpressionTest02() =>
      TestSumWithAccuracy(i => (float) i.ULongValue * i.ULongValue, FloatValueAccuracy);

    [Test]
    public void ULongFieldExpressionTest03() =>
      TestSumWithAccuracy(i => (float) i.ULongValue * (float) i.ULongValue, FloatValueAccuracy);

    [Test]
    public void ULongFieldExpressionTest04() =>
      TestSum(i => (double) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest05() =>
      TestSum(i => (double) i.ULongValue * (double) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest06() =>
      TestSum(i => (decimal) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest07() =>
      TestSum(i => (decimal) i.ULongValue * (decimal) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest08() =>
      TestSumWithAccuracy(i => i.ULongValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void ULongFieldExpressionTest09() => TestSum(i => i.ULongValue * i.DoubleValue1);

    [Test]
    public void ULongFieldExpressionTest10() => TestSum(i => i.ULongValue * i.DecimalValue);

    [Test]
    public void FloatFieldExpressionTest01() =>
      TestSumWithAccuracy(i => i.FloatValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest02() =>
      TestSumWithAccuracy(i => (double) i.FloatValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest03() =>
      TestSumWithAccuracy(i => (double) i.FloatValue * (double) i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest04() =>
      TestSumWithAccuracy(i => (decimal) i.FloatValue * (decimal) i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest05() =>
      TestSumWithAccuracy(i => i.FloatValue * i.DoubleValue1, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest06() =>
      TestSumWithAccuracy(i => (decimal) i.FloatValue * i.DecimalValue, FloatValueAccuracy);

    [Test]
    public void DoubleFieldExpressionTest01() =>
      TestSumWithAccuracy(i => i.DoubleValue1 * i.DoubleValue1, DoubleValueAccuracy);

    [Test]
    public void DoubleFieldExpressionTest02() =>
      TestSumWithAccuracy(i => (decimal) i.DoubleValue1 * i.DecimalValue, DoubleValueAccuracy);

    [Test]
    public void DecimalFieldExpressionTest() =>
      TestSum(i => i.DecimalValue * i.DecimalValue);

    [Test]
    public void NullableByteFieldExpressionTest01() =>
      TestSum(i => i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest02() =>
      TestSum(i => (short?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest03() =>
      TestSum(i => (short?) i.NullableByteValue + (short?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest04() =>
      TestSum(i => (int?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest05() =>
      TestSum(i => (int?) i.NullableByteValue + (int?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest06() =>
      TestSum(i => (long?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest07() =>
      TestSum(i => (long?) i.NullableByteValue + (long?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest08() =>
      TestSumWithAccuracy(i => (float?) i.NullableByteValue + i.NullableByteValue, FloatValueAccuracy);

    [Test]
    public void NullableByteFieldExpressionTest09() =>
      TestSumWithAccuracy(i => (float?) i.NullableByteValue + (float?) i.NullableByteValue, FloatValueAccuracy);

    [Test]
    public void NullableByteFieldExpressionTest10() =>
      TestSum(i => (double?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest11() =>
      TestSum(i => (double) i.ByteValue + (double) i.ByteValue);

    [Test]
    public void NullableByteFieldExpressionTest12() =>
      TestSum(i => (decimal?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest13() =>
      TestSum(i => (decimal?) i.NullableByteValue + (decimal?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest14() =>
      TestSum(i => i.NullableByteValue + i.NullableIntValue);

    [Test]
    public void NullableByteFieldExpressionTest15() =>
      TestSum(i => i.NullableByteValue + i.NullableLongValue);

    [Test]
    public void NullableByteFieldExpressionTest16() =>
      TestSumWithAccuracy(i => i.NullableByteValue + i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableByteFieldExpressionTest17() =>
      TestSum(i => i.NullableByteValue + i.NullableDoubleValue1);

    [Test]
    public void NullableByteFieldExpressionTest18() =>
      TestSum(i => i.NullableByteValue + i.NullableDecimalValue);

    [Test]
    public void NullableSByteFieldExpressionTest01() =>
      TestSum(i => i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest02() =>
      TestSum(i => (short?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest03() =>
      TestSum(i => (short?) i.NullableSByteValue + (short?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest04() =>
      TestSum(i => (int?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest05() =>
      TestSum(i => (int?) i.NullableSByteValue + (int?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest06() =>
      TestSum(i => (long?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest07() =>
      TestSum(i => (long?) i.NullableSByteValue + (long?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest08() =>
      TestSumWithAccuracy(i => (float?) i.NullableSByteValue + i.NullableSByteValue, FloatValueAccuracy);

    [Test]
    public void NullableSByteFieldExpressionTest09() =>
      TestSumWithAccuracy(i => (float?) i.NullableSByteValue + (float?) i.NullableSByteValue, FloatValueAccuracy);

    [Test]
    public void NullableSByteFieldExpressionTest10() =>
      TestSum(i => (double?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest11() =>
      TestSum(i => (double?) i.NullableSByteValue + (double?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest12() =>
      TestSum(i => (decimal?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest13() =>
      TestSum(i => (decimal?) i.NullableSByteValue + (decimal?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest14() =>
      TestSum(i => i.NullableSByteValue + i.NullableByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest15() =>
      TestSum(i => i.NullableSByteValue + i.NullableIntValue);

    [Test]
    public void NullableSByteFieldExpressionTest16() =>
      TestSum(i => i.NullableSByteValue + i.NullableLongValue);

    [Test]
    public void NullableSByteFieldExpressionTest17() =>
      TestSumWithAccuracy(i => i.NullableSByteValue + i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableSByteFieldExpressionTest18() =>
      TestSum(i => i.NullableSByteValue + i.NullableDoubleValue1);

    [Test]
    public void NullableSByteFieldExpressionTest19() =>
      TestSum(i => i.NullableSByteValue + i.NullableDecimalValue);

    [Test]
    public void NullableShortFieldExpressionTest01() =>
      TestSum(i => i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest02() =>
      TestSum(i => (int?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest03() =>
      TestSum(i => (int?) i.NullableShortValue + (int?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest04() =>
      TestSum(i => (long?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest05() =>
      TestSum(i => (long?) i.NullableShortValue + (long?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest06() =>
      TestSumWithAccuracy(i => (float?) i.NullableShortValue + i.NullableShortValue, FloatValueAccuracy);

    [Test]
    public void NullableShortFieldExpressionTest07() =>
      TestSumWithAccuracy(i => (float?) i.NullableShortValue + (float?) i.NullableShortValue, FloatValueAccuracy);

    [Test]
    public void NullableShortFieldExpressionTest08() =>
      TestSum(i => (double?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest09() =>
      TestSum(i => (double?) i.NullableShortValue + (double?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest10() =>
      TestSum(i => (decimal?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest11() =>
      TestSum(i => (decimal?) i.NullableShortValue + (decimal?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest12() =>
      TestSum(i => i.NullableShortValue + i.NullableByteValue);

    [Test]
    public void NullableShortFieldExpressionTest13() =>
      TestSum(i => i.NullableShortValue + i.NullableIntValue);

    [Test]
    public void NullableShortFieldExpressionTest14() =>
      TestSum(i => i.NullableShortValue + i.NullableLongValue);

    [Test]
    public void NullableShortFieldExpressionTest15() =>
      TestSumWithAccuracy(i => i.NullableShortValue + i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableShortFieldExpressionTest16() =>
      TestSum(i => i.NullableShortValue + i.NullableDoubleValue1);

    [Test]
    public void NullableShortFieldExpressionTest17() =>
      TestSum(i => i.NullableShortValue + i.NullableDecimalValue);

    [Test]
    public void NullableUShortFieldExpressionTest01() =>
      TestSum(i => i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest02() =>
      TestSum(i => (int?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest03() =>
      TestSum(i => (int?) i.NullableUShortValue + (int?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest04() =>
      TestSum(i => (long?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest05() =>
      TestSum(i => (long?) i.NullableUShortValue + (long?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest06() =>
      TestSumWithAccuracy(i => (float?) i.NullableUShortValue + i.NullableUShortValue, FloatValueAccuracy);

    [Test]
    public void NullableUShortFieldExpressionTest07() =>
      TestSumWithAccuracy(i => (float?) i.NullableUShortValue + (float?) i.NullableUShortValue, FloatValueAccuracy);

    [Test]
    public void NullableUShortFieldExpressionTest08() =>
      TestSum(i => (double?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest09() =>
      TestSum(i => (double?) i.NullableUShortValue + (double?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest10() =>
      TestSum(i => (decimal?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest11() =>
      TestSum(i => (decimal?) i.NullableUShortValue + (decimal?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest12() =>
      TestSum(i => i.NullableUShortValue + i.NullableByteValue);

    [Test]
    public void NullableUShortFieldExpressionTest13() =>
      TestSum(i => i.NullableUShortValue + i.NullableIntValue);

    [Test]
    public void NullableUShortFieldExpressionTest14() =>
      TestSum(i => i.NullableUShortValue + i.NullableLongValue);

    [Test]
    public void NullableUShortFieldExpressionTest15() =>
      TestSumWithAccuracy(i => i.NullableUShortValue + i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableUShortFieldExpressionTest16() =>
      TestSum(i => i.NullableUShortValue + i.NullableDoubleValue1);

    [Test]
    public void NullableUShortFieldExpressionTest17() =>
      TestSum(i => i.NullableUShortValue + i.NullableDecimalValue);

    [Test]
    public void NullableIntFieldExpressionTest01() =>
      TestSum(i => i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest02() =>
      TestSum(i => (long?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest03() =>
      TestSum(i => (long?) i.NullableIntValue * (long?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest04() =>
      TestSumWithAccuracy(i => (float?) i.NullableIntValue * i.NullableIntValue, FloatValueAccuracy);

    [Test]
    public void NullableIntFieldExpressionTest05() =>
      TestSumWithAccuracy(i => (float?) i.NullableIntValue * (float?) i.NullableIntValue, FloatValueAccuracy);

    [Test]
    public void NullableIntFieldExpressionTest06() =>
      TestSum(i => (double?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest07() =>
      TestSum(i => (double?) i.NullableIntValue * (double?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest08() =>
      TestSum(i => (decimal?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest09() =>
      TestSum(i => (decimal?) i.NullableIntValue * (decimal?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest10() =>
      TestSum(i => i.NullableIntValue * i.NullableByteValue);

    [Test]
    public void NullableIntFieldExpressionTest11() =>
      TestSum(i => i.NullableIntValue * i.NullableLongValue);

    [Test]
    public void NullableIntFieldExpressionTest12() =>
      TestSumWithAccuracy(i => i.NullableIntValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableIntFieldExpressionTest13() =>
      TestSum(i => i.NullableIntValue * i.NullableDoubleValue1);

    [Test]
    public void NullableIntFieldExpressionTest14() =>
      TestSum(i => i.NullableIntValue * i.NullableDecimalValue);

    [Test]
    public void NullableUIntFieldExpressionTest01() =>
      TestSum(i => i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest02() =>
      TestSum(i => (long?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest03() =>
      TestSum(i => (long?) i.NullableUIntValue * (long?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest04() =>
      TestSumWithAccuracy(i => (float?) i.NullableUIntValue * i.NullableUIntValue, FloatValueAccuracy);

    [Test]
    public void NullableUIntFieldExpressionTest05() =>
      TestSum(i => (float?) i.NullableUIntValue * (float?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest06() =>
      TestSum(i => (double?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest07() =>
      TestSum(i => (double?) i.NullableUIntValue * (double?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest08() =>
      TestSum(i => (decimal?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest09() =>
      TestSum(i => (decimal?) i.NullableUIntValue * (decimal?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest10() =>
      TestSum(i => i.NullableUIntValue + i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest11() =>
      TestSum(i => i.NullableUIntValue * i.NullableLongValue);

    [Test]
    public void NullableUIntFieldExpressionTest12() =>
      TestSumWithAccuracy(i => i.NullableUIntValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableUIntFieldExpressionTest13() =>
      TestSum(i => i.NullableUIntValue * i.NullableDoubleValue1);

    [Test]
    public void NullableUIntFieldExpressionTest14() =>
      TestSum(i => i.NullableUIntValue * i.NullableDecimalValue);

    [Test]
    public void NullableLongFieldExpressionTest01() =>
      TestSum(i => i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest02() =>
      TestSumWithAccuracy(i => (float?) i.NullableLongValue * i.NullableLongValue, FloatValueAccuracy);

    [Test]
    public void NullableLongFieldExpressionTest03() =>
      TestSumWithAccuracy(i => (float?) i.NullableLongValue * (float?) i.NullableLongValue, FloatValueAccuracy);

    [Test]
    public void NullableLongFieldExpressionTest04() =>
      TestSum(i => (double?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest05() =>
      TestSum(i => (double?) i.NullableLongValue * (double?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest06() =>
      TestSum(i => (decimal?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest07() =>
      TestSum(i => (decimal?) i.NullableLongValue * (decimal?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest08() =>
      TestSumWithAccuracy(i => i.NullableLongValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableLongFieldExpressionTest09() =>
      TestSum(i => i.NullableLongValue * i.NullableDoubleValue1);

    [Test]
    public void NullableLongFieldExpressionTest10() =>
      TestSum(i => i.NullableLongValue * i.NullableDecimalValue);

    [Test]
    public void NullableULongFieldExpressionTest02() =>
      TestSumWithAccuracy(i => (float?) i.NullableULongValue * i.NullableULongValue, FloatValueAccuracy);

    [Test]
    public void NullableULongFieldExpressionTest03() =>
      TestSumWithAccuracy(i => (float?) i.NullableULongValue * (float?) i.NullableULongValue, FloatValueAccuracy);

    [Test]
    public void NullableULongFieldExpressionTest04() =>
      TestSum(i => (double?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest05() =>
      TestSum(i => (double?) i.NullableULongValue * (double?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest06() =>
      TestSum(i => (decimal?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest07() =>
      TestSum(i => (decimal?) i.NullableULongValue * (decimal?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest08() =>
      TestSumWithAccuracy(i => i.NullableULongValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableULongFieldExpressionTest09() =>
      TestSum(i => i.NullableULongValue * i.NullableDoubleValue1);

    [Test]
    public void NullableULongFieldExpressionTest10() =>
      TestSum(i => i.NullableULongValue * i.NullableDecimalValue);

    [Test]
    public void NullableFloatFieldExpressionTest01() =>
      TestSumWithAccuracy(i => i.NullableFloatValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest02() =>
      TestSumWithAccuracy(i => (double?) i.NullableFloatValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest03() =>
      TestSumWithAccuracy(i => (double?) i.NullableFloatValue * (double?) i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest04() =>
      TestSumWithAccuracy(i => (decimal?) i.NullableFloatValue * (decimal?) i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest05() =>
      TestSumWithAccuracy(i => i.NullableFloatValue * i.NullableDoubleValue1, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest06() =>
      TestSumWithAccuracy(i => (decimal?) i.NullableFloatValue * i.NullableDecimalValue, FloatValueAccuracy);

    [Test]
    public void NullableDoubleFieldExpressionTest01() =>
      TestSum(i => i.NullableDoubleValue1 * i.NullableDoubleValue1);

    [Test]
    public void NullableDoubleFieldExpressionTest02() =>
      TestSum(i => (decimal?) i.NullableDoubleValue1 * i.NullableDecimalValue);

    [Test]
    public void NullableDecimalFieldExpressionTest() =>
      TestSum(i => i.NullableDecimalValue * i.NullableDecimalValue);


    private void TestSum(Expression<Func<TestEntity, int>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }


    private void TestSum(Expression<Func<TestEntity, int?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSum(Expression<Func<TestEntity, long>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSum(Expression<Func<TestEntity, long?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSum(Expression<Func<TestEntity, float>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSum(Expression<Func<TestEntity, float?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSum(Expression<Func<TestEntity, double>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSum(Expression<Func<TestEntity, double?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSum(Expression<Func<TestEntity, decimal>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSum(Expression<Func<TestEntity, decimal?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSumWithAccuracy<TAccuracy>(Expression<Func<TestEntity, float>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSumWithAccuracy<TAccuracy>(Expression<Func<TestEntity, float?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSumWithAccuracy<TAccuracy>(Expression<Func<TestEntity, double>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSumWithAccuracy<TAccuracy>(Expression<Func<TestEntity, double?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSumWithAccuracy<TAccuracy>(Expression<Func<TestEntity, decimal>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Sum(t => t)");
    }

    private void TestSumWithAccuracy<TAccuracy>(Expression<Func<TestEntity, decimal?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Sum(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Sum(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Sum(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Sum()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Sum(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Sum(t => t)");
    }
  }
}