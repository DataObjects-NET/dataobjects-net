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
  public sealed class MinProcessingTest : AggregatesProblemTestBase
  {
    [Test]
    public void ByteFieldTest() => TestMin(i => i.ByteValue);

    [Test]
    public void SByteFieldTest() => TestMin(i => i.SByteValue);

    [Test]
    public void ShortFieldTest() => TestMin(i => i.ShortValue);

    [Test]
    public void UShortFieldTest() => TestMin(i => i.UShortValue);

    [Test]
    public void IntFieldTest() => TestMin(i => i.IntValue);

    [Test]
    public void UIntFieldTest() => TestMin(i => i.UIntValue);

    [Test]
    public void LongFieldTest() => TestMin(i => i.LongValue);

    [Test]
    public void ULongFieldTest() => TestMin(i => i.ULongValue);

    [Test]
    public void FloatFieldTest() => TestMin(i => i.FloatValue);

    [Test]
    public void DoubleFieldTest() => TestMin(i => i.DoubleValue1);

    [Test]
    public void DecimalFieldTest() => TestMin(i => i.DecimalValue);

    [Test]
    public void NullableByteFieldTest() => TestMin(i => i.NullableByteValue);

    [Test]
    public void NullableSByteFieldTest() => TestMin(i => i.NullableSByteValue);

    [Test]
    public void NullableShortFieldTest() => TestMin(i => i.NullableShortValue);

    [Test]
    public void NullableUShortFieldTest() => TestMin(i => i.NullableUShortValue);

    [Test]
    public void NullableIntFieldTest() => TestMin(i => i.NullableByteValue);

    [Test]
    public void NullableUIntFieldTest() => TestMin(i => i.NullableUIntValue);

    [Test]
    public void NullableLongFieldTest() => TestMin(i => i.NullableByteValue);

    [Test]
    public void NullableFloatFieldTest() => TestMin(i => i.NullableFloatValue);

    [Test]
    public void NullableDoubleFieldTest() => TestMin(i => i.NullableDoubleValue1);

    [Test]
    public void NullableDecimalFieldTest() => TestMin(i => i.NullableDecimalValue);

    [Test]
    public void ByteFieldExpressionTest01() => TestMin(i => i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest02() =>
      TestMin(i => (short) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest03() =>
      TestMin(i => (short) i.ByteValue + (short) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest04() =>
      TestMin(i => (int) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest05() =>
      TestMin(i => (int) i.ByteValue + (int) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest06() =>
      TestMin(i => (long) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest07() =>
      TestMin(i => (long) i.ByteValue + (long) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest08() =>
      TestMin(i => (float) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest09() =>
      TestMin(i => (float) i.ByteValue + (float) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest10() =>
      TestMin(i => (double) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest11() =>
      TestMin(i => (double) i.ByteValue + (double) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest12() =>
      TestMin(i => (decimal) i.ByteValue + i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest13() =>
      TestMin(i => (decimal) i.ByteValue + (decimal) i.ByteValue);

    [Test]
    public void ByteFieldExpressionTest14() => TestMin(i => i.ByteValue + i.IntValue);

    [Test]
    public void ByteFieldExpressionTest15() => TestMin(i => i.ByteValue + i.LongValue);

    [Test]
    public void ByteFieldExpressionTest16() => TestMin(i => i.ByteValue + i.FloatValue);

    [Test]
    public void ByteFieldExpressionTest17() => TestMin(i => i.ByteValue + i.DoubleValue1);

    [Test]
    public void ByteFieldExpressionTest18() => TestMin(i => i.ByteValue + i.DecimalValue);

    [Test]
    public void SByteFieldExpressionTest01() => TestMin(i => i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest02() =>
      TestMin(i => (short) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest03() =>
      TestMin(i => (short) i.SByteValue + (short) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest04() =>
      TestMin(i => (int) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest05() =>
      TestMin(i => (int) i.SByteValue + (int) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest06() =>
      TestMin(i => (long) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest07() =>
      TestMin(i => (long) i.SByteValue + (long) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest08() =>
      TestMin(i => (float) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest09() =>
      TestMin(i => (float) i.SByteValue + (float) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest10() =>
      TestMin(i => (double) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest11() =>
      TestMin(i => (double) i.SByteValue + (double) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest12() =>
      TestMin(i => (decimal) i.SByteValue + i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest13() =>
      TestMin(i => (decimal) i.SByteValue + (decimal) i.SByteValue);

    [Test]
    public void SByteFieldExpressionTest14() => TestMin(i => i.SByteValue + i.ByteValue);

    [Test]
    public void SByteFieldExpressionTest15() => TestMin(i => i.SByteValue + i.IntValue);

    [Test]
    public void SByteFieldExpressionTest16() => TestMin(i => i.SByteValue + i.LongValue);

    [Test]
    public void SByteFieldExpressionTest17() => TestMin(i => i.SByteValue + i.FloatValue);

    [Test]
    public void SByteFieldExpressionTest18() => TestMin(i => i.SByteValue + i.DoubleValue1);

    [Test]
    public void SByteFieldExpressionTest19() => TestMin(i => i.SByteValue + i.DecimalValue);

    [Test]
    public void ShortFieldExpressionTest01() => TestMin(i => i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest02() =>
      TestMin(i => (int) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest03() =>
      TestMin(i => (int) i.ShortValue + (int) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest04() =>
      TestMin(i => (long) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest05() =>
      TestMin(i => (long) i.ShortValue + (long) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest06() =>
      TestMin(i => (float) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest07() =>
      TestMin(i => (float) i.ShortValue + (float) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest08() =>
      TestMin(i => (double) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest09() =>
      TestMin(i => (double) i.ShortValue + (double) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest10() =>
      TestMin(i => (decimal) i.ShortValue + i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest11() =>
      TestMin(i => (decimal) i.ShortValue + (decimal) i.ShortValue);

    [Test]
    public void ShortFieldExpressionTest12() => TestMin(i => i.ShortValue + i.ByteValue);

    [Test]
    public void ShortFieldExpressionTest13() => TestMin(i => i.ShortValue + i.IntValue);

    [Test]
    public void ShortFieldExpressionTest14() => TestMin(i => i.ShortValue + i.LongValue);

    [Test]
    public void ShortFieldExpressionTest15() => TestMin(i => i.ShortValue + i.FloatValue);

    [Test]
    public void ShortFieldExpressionTest16() => TestMin(i => i.ShortValue + i.DoubleValue1);

    [Test]
    public void ShortFieldExpressionTest17() => TestMin(i => i.ShortValue + i.DecimalValue);

    [Test]
    public void UShortFieldExpressionTest01() => TestMin(i => i.ShortValue + i.ShortValue);

    [Test]
    public void UShortFieldExpressionTest02() =>
      TestMin(i => (int) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest03() =>
      TestMin(i => (int) i.UShortValue + (int) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest04() =>
      TestMin(i => (long) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest05() =>
      TestMin(i => (long) i.UShortValue + (long) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest06() =>
      TestMin(i => (float) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest07() =>
      TestMin(i => (float) i.UShortValue + (float) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest08() =>
      TestMin(i => (double) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest09() =>
      TestMin(i => (double) i.UShortValue + (double) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest10() =>
      TestMin(i => (decimal) i.UShortValue + i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest11() =>
      TestMin(i => (decimal) i.UShortValue + (decimal) i.UShortValue);

    [Test]
    public void UShortFieldExpressionTest12() => TestMin(i => i.UShortValue + i.ByteValue);

    [Test]
    public void UShortFieldExpressionTest13() => TestMin(i => i.UShortValue + i.IntValue);

    [Test]
    public void UShortFieldExpressionTest14() => TestMin(i => i.UShortValue + i.LongValue);

    [Test]
    public void UShortFieldExpressionTest15() => TestMin(i => i.UShortValue + i.FloatValue);

    [Test]
    public void UShortFieldExpressionTest16() => TestMin(i => i.UShortValue + i.DoubleValue1);

    [Test]
    public void UShortFieldExpressionTest17() => TestMin(i => i.UShortValue + i.DecimalValue);

    [Test]
    public void IntFieldExpressionTest01() => TestMin(i => i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest02() =>
      TestMin(i => (long) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest03() =>
      TestMin(i => (long) i.IntValue * (long) i.IntValue);

    [Test]
    public void IntFieldExpressionTest04() =>
      TestMin(i => (float) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest05() =>
      TestMin(i => (float) i.IntValue * (float) i.IntValue);

    [Test]
    public void IntFieldExpressionTest06() =>
      TestMin(i => (double) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest07() =>
      TestMin(i => (double) i.IntValue * (double) i.IntValue);

    [Test]
    public void IntFieldExpressionTest08() =>
      TestMin(i => (decimal) i.IntValue * i.IntValue);

    [Test]
    public void IntFieldExpressionTest09() =>
      TestMin(i => (decimal) i.IntValue * (decimal) i.IntValue);

    [Test]
    public void IntFieldExpressionTest10() => TestMin(i => i.IntValue * i.ByteValue);

    [Test]
    public void IntFieldExpressionTest11() => TestMin(i => i.IntValue * i.LongValue);

    [Test]
    public void IntFieldExpressionTest12() => TestMin(i => i.IntValue * i.FloatValue);

    [Test]
    public void IntFieldExpressionTest13() => TestMin(i => i.IntValue * i.DoubleValue1);

    [Test]
    public void IntFieldExpressionTest14() => TestMin(i => i.IntValue * i.DecimalValue);

    [Test]
    public void UIntFieldExpressionTest01() => TestMin(i => i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest02() =>
      TestMin(i => (long) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest03() =>
      TestMin(i => (long) i.UIntValue * (long) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest04() =>
      TestMin(i => (float) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest05() =>
      TestMin(i => (float) i.UIntValue * (float) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest06() =>
      TestMin(i => (double) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest07() =>
      TestMin(i => (double) i.UIntValue * (double) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest08() =>
      TestMin(i => (decimal) i.UIntValue * i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest09() =>
      TestMin(i => (decimal) i.UIntValue * (decimal) i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest10() => TestMin(i => i.UIntValue + i.UIntValue);

    [Test]
    public void UIntFieldExpressionTest11() => TestMin(i => i.UIntValue * i.LongValue);

    [Test]
    public void UIntFieldExpressionTest12() => TestMin(i => i.UIntValue * i.FloatValue);

    [Test]
    public void UIntFieldExpressionTest13() => TestMin(i => i.UIntValue * i.DoubleValue1);

    [Test]
    public void UIntFieldExpressionTest14() => TestMin(i => i.UIntValue * i.DecimalValue);

    [Test]
    public void LongFieldExpressionTest01() => TestMin(i => i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest02() =>
      TestMin(i => (float) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest03() =>
      TestMin(i => (float) i.LongValue * (float) i.LongValue);

    [Test]
    public void LongFieldExpressionTest04() =>
      TestMin(i => (double) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest05() =>
      TestMin(i => (double) i.LongValue * (double) i.LongValue);

    [Test]
    public void LongFieldExpressionTest06() =>
      TestMin(i => (decimal) i.LongValue * i.LongValue);

    [Test]
    public void LongFieldExpressionTest07() =>
      TestMin(i => (decimal) i.LongValue * (decimal) i.LongValue);

    [Test]
    public void LongFieldExpressionTest08() => TestMin(i => i.LongValue * i.FloatValue);

    [Test]
    public void LongFieldExpressionTest09() => TestMin(i => i.LongValue * i.DoubleValue1);

    [Test]
    public void LongFieldExpressionTest10() => TestMin(i => i.LongValue * i.DecimalValue);

    [Test]
    public void ULongFieldExpressionTest02() =>
      TestMin(i => (float) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest03() =>
      TestMin(i => (float) i.ULongValue * (float) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest04() =>
      TestMin(i => (double) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest05() =>
      TestMin(i => (double) i.ULongValue * (double) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest06() =>
      TestMin(i => (decimal) i.ULongValue * i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest07() =>
      TestMin(i => (decimal) i.ULongValue * (decimal) i.ULongValue);

    [Test]
    public void ULongFieldExpressionTest08() => TestMin(i => i.ULongValue * i.FloatValue);

    [Test]
    public void ULongFieldExpressionTest09() => TestMin(i => i.ULongValue * i.DoubleValue1);

    [Test]
    public void ULongFieldExpressionTest10() =>
      TestMin(i => i.ULongValue * i.DecimalValue);

    [Test]
    public void FloatFieldExpressionTest01() =>
      TestMinWithAccuracy(i => i.FloatValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest02() =>
      TestMinWithAccuracy(i => (double) i.FloatValue * i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest03() =>
      TestMinWithAccuracy(i => (double) i.FloatValue * (double) i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest04() =>
      TestMinWithAccuracy(i => (decimal) i.FloatValue * (decimal) i.FloatValue, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest05() =>
      TestMinWithAccuracy(i => i.FloatValue * i.DoubleValue1, FloatValueAccuracy);

    [Test]
    public void FloatFieldExpressionTest06() =>
      TestMinWithAccuracy(i => (decimal) i.FloatValue * i.DecimalValue, FloatValueAccuracy);

    [Test]
    public void DoubleFieldExpressionTest01() =>
      TestMinWithAccuracy(i => i.DoubleValue1 * i.DoubleValue1, DoubleValueAccuracy);

    [Test]
    public void DoubleFieldExpressionTest02() =>
      TestMinWithAccuracy(i => (decimal) i.DoubleValue1 * i.DecimalValue, DoubleValueAccuracy);

    [Test]
    public void DecimalFieldExpressionTest() => TestMin(i => i.DecimalValue * i.DecimalValue);

    [Test]
    public void NullableByteFieldExpressionTest01() =>
      TestMin(i => i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest02() =>
      TestMin(i => (short?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest03() =>
      TestMin(i => (short?) i.NullableByteValue + (short?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest04() =>
      TestMin(i => (int?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest05() =>
      TestMin(i => (int?) i.NullableByteValue + (int?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest06() =>
      TestMin(i => (long?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest07() =>
      TestMin(i => (long?) i.NullableByteValue + (long?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest08() =>
      TestMin(i => (float?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest09() =>
      TestMin(i => (float?) i.NullableByteValue + (float?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest10() =>
      TestMin(i => (double?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest11() =>
      TestMin(i => (double) i.ByteValue + (double) i.ByteValue);

    [Test]
    public void NullableByteFieldExpressionTest12() =>
      TestMin(i => (decimal?) i.NullableByteValue + i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest13() =>
      TestMin(i => (decimal?) i.NullableByteValue + (decimal?) i.NullableByteValue);

    [Test]
    public void NullableByteFieldExpressionTest14() =>
      TestMin(i => i.NullableByteValue + i.NullableIntValue);

    [Test]
    public void NullableByteFieldExpressionTest15() =>
      TestMin(i => i.NullableByteValue + i.NullableLongValue);

    [Test]
    public void NullableByteFieldExpressionTest16() =>
      TestMin(i => i.NullableByteValue + i.NullableFloatValue);

    [Test]
    public void NullableByteFieldExpressionTest17() =>
      TestMin(i => i.NullableByteValue + i.NullableDoubleValue1);

    [Test]
    public void NullableByteFieldExpressionTest18() =>
      TestMin(i => i.NullableByteValue + i.NullableDecimalValue);

    [Test]
    public void NullableSByteFieldExpressionTest01() =>
      TestMin(i => i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest02() =>
      TestMin(i => (short?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest03() =>
      TestMin(i => (short?) i.NullableSByteValue + (short?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest04() =>
      TestMin(i => (int?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest05() =>
      TestMin(i => (int?) i.NullableSByteValue + (int?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest06() =>
      TestMin(i => (long?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest07() =>
      TestMin(i => (long?) i.NullableSByteValue + (long?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest08() =>
      TestMin(i => (float?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest09() =>
      TestMin(i => (float?) i.NullableSByteValue + (float?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest10() =>
      TestMin(i => (double?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest11() =>
      TestMin(i => (double?) i.NullableSByteValue + (double?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest12() =>
      TestMin(i => (decimal?) i.NullableSByteValue + i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest13() =>
      TestMin(i => (decimal?) i.NullableSByteValue + (decimal?) i.NullableSByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest14() =>
      TestMin(i => i.NullableSByteValue + i.NullableByteValue);

    [Test]
    public void NullableSByteFieldExpressionTest15() =>
      TestMin(i => i.NullableSByteValue + i.NullableIntValue);

    [Test]
    public void NullableSByteFieldExpressionTest16() =>
      TestMin(i => i.NullableSByteValue + i.NullableLongValue);

    [Test]
    public void NullableSByteFieldExpressionTest17() =>
      TestMin(i => i.NullableSByteValue + i.NullableFloatValue);

    [Test]
    public void NullableSByteFieldExpressionTest18() =>
      TestMin(i => i.NullableSByteValue + i.NullableDoubleValue1);

    [Test]
    public void NullableSByteFieldExpressionTest19() =>
      TestMin(i => i.NullableSByteValue + i.NullableDecimalValue);

    [Test]
    public void NullableShortFieldExpressionTest01() =>
      TestMin(i => i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest02() =>
      TestMin(i => (int?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest03() =>
      TestMin(i => (int?) i.NullableShortValue + (int?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest04() =>
      TestMin(i => (long?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest05() =>
      TestMin(i => (long?) i.NullableShortValue + (long?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest06() =>
      TestMin(i => (float?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest07() =>
      TestMin(i => (float?) i.NullableShortValue + (float?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest08() =>
      TestMin(i => (double?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest09() =>
      TestMin(i => (double?) i.NullableShortValue + (double?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest10() =>
      TestMin(i => (decimal?) i.NullableShortValue + i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest11() =>
      TestMin(i => (decimal?) i.NullableShortValue + (decimal?) i.NullableShortValue);

    [Test]
    public void NullableShortFieldExpressionTest12() =>
      TestMin(i => i.NullableShortValue + i.NullableByteValue);

    [Test]
    public void NullableShortFieldExpressionTest13() =>
      TestMin(i => i.NullableShortValue + i.NullableIntValue);

    [Test]
    public void NullableShortFieldExpressionTest14() =>
      TestMin(i => i.NullableShortValue + i.NullableLongValue);

    [Test]
    public void NullableShortFieldExpressionTest15() =>
      TestMin(i => i.NullableShortValue + i.NullableFloatValue);

    [Test]
    public void NullableShortFieldExpressionTest16() =>
      TestMin(i => i.NullableShortValue + i.NullableDoubleValue1);

    [Test]
    public void NullableShortFieldExpressionTest17() =>
      TestMin(i => i.NullableShortValue + i.NullableDecimalValue);

    [Test]
    public void NullableUShortFieldExpressionTest01() =>
      TestMin(i => i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest02() =>
      TestMin(i => (int?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest03() =>
      TestMin(i => (int?) i.NullableUShortValue + (int?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest04() =>
      TestMin(i => (long?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest05() =>
      TestMin(i => (long?) i.NullableUShortValue + (long?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest06() =>
      TestMin(i => (float?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest07() =>
      TestMin(i => (float?) i.NullableUShortValue + (float?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest08() =>
      TestMin(i => (double?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest09() =>
      TestMin(i => (double?) i.NullableUShortValue + (double?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest10() =>
      TestMin(i => (decimal?) i.NullableUShortValue + i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest11() =>
      TestMin(i => (decimal?) i.NullableUShortValue + (decimal?) i.NullableUShortValue);

    [Test]
    public void NullableUShortFieldExpressionTest12() =>
      TestMin(i => i.NullableUShortValue + i.NullableByteValue);

    [Test]
    public void NullableUShortFieldExpressionTest13() =>
      TestMin(i => i.NullableUShortValue + i.NullableIntValue);

    [Test]
    public void NullableUShortFieldExpressionTest14() =>
      TestMin(i => i.NullableUShortValue + i.NullableLongValue);

    [Test]
    public void NullableUShortFieldExpressionTest15() =>
      TestMin(i => i.NullableUShortValue + i.NullableFloatValue);

    [Test]
    public void NullableUShortFieldExpressionTest16() =>
      TestMin(i => i.NullableUShortValue + i.NullableDoubleValue1);

    [Test]
    public void NullableUShortFieldExpressionTest17() =>
      TestMin(i => i.NullableUShortValue + i.NullableDecimalValue);

    [Test]
    public void NullableIntFieldExpressionTest01() =>
      TestMin(i => i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest02() =>
      TestMin(i => (long?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest03() =>
      TestMin(i => (long?) i.NullableIntValue * (long?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest04() =>
      TestMin(i => (float?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest05() =>
      TestMin(i => (float?) i.NullableIntValue * (float?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest06() =>
      TestMin(i => (double?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest07() =>
      TestMin(i => (double?) i.NullableIntValue * (double?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest08() =>
      TestMin(i => (decimal?) i.NullableIntValue * i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest09() =>
      TestMin(i => (decimal?) i.NullableIntValue * (decimal?) i.NullableIntValue);

    [Test]
    public void NullableIntFieldExpressionTest10() =>
      TestMin(i => i.NullableIntValue * i.NullableByteValue);

    [Test]
    public void NullableIntFieldExpressionTest11() =>
      TestMin(i => i.NullableIntValue * i.NullableLongValue);

    [Test]
    public void NullableIntFieldExpressionTest12() =>
      TestMin(i => i.NullableIntValue * i.NullableFloatValue);

    [Test]
    public void NullableIntFieldExpressionTest13() =>
      TestMin(i => i.NullableIntValue * i.NullableDoubleValue1);

    [Test]
    public void NullableIntFieldExpressionTest14() =>
      TestMin(i => i.NullableIntValue * i.NullableDecimalValue);

    [Test]
    public void NullableUIntFieldExpressionTest01() =>
      TestMin(i => i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest02() =>
      TestMin(i => (long?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest03() =>
      TestMin(i => (long?) i.NullableUIntValue * (long?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest04() =>
      TestMin(i => (float?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest05() =>
      TestMin(i => (float?) i.NullableUIntValue * (float?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest06() =>
      TestMin(i => (double?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest07() =>
      TestMin(i => (double?) i.NullableUIntValue * (double?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest08() =>
      TestMin(i => (decimal?) i.NullableUIntValue * i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest09() =>
      TestMin(i => (decimal?) i.NullableUIntValue * (decimal?) i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest10() =>
      TestMin(i => i.NullableUIntValue + i.NullableUIntValue);

    [Test]
    public void NullableUIntFieldExpressionTest11() =>
      TestMin(i => i.NullableUIntValue * i.NullableLongValue);

    [Test]
    public void NullableUIntFieldExpressionTest12() =>
      TestMin(i => i.NullableUIntValue * i.NullableFloatValue);

    [Test]
    public void NullableUIntFieldExpressionTest13() =>
      TestMin(i => i.NullableUIntValue * i.NullableDoubleValue1);

    [Test]
    public void NullableUIntFieldExpressionTest14() =>
      TestMin(i => i.NullableUIntValue * i.NullableDecimalValue);

    [Test]
    public void NullableLongFieldExpressionTest01() =>
      TestMin(i => i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest02() =>
      TestMin(i => (float?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest03() =>
      TestMin(i => (float?) i.NullableLongValue * (float?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest04() =>
      TestMin(i => (double?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest05() =>
      TestMin(i => (double?) i.NullableLongValue * (double?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest06() =>
      TestMin(i => (decimal?) i.NullableLongValue * i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest07() =>
      TestMin(i => (decimal?) i.NullableLongValue * (decimal?) i.NullableLongValue);

    [Test]
    public void NullableLongFieldExpressionTest08() =>
      TestMin(i => i.NullableLongValue * i.NullableFloatValue);

    [Test]
    public void NullableLongFieldExpressionTest09() =>
      TestMin(i => i.NullableLongValue * i.NullableDoubleValue1);

    [Test]
    public void NullableLongFieldExpressionTest10() =>
      TestMin(i => i.NullableLongValue * i.NullableDecimalValue);

    [Test]
    public void NullableULongFieldExpressionTest02() =>
      TestMin(i => (float?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest03() =>
      TestMin(i => (float?) i.NullableULongValue * (float?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest04() =>
      TestMin(i => (double?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest05() =>
      TestMin(i => (double?) i.NullableULongValue * (double?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest06() =>
      TestMin(i => (decimal?) i.NullableULongValue * i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest07() =>
      TestMin(i => (decimal?) i.NullableULongValue * (decimal?) i.NullableULongValue);

    [Test]
    public void NullableULongFieldExpressionTest08() =>
      TestMin(i => i.NullableULongValue * i.NullableFloatValue);

    [Test]
    public void NullableULongFieldExpressionTest09() =>
      TestMin(i => i.NullableULongValue * i.NullableDoubleValue1);

    [Test]
    public void NullableULongFieldExpressionTest10() =>
      TestMin(i => i.NullableULongValue * i.NullableDecimalValue);

    [Test]
    public void NullableFloatFieldExpressionTest01() =>
      TestMinWithAccuracy(i => i.NullableFloatValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest02() =>
      TestMinWithAccuracy(i => (double?) i.NullableFloatValue * i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest03() =>
      TestMinWithAccuracy(i => (double?) i.NullableFloatValue * (double?) i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest04() =>
      TestMinWithAccuracy(i => (decimal?) i.NullableFloatValue * (decimal?) i.NullableFloatValue, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest05() =>
      TestMinWithAccuracy(i => i.NullableFloatValue * i.NullableDoubleValue1, FloatValueAccuracy);

    [Test]
    public void NullableFloatFieldExpressionTest06() =>
      TestMinWithAccuracy(i => (decimal?) i.NullableFloatValue * i.NullableDecimalValue, FloatValueAccuracy);

    [Test]
    public void NullableDoubleFieldExpressionTest01() =>
      TestMin(i => i.NullableDoubleValue1 * i.NullableDoubleValue1);

    [Test]
    public void NullableDoubleFieldExpressionTest02() =>
      TestMin(i => (decimal?) i.NullableDoubleValue1 * i.NullableDecimalValue);

    [Test]
    public void NullableDecimalFieldExpressionTest() =>
      TestMin(i => i.NullableDecimalValue * i.NullableDecimalValue);

    private void TestMin<TResult>(Expression<Func<TestEntity, TResult>> selector)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Min(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Min(selector.Compile());
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Min(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min();
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Min()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min(t => t);
      Assert.That(queryableResult, Is.EqualTo(enumerableResult), "Failed on Select(selector).Min(t => t)");
    }

    private void TestMinWithAccuracy<TAccuracy>(Expression<Func<TestEntity, float>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Min(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Min(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Min(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Min()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Min(t => t)");
    }

    private void TestMinWithAccuracy<TAccuracy>(Expression<Func<TestEntity, float?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Min(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Min(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Min(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Min()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Min(t => t)");
    }

    private void TestMinWithAccuracy<TAccuracy>(Expression<Func<TestEntity, double>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Min(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Min(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Min(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Min()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Min(t => t)");
    }

    private void TestMinWithAccuracy<TAccuracy>(Expression<Func<TestEntity, double?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Min(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Min(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Min(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Min()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Min(t => t)");
    }

    private void TestMinWithAccuracy<TAccuracy>(Expression<Func<TestEntity, decimal>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Min(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Min(selector.Compile());
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Min(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min();
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Min()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min(t => t);
      Assert.That(Math.Abs(queryableResult - enumerableResult), Is.LessThan(accuracy), "Failed on Select(selector).Min(t => t)");
    }

    private void TestMinWithAccuracy<TAccuracy>(Expression<Func<TestEntity, decimal?>> selector, TAccuracy accuracy)
    {
      var queryableResult = GlobalSession.Query.All<TestEntity>().Min(selector);
      var enumerableResult = GlobalSession.Query.All<TestEntity>().AsEnumerable().Min(selector.Compile());
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Min(selector)");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min();
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Min()");

      queryableResult = GlobalSession.Query.All<TestEntity>().Select(selector).Min(t => t);
      Assert.That(Math.Abs(queryableResult.Value - enumerableResult.Value), Is.LessThan(accuracy), "Failed on Select(selector).Min(t => t)");
    }
  }
}