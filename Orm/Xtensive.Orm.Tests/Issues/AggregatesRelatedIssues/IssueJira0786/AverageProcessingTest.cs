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
  public sealed class AverageProcessingTest : AggregatesProblemTestBase
  {
    [Test]
    public void ByteFieldTest() => TestAverage(i => i.ByteValue);

    [Test]
    public void SByteFieldTest() => TestAverage(i => i.SByteValue);

    [Test]
    public void ShortFieldTest() => TestAverage(i => i.ShortValue);

    [Test]
    public void UShortFieldTest() => TestAverage(i => i.UShortValue);

    [Test]
    public void IntFieldTest() => TestAverage(i => i.IntValue);

    [Test]
    public void UIntFieldTest() => TestAverage(i => i.UIntValue);

    [Test]
    public void LongFieldTest() => TestAverage(i => i.LongValue);

    [Test]
    public void FloatFieldTest() => TestAverage(i => i.FloatValue);

    [Test]
    public void DoubleFieldTest() => TestAverage(i => i.DoubleValue1 + 1.0);

    [Test]
    public void DecimalFieldTest() => TestAverage(i => i.DecimalValue);

    [Test]
    public void NullableByteFieldTest() => TestAverage(i => i.NullableByteValue);

    [Test]
    public void NullableSByteFieldTest() => TestAverage(i => i.NullableSByteValue);

    [Test]
    public void NullableShortFieldTest() => TestAverage(i => i.NullableShortValue);

    [Test]
    public void NullableUShortFieldTest() => TestAverage(i => i.NullableUShortValue);

    [Test]
    public void NullableIntFieldTest() => TestAverage(i => i.NullableIntValue);

    [Test]
    public void NullableUIntFieldTest() => TestAverage(i => i.NullableUIntValue);

    [Test]
    public void NullableLongFieldTest() => TestAverage(i => i.NullableLongValue);

    [Test]
    public void NullableFloatFieldTest() => TestAverage(i => i.NullableFloatValue);

    [Test]
    public void NullableDoubleFieldTest() => TestAverage(i => i.NullableDoubleValue1);

    [Test]
    public void NullableDecimalFieldTest() => TestAverage(i => i.NullableDecimalValue);

    [Test]
    public void ByteFieldExpressionTest01() => TestAverage(i => i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest02() =>
      TestAverage(i => (short) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest03() =>
      TestAverage(i => (short) i.ByteValue + (short) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest04() =>
      TestAverage(i => (int) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest05() =>
      TestAverage(i => (int) i.ByteValue + (int) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest06() =>
      TestAverage(i => (long) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest07() =>
      TestAverage(i => (long) i.ByteValue + (long) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest08() =>
      TestAverage(i => (float) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest09() =>
      TestAverage(i => (float) i.ByteValue + (float) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest10() =>
      TestAverage(i => (double) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest11() =>
      TestAverage(i => (double) i.ByteValue + (double) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest12() =>
      TestAverageWithAccuracy(i => (decimal) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest13() =>
      TestAverage(i => (decimal) i.ByteValue + (decimal) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest14() => TestAverage(i => i.ByteValue + i.IntValue);

    [Test]
    public void ByteFieldExpressionTest15() => TestAverage(i => i.ByteValue + i.LongValue);

    [Test]
    public void ByteFieldExpressionTest16() => TestAverage(i => i.ByteValue + i.FloatValue);

    [Test]
    public void ByteFieldExpressionTest17() => TestAverage(i => i.ByteValue + i.DoubleValue1);

    [Test]
    public void ByteFieldExpressionTest18() => TestAverage(i => i.ByteValue + i.DecimalValue);

    [Test]
    public void SByteFieldExpressionTest01() => TestAverage(i => i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest02() =>
      TestAverage(i => (short) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest03() =>
      TestAverage(i => (short) i.SByteValue + (short) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest04() =>
      TestAverage(i => (int) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest05() =>
      TestAverage(i => (int) i.SByteValue + (int) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest06() =>
      TestAverage(i => (long) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest07() =>
      TestAverage(i => (long) i.SByteValue + (long) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest08() =>
      TestAverage(i => (float) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest09() =>
      TestAverage(i => (float) i.SByteValue + (float) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest10() =>
      TestAverage(i => (double) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest11() =>
      TestAverage(i => (double) i.SByteValue + (double) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest12() =>
      TestAverage(i => (decimal) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest13() =>
      TestAverage(i => (decimal) i.SByteValue + (decimal) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest14() => TestAverage(i => i.SByteValue + i.ByteValue);

    [Test]
    public void SByteFieldExpressionTest15() => TestAverage(i => i.SByteValue + i.IntValue);

    [Test]
    public void SByteFieldExpressionTest16() => TestAverage(i => i.SByteValue + i.LongValue);

    [Test]
    public void SByteFieldExpressionTest17() => TestAverage(i => i.SByteValue + i.FloatValue);

    [Test]
    public void SByteFieldExpressionTest18() => TestAverage(i => i.SByteValue + i.DoubleValue1);

    [Test]
    public void SByteFieldExpressionTest19() => TestAverage(i => i.SByteValue + i.DecimalValue);

    [Test]
    public void ShortFieldExpressionTest01() => TestAverage(i => i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest02() =>
      TestAverage(i => (int) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest03() =>
      TestAverage(i => (int) i.ShortValue + (int) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest04() =>
      TestAverage(i => (long) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest05() =>
      TestAverage(i => (long) i.ShortValue + (long) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest06() =>
      TestAverage(i => (float) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest07() =>
      TestAverage(i => (float) i.ShortValue + (float) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest08() =>
      TestAverage(i => (double) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest09() =>
      TestAverage(i => (double) i.ShortValue + (double) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest10() =>
      TestAverage(i => (decimal) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest11() =>
      TestAverage(i => (decimal) i.ShortValue + (decimal) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest12() =>
      TestAverage(i => i.ShortValue + i.ByteValue);

    [Test]
    public void ShortFieldExpressionTest13() =>
      TestAverage(i => i.ShortValue + i.IntValue);

    [Test]
    public void ShortFieldExpressionTest14() =>
      TestAverage(i => i.ShortValue + i.LongValue);

    [Test]
    public void ShortFieldExpressionTest15() =>
      TestAverage(i => i.ShortValue + i.FloatValue);

    [Test]
    public void ShortFieldExpressionTest16() =>
      TestAverage(i => i.ShortValue + i.DoubleValue1);

    [Test]
    public void ShortFieldExpressionTest17() =>
      TestAverage(i => i.ShortValue + i.DecimalValue);

    [Test]
    public void UShortFieldExpressionTest01() =>
      TestAverage(i => i.ShortValue + i.ShortValue);

    [Test]
    public void UShortFieldExpressionTest02() =>
      TestAverage(i => (int) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest03() =>
      TestAverage(i => (int) i.UShortValue + (int) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest04() =>
      TestAverage(i => (long) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest05() =>
      TestAverage(i => (long) i.UShortValue + (long) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest06() =>
      TestAverage(i => (float) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest07() =>
      TestAverage(i => (float) i.UShortValue + (float) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest08() =>
      TestAverage(i => (double) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest09() =>
      TestAverage(i => (double) i.UShortValue + (double) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest10() =>
      TestAverage(i => (decimal) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest11() =>
      TestAverage(i => (decimal) i.UShortValue + (decimal) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest12() =>
      TestAverage(i => i.UShortValue + i.ByteValue);

    [Test]
    public void UShortFieldExpressionTest13() =>
      TestAverage(i => i.UShortValue + i.IntValue);

    [Test]
    public void UShortFieldExpressionTest14() =>
      TestAverage(i => i.UShortValue + i.LongValue);

    [Test]
    public void UShortFieldExpressionTest15() =>
      TestAverage(i => i.UShortValue + i.FloatValue);

    [Test]
    public void UShortFieldExpressionTest16() =>
      TestAverage(i => i.UShortValue + i.DoubleValue1);

    [Test]
    public void UShortFieldExpressionTest17() =>
      TestAverage(i => i.UShortValue + i.DecimalValue);

    [Test]
    public void IntFieldExpressionTest01() =>
      TestAverage(i => i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest02() =>
      TestAverage(i => (long) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest03() =>
      TestAverage(i => (long) i.IntValue * (long) i.IntValue);

    [Test]
    public void IntFieldExpressionTest04() =>
      TestAverage(i => (float) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest05() =>
      TestAverage(i => (float) i.IntValue * (float) i.IntValue);

    [Test]
    public void IntFieldExpressionTest06() =>
      TestAverage(i => (double) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest07() =>
      TestAverage(i => (double) i.IntValue * (double) i.IntValue);

    [Test]
    public void IntFieldExpressionTest08() =>
      TestAverageWithAccuracy(i => (decimal) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest09() =>
      TestAverageWithAccuracy(i => (decimal) i.IntValue * (decimal) i.IntValue);

    [Test]
    public void IntFieldExpressionTest10() =>
      TestAverage(i => i.IntValue * i.ByteValue);

    [Test]
    public void IntFieldExpressionTest11() =>
      TestAverage(i => i.IntValue * i.LongValue);

    [Test]
    public void IntFieldExpressionTest12() =>
      TestAverage(i => i.IntValue * i.FloatValue);

    [Test]
    public void IntFieldExpressionTest13() =>
      TestAverage(i => i.IntValue * i.DoubleValue1);

    [Test]
    public void IntFieldExpressionTest14() =>
      TestAverageWithAccuracy(i => i.IntValue * i.DecimalValue);

    [Test]
    public void UIntFieldExpressionTest01() =>
      TestAverage(i => i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest02() =>
      TestAverage(i => (long) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest03() =>
      TestAverage(i => (long) i.UIntValue * (long) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest04() =>
      TestAverage(i => (float) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest05() =>
      TestAverage(i => (float) i.UIntValue * (float) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest06() =>
      TestAverage(i => (double) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest07() =>
      TestAverage(i => (double) i.UIntValue * (double) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest08() =>
      TestAverageWithAccuracy(i => (decimal) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest09() =>
      TestAverageWithAccuracy(i => (decimal) i.UIntValue * (decimal) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest10() =>
      TestAverage(i => i.UIntValue + i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest11() =>
      TestAverage(i => i.UIntValue * i.LongValue);

    [Test]
    public void UIntFieldExpressionTest12() =>
      TestAverage(i => i.UIntValue * i.FloatValue);

    [Test]
    public void UIntFieldExpressionTest13() =>
      TestAverage(i => i.UIntValue * i.DoubleValue1);

    [Test]
    public void UIntFieldExpressionTest14() =>
      TestAverageWithAccuracy(i => i.UIntValue * i.DecimalValue);

    [Test]
    public void LongFieldExpressionTest01() =>
      TestAverage(i => i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest02() =>
      TestAverage(i => (float) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest03() =>
      TestAverage(i => (float) i.LongValue * (float) i.LongValue);

    [Test]
    public void LongFieldExpressionTest04() =>
      TestAverage(i => (double) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest05() =>
      TestAverage(i => (double) i.LongValue * (double) i.LongValue);

    [Test]
    public void LongFieldExpressionTest06() =>
      TestAverageWithAccuracy(i => (decimal) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest07() =>
      TestAverageWithAccuracy(i => (decimal) i.LongValue * (decimal) i.LongValue);

    [Test]
    public void LongFieldExpressionTest08() =>
      TestAverage(i => i.LongValue * i.FloatValue);

    [Test]
    public void LongFieldExpressionTest09() =>
      TestAverage(i => i.LongValue * i.DoubleValue1);

    [Test]
    public void LongFieldExpressionTest10() =>
      TestAverageWithAccuracy(i => i.LongValue * i.DecimalValue);

    [Test]
    public void ULongFieldExpressionTest02() =>
      TestAverage(i => (float) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest03() =>
      TestAverage(i => (float) i.ULongValue * (float) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest04() =>
      TestAverage(i => (double) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest05() =>
      TestAverage(i => (double) i.ULongValue * (double) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest06() =>
      TestAverageWithAccuracy(i => (decimal) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest07() =>
      TestAverageWithAccuracy(i => (decimal) i.ULongValue * (decimal) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest08() =>
      TestAverage(i => i.ULongValue * i.FloatValue);

    [Test]
    public void ULongFieldExpressionTest09() =>
      TestAverage(i => i.ULongValue * i.DoubleValue1);

    [Test]
    public void ULongFieldExpressionTest10() =>
      TestAverageWithAccuracy(i => i.ULongValue * i.DecimalValue);

    [Test]
    public void FloatFieldExpressionTest01() =>
      TestAverageWithAccuracy(i => i.FloatValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest02() =>
      TestAverageWithAccuracy(i => (double) i.FloatValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest03() =>
      TestAverageWithAccuracy(i => (double) i.FloatValue * (double) i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest04() =>
      TestAverageWithAccuracy(i => (decimal) i.FloatValue * (decimal) i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest05() =>
      TestAverageWithAccuracy(i => i.FloatValue * i.DoubleValue1, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest06() =>
      TestAverageWithAccuracy(i => (decimal) i.FloatValue * i.DecimalValue, FloatValueAccuracy);

    [Test]
    public void DoubleFieldExpressionTest01() =>
      TestAverageWithAccuracy(i => i.DoubleValue1 * i.DoubleValue1, DoubleValueAccuracy);

    [Test]
    public void DoubleFieldExpressionTest02() =>
      TestAverageWithAccuracy(i => (decimal) i.DoubleValue1 * i.DecimalValue, DoubleValueAccuracy);

    [Test]
    public void DecimalFieldExpressionTest() =>
      TestAverageWithAccuracy(i => i.DecimalValue * i.DecimalValue, DecimalValueAccuracy);

    [Test]
    public void NullableByteFieldExpressionTest01() =>
      TestAverage(i => i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest02() =>
      TestAverage(i => (short?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest03() =>
      TestAverage(i => (short?) i.NullableByteValue + (short?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest04() =>
      TestAverage(i => (int?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest05() =>
      TestAverage(i => (int?) i.NullableByteValue + (int?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest06() =>
      TestAverage(i => (long?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest07() =>
      TestAverage(i => (long?) i.NullableByteValue + (long?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest08() =>
      TestAverage(i => (float?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest09() =>
      TestAverage(i => (float?) i.NullableByteValue + (float?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest10() =>
      TestAverage(i => (double?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest11() =>
      TestAverage(i => (double) i.ByteValue + (double) i.ByteValue);

    [Test]
    public void NullableByteFieldExpressionTest12() =>
      TestAverage(i => (decimal?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest13() =>
      TestAverage(i => (decimal?) i.NullableByteValue + (decimal?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest14() =>
      TestAverage(i => i.NullableByteValue + i.NullableIntValue);

    [Test]
    public void NullableByteFieldExpressionTest15() =>
      TestAverage(i => i.NullableByteValue + i.NullableLongValue);

    [Test]
    public void NullableByteFieldExpressionTest16() =>
      TestAverage(i => i.NullableByteValue + i.NullableFloatValue);

    [Test]
    public void NullableByteFieldExpressionTest17() =>
      TestAverage(i => i.NullableByteValue + i.NullableDoubleValue1);

    [Test]
    public void NullableByteFieldExpressionTest18() =>
      TestAverage(i => i.NullableByteValue + i.NullableDecimalValue);

    [Test]
    public void NullableSByteFieldExpressionTest01() =>
      TestAverage(i => i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest02() =>
      TestAverage(i => (short?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest03() =>
      TestAverage(i => (short?) i.NullableSByteValue + (short?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest04() =>
      TestAverage(i => (int?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest05() =>
      TestAverage(i => (int?) i.NullableSByteValue + (int?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest06() =>
      TestAverage(i => (long?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest07() =>
      TestAverage(i => (long?) i.NullableSByteValue + (long?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest08() =>
      TestAverage(i => (float?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest09() =>
      TestAverage(i => (float?) i.NullableSByteValue + (float?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest10() =>
      TestAverage(i => (double?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest11() =>
      TestAverage(i => (double?) i.NullableSByteValue + (double?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest12() =>
      TestAverage(i => (decimal?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest13() =>
      TestAverage(i => (decimal?) i.NullableSByteValue + (decimal?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest14() =>
      TestAverage(i => i.NullableSByteValue + i.NullableByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest15() =>
      TestAverage(i => i.NullableSByteValue + i.NullableIntValue);

    [Test]
    public void NullableSByteFieldExpressionTest16() =>
      TestAverage(i => i.NullableSByteValue + i.NullableLongValue);

    [Test]
    public void NullableSByteFieldExpressionTest17() =>
      TestAverage(i => i.NullableSByteValue + i.NullableFloatValue);

    [Test]
    public void NullableSByteFieldExpressionTest18() =>
      TestAverage(i => i.NullableSByteValue + i.NullableDoubleValue1);

    [Test]
    public void NullableSByteFieldExpressionTest19() =>
      TestAverage(i => i.NullableSByteValue + i.NullableDecimalValue);

    [Test]
    public void NullableShortFieldExpressionTest01() =>
      TestAverage(i => i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest02() =>
      TestAverage(i => (int?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest03() =>
      TestAverage(i => (int?) i.NullableShortValue + (int?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest04() =>
      TestAverage(i => (long?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest05() =>
      TestAverage(i => (long?) i.NullableShortValue + (long?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest06() =>
      TestAverage(i => (float?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest07() =>
      TestAverage(i => (float?) i.NullableShortValue + (float?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest08() =>
      TestAverage(i => (double?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest09() =>
      TestAverage(i => (double?) i.NullableShortValue + (double?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest10() =>
      TestAverage(i => (decimal?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest11() =>
      TestAverage(i => (decimal?) i.NullableShortValue + (decimal?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest12() =>
      TestAverage(i => i.NullableShortValue + i.NullableByteValue);

    [Test]
    public void NullableShortFieldExpressionTest13() =>
      TestAverage(i => i.NullableShortValue + i.NullableIntValue);

    [Test]
    public void NullableShortFieldExpressionTest14() =>
      TestAverage(i => i.NullableShortValue + i.NullableLongValue);

    [Test]
    public void NullableShortFieldExpressionTest15() =>
      TestAverage(i => i.NullableShortValue + i.NullableFloatValue);

    [Test]
    public void NullableShortFieldExpressionTest16() =>
      TestAverage(i => i.NullableShortValue + i.NullableDoubleValue1);

    [Test]
    public void NullableShortFieldExpressionTest17() =>
      TestAverage(i => i.NullableShortValue + i.NullableDecimalValue);

    [Test]
    public void NullableUShortFieldExpressionTest01() =>
      TestAverage(i => i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest02() =>
      TestAverage(i => (int?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest03() =>
      TestAverage(i => (int?) i.NullableUShortValue + (int?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest04() =>
      TestAverage(i => (long?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest05() =>
      TestAverage(i => (long?) i.NullableUShortValue + (long?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest06() =>
      TestAverage(i => (float?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest07() =>
      TestAverage(i => (float?) i.NullableUShortValue + (float?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest08() =>
      TestAverage(i => (double?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest09() =>
      TestAverage(i => (double?) i.NullableUShortValue + (double?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest10() =>
      TestAverage(i => (decimal?) i.NullableUShortValue + i.NullableUShortValue);


    [Test]
    public void NullableUShortFieldExpressionTest11() =>
      TestAverage(i => (decimal?) i.NullableUShortValue + (decimal?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest12() =>
      TestAverage(i => i.NullableUShortValue + i.NullableByteValue);

    [Test]
    public void NullableUShortFieldExpressionTest13() =>
      TestAverage(i => i.NullableUShortValue + i.NullableIntValue);

    [Test]
    public void NullableUShortFieldExpressionTest14() =>
      TestAverage(i => i.NullableUShortValue + i.NullableLongValue);

    [Test]
    public void NullableUShortFieldExpressionTest15() =>
      TestAverage(i => i.NullableUShortValue + i.NullableFloatValue);

    [Test]
    public void NullableUShortFieldExpressionTest16() =>
      TestAverage(i => i.NullableUShortValue + i.NullableDoubleValue1);

    [Test]
    public void NullableUShortFieldExpressionTest17() =>
      TestAverage(i => i.NullableUShortValue + i.NullableDecimalValue);

    [Test]
    public void NullableIntFieldExpressionTest01() =>
      TestAverage(i => i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest02() =>
      TestAverage(i => (long?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest03() =>
      TestAverage(i => (long?) i.NullableIntValue * (long?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest04() =>
      TestAverage(i => (float?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest05() =>
      TestAverage(i => (float?) i.NullableIntValue * (float?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest06() =>
      TestAverage(i => (double?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest07() =>
      TestAverage(i => (double?) i.NullableIntValue * (double?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest08() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest09() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableIntValue * (decimal?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest10() =>
      TestAverage(i => i.NullableIntValue * i.NullableByteValue);

    [Test]
    public void NullableIntFieldExpressionTest11() =>
      TestAverage(i => i.NullableIntValue * i.NullableLongValue);

    [Test]
    public void NullableIntFieldExpressionTest12() =>
      TestAverage(i => i.NullableIntValue * i.NullableFloatValue);

    [Test]
    public void NullableIntFieldExpressionTest13() =>
      TestAverage(i => i.NullableIntValue * i.NullableDoubleValue1);

    [Test]
    public void NullableIntFieldExpressionTest14() =>
      TestAverageWithAccuracy(i => i.NullableIntValue * i.NullableDecimalValue);

    [Test]
    public void NullableUIntFieldExpressionTest01() =>
      TestAverage(i => i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest02() =>
      TestAverage(i => (long?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest03() =>
      TestAverage(i => (long?) i.NullableUIntValue * (long?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest04() =>
      TestAverage(i => (float?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest05() =>
      TestAverage(i => (float?) i.NullableUIntValue * (float?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest06() =>
      TestAverage(i => (double?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest07() =>
      TestAverage(i => (double?) i.NullableUIntValue * (double?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest08() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest09() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableUIntValue * (decimal?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest10() =>
      TestAverage(i => i.NullableUIntValue + i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest11() =>
      TestAverage(i => i.NullableUIntValue * i.NullableLongValue);

    [Test]
    public void NullableUIntFieldExpressionTest12() =>
      TestAverage(i => i.NullableUIntValue * i.NullableFloatValue);

    [Test]
    public void NullableUIntFieldExpressionTest13() =>
      TestAverage(i => i.NullableUIntValue * i.NullableDoubleValue1);

    [Test]
    public void NullableUIntFieldExpressionTest14() =>
      TestAverageWithAccuracy(i => i.NullableUIntValue * i.NullableDecimalValue);

    [Test]
    public void NullableLongFieldExpressionTest01() =>
      TestAverage(i => i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest02() =>
      TestAverage(i => (float?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest03() =>
      TestAverage(i => (float?) i.NullableLongValue * (float?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest04() =>
      TestAverage(i => (double?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest05() =>
      TestAverage(i => (double?) i.NullableLongValue * (double?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest06() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest07() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableLongValue * (decimal?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest08() =>
      TestAverage(i => i.NullableLongValue * i.NullableFloatValue);

    [Test]
    public void NullableLongFieldExpressionTest09() =>
      TestAverage(i => i.NullableLongValue * i.NullableDoubleValue1);

    [Test]
    public void NullableLongFieldExpressionTest10() =>
      TestAverageWithAccuracy(i => i.NullableLongValue * i.NullableDecimalValue);

    [Test]
    public void NullableULongFieldExpressionTest02() =>
      TestAverage(i => (float?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest03() =>
      TestAverage(i => (float?) i.NullableULongValue * (float?) i.NullableULongValue);


    [Test]
    public void NullableULongFieldExpressionTest04() =>
      TestAverage(i => (double?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest05() =>
      TestAverage(i => (double?) i.NullableULongValue * (double?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest06() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest07() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableULongValue * (decimal?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest08() =>
      TestAverage(i => i.NullableULongValue * i.NullableFloatValue);

    [Test]
    public void NullableULongFieldExpressionTest09() =>
      TestAverage(i => i.NullableULongValue * i.NullableDoubleValue1);

    [Test]
    public void NullableULongFieldExpressionTest10() =>
      TestAverageWithAccuracy(i => i.NullableULongValue * i.NullableDecimalValue);

    [Test]
    public void NullableFloatFieldExpressionTest01() =>
      TestAverageWithAccuracy(i => i.NullableFloatValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest02() =>
      TestAverageWithAccuracy(i => (double?) i.NullableFloatValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest03() =>
      TestAverageWithAccuracy(i => (double?) i.NullableFloatValue * (double?) i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest04() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableFloatValue * (decimal?) i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest05() =>
      TestAverageWithAccuracy(i => i.NullableFloatValue * i.NullableDoubleValue1, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest06() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableFloatValue * i.NullableDecimalValue, FloatValueAccuracy);

    [Test]
    public void NullableDoubleFieldExpressionTest01() =>
      TestAverage(i => i.NullableDoubleValue1 * i.NullableDoubleValue1);

    [Test]
    public void NullableDoubleFieldExpressionTest02() =>
      TestAverageWithAccuracy(i => (decimal?) i.NullableDoubleValue1 * i.NullableDecimalValue, DoubleValueAccuracy);

    [Test]
    public void NullableDecimalFieldExpressionTest() =>
      TestAverageWithAccuracy(i => i.NullableDecimalValue * i.NullableDecimalValue);



    private void TestAverage(Expression<Func<TestEntity, int>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverage(Expression<Func<TestEntity, int?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverage(Expression<Func<TestEntity, long>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverage(Expression<Func<TestEntity, long?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverage(Expression<Func<TestEntity, float>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverage(Expression<Func<TestEntity, float?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverage(Expression<Func<TestEntity, double>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverage(Expression<Func<TestEntity, double?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverage(Expression<Func<TestEntity, decimal>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverage(Expression<Func<TestEntity, decimal?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverageWithAccuracy<TAccuracy>(Expression<Func<TestEntity, float>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverageWithAccuracy<TAccuracy>(Expression<Func<TestEntity, float?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverageWithAccuracy<TAccuracy>(Expression<Func<TestEntity, double>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverageWithAccuracy<TAccuracy>(Expression<Func<TestEntity, double?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverageWithAccuracy<TAccuracy>(Expression<Func<TestEntity, decimal>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverageWithAccuracy<TAccuracy>(Expression<Func<TestEntity, decimal?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverageWithAccuracy(Expression<Func<TestEntity, decimal>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(Math.Abs(queryableResult - decimal.Round(enumerableResult, 19)), Is.LessThan(DecimalValueAccuracy), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(Math.Abs(queryableResult - decimal.Round(enumerableResult, 19)), Is.LessThan(DecimalValueAccuracy), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(Math.Abs(queryableResult - decimal.Round(enumerableResult, 19)), Is.LessThan(DecimalValueAccuracy), "Failed on Select(selector).Average(t => t)");
    }

    private void TestAverageWithAccuracy(Expression<Func<TestEntity, decimal?>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Average(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Average(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - decimal.Round(enumerableResult.Value, 19)), Is.LessThan(DecimalValueAccuracy), "Failed on Average(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average();
      Assert.That(Math.Abs(queryableResult.Value - decimal.Round(enumerableResult.Value, 19)), Is.LessThan(DecimalValueAccuracy), "Failed on Select(selector).Average()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Average(t => t);
      Assert.That(Math.Abs(queryableResult.Value - decimal.Round(enumerableResult.Value, 19)), Is.LessThan(DecimalValueAccuracy), "Failed on Select(selector).Average(t => t)");
    }
  }
}