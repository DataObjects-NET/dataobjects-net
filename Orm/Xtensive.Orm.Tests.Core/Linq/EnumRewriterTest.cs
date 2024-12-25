// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Reflection;
using Xtensive.Orm.Linq.Expressions.Visitors;


namespace Xtensive.Orm.Tests.Core.Linq
{
  public class EnumRewriterTest
  {
    private enum ByteBasedEnum : byte
    {
      Value1 = 1, Value2, Value3
    }

    private enum SByteBasedEnum : sbyte
    {
      Value1 = 1, Value2, Value3
    }

    private enum ShortBasedEnum : short
    {
      Value1 = 1, Value2, Value3
    }

    private enum UShortBasedEnum : ushort
    {
      Value1 = 1, Value2, Value3
    }

    private enum IntBasedEnum : int
    {
      Value1 = 1, Value2, Value3
    }

    private enum UIntBasedEnum : uint
    {
      Value1 = 1, Value2, Value3
    }

    private enum LongBasedEnum : long
    {
      Value1 = 1, Value2,  Value3
    }

    private enum ULongBasedEnum : ulong
    {
      Value1 = 1, Value2, Value3
    }


    private Expression[] ConstExpressions;
    private Expression[] NonNullableExpressions;
    private Expression[] NullableExpressions;
    private Expression[] NullExpressions;

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      ConstExpressions = new[] {
        // non-enum constants
        Expression.Constant(1, typeof(int)),
        Expression.Constant(2, typeof(int?)),
        Expression.Constant(null, typeof(int?)),
      };

      NonNullableExpressions = new[] {
        Expression.Constant(ByteBasedEnum.Value1, typeof(ByteBasedEnum)),
        Expression.Constant(SByteBasedEnum.Value1, typeof(SByteBasedEnum)),
        Expression.Constant(ShortBasedEnum.Value1, typeof(ShortBasedEnum)),
        Expression.Constant(UShortBasedEnum.Value1, typeof(UShortBasedEnum)),
        Expression.Constant(IntBasedEnum.Value1,  typeof(IntBasedEnum)),
        Expression.Constant(UIntBasedEnum.Value1,  typeof(UIntBasedEnum)),
        Expression.Constant(LongBasedEnum.Value1, typeof(LongBasedEnum)),
        Expression.Constant(ULongBasedEnum.Value1, typeof(ULongBasedEnum)),
      };

      NullableExpressions = new[] {
        Expression.Constant(ByteBasedEnum.Value2, typeof(ByteBasedEnum?)),
        Expression.Constant(SByteBasedEnum.Value2, typeof(SByteBasedEnum?)),
        Expression.Constant(ShortBasedEnum.Value2, typeof(ShortBasedEnum?)),
        Expression.Constant(UShortBasedEnum.Value2, typeof(UShortBasedEnum?)),
        Expression.Constant(IntBasedEnum.Value2,  typeof(IntBasedEnum?)),
        Expression.Constant(UIntBasedEnum.Value2,  typeof(UIntBasedEnum?)),
        Expression.Constant(LongBasedEnum.Value2, typeof(LongBasedEnum?)),
        Expression.Constant(ULongBasedEnum.Value2, typeof(ULongBasedEnum?)),
      };

      NullExpressions = new[] {
        Expression.Constant(null, typeof(ByteBasedEnum?)),
        Expression.Constant(null, typeof(SByteBasedEnum?)),
        Expression.Constant(null, typeof(ShortBasedEnum?)),
        Expression.Constant(null, typeof(UShortBasedEnum?)),
        Expression.Constant(null, typeof(IntBasedEnum?)),
        Expression.Constant(null, typeof(UIntBasedEnum?)),
        Expression.Constant(null, typeof(LongBasedEnum?)),
        Expression.Constant(null, typeof(ULongBasedEnum?)),
      };
    }

    [Test]
    public void NonEnumValuesTest()
    {
      foreach (var exp in ConstExpressions) {
        var rewrited = EnumRewriter.Rewrite(exp);
        Assert.That(rewrited, Is.EqualTo(exp));
      }
    }

    [Test]
    public void NonNullableEnumsTest()
    {
      foreach (var exp in NonNullableExpressions) {
        var rewrited = EnumRewriter.Rewrite(exp);
        var expType = exp.Type;
        var enumType = expType.StripNullable();
        Assert.That(rewrited, Is.InstanceOf<UnaryExpression>());
        var convert = rewrited as UnaryExpression;
        Assert.That(convert.NodeType, Is.EqualTo(ExpressionType.Convert));
        Assert.That(convert.Type, Is.EqualTo(expType));
        var operand = convert.Operand;
        Assert.That(operand, Is.InstanceOf<ConstantExpression>());
        var constant = operand as ConstantExpression;
        Assert.That(constant.Type, Is.Not.EqualTo(enumType));
        Assert.That(constant.Type, Is.EqualTo(Enum.GetUnderlyingType(enumType)));
      }
    }

    [Test]
    public void NullableEnumsTest()
    {
      foreach (var exp in NullableExpressions) {
        var rewrited = EnumRewriter.Rewrite(exp);
        var expType = exp.Type;
        var enumType = expType.StripNullable();

        Assert.That(rewrited, Is.InstanceOf<UnaryExpression>());
        var convert = rewrited as UnaryExpression;
        Assert.That(convert.NodeType, Is.EqualTo(ExpressionType.Convert));
        Assert.That(convert.Type, Is.EqualTo(expType));
        var operand = convert.Operand;
        Assert.That(operand, Is.InstanceOf<ConstantExpression>());
        var constant = operand as ConstantExpression;
        Assert.That(constant.Type, Is.Not.EqualTo(enumType));
        Assert.That(constant.Type, Is.EqualTo(Enum.GetUnderlyingType(enumType)));
        Assert.That(constant.Value, Is.GreaterThan(1));

      }
    }

    [Test]
    public void NullsAsNullableEnumsTest()
    {
      foreach (var exp in NullExpressions) {

        var rewrited = EnumRewriter.Rewrite(exp);
        var expType = exp.Type;
        var enumType = expType.StripNullable();

        Assert.That(rewrited, Is.InstanceOf<UnaryExpression>());
        var convert = rewrited as UnaryExpression;
        Assert.That(convert.NodeType, Is.EqualTo(ExpressionType.Convert));
        Assert.That(convert.Type, Is.EqualTo(expType));
        var operand = convert.Operand;
        Assert.That(operand, Is.InstanceOf<ConstantExpression>());
        var constant = operand as ConstantExpression;
        Assert.That(constant.Type, Is.Not.EqualTo(enumType));
        Assert.That(constant.Type, Is.EqualTo(typeof(object)));
        Assert.That(constant.Value, Is.Null);
      }
    }
  }
}
