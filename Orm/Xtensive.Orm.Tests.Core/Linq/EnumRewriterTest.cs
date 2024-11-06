using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Text;
using Xtensive.Reflection;
using Xtensive.Orm.Linq.Expressions.Visitors;
using NUnit.Framework;
using System.Linq;

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


    private Expression[] Expressions;

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      Expressions = new[] {
        // non-enum constants
        Expression.Constant(1, typeof(int)),
        Expression.Constant(2, typeof(int?)),
        Expression.Constant(null, typeof(int?)),

        //short enums
        Expression.Constant(ShortBasedEnum.Value1, typeof(ShortBasedEnum)),
        Expression.Constant(ShortBasedEnum.Value1, typeof(ShortBasedEnum)),
        Expression.Constant(ShortBasedEnum.Value1, typeof(ShortBasedEnum)),
        Expression.Constant(ShortBasedEnum.Value1, typeof(ShortBasedEnum)),
        Expression.Constant(IntBasedEnum.Value1, typeof(IntBasedEnum)),
        Expression.Constant(LongBasedEnum.Value1, typeof(LongBasedEnum)),

        Expression.Constant(ShortBasedEnum.Value2, typeof(ShortBasedEnum?)),
        Expression.Constant(IntBasedEnum.Value2, typeof(IntBasedEnum?)),
        Expression.Constant(LongBasedEnum.Value2, typeof(LongBasedEnum?)),

        Expression.Constant(null, typeof(ShortBasedEnum?)),
        Expression.Constant(null, typeof(IntBasedEnum?)),
        Expression.Constant(null, typeof(LongBasedEnum?)),
      };
    }

    [Test]
    public void NonEnumValuesTest()
    {
      foreach (var exp in Expressions.Take(3)) {
        var rewrited = EnumRewriter.Rewrite(exp);
        Assert.That(rewrited, Is.EqualTo(exp));
      }
    }

    [Test]
    public void NonNullableEnumsTest()
    {
      foreach (var exp in Expressions.Skip(3).Take(3)) {
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
      foreach (var exp in Expressions.Skip(6).Take(3)) {
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
      foreach (var exp in Expressions.Skip(9).Take(3)) {

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

    //[Test]
    //public void ComplexTest()
    //{
    //  foreach (var exp in Expressions) {
    //    var rewrited = EnumRewriter.Rewrite(exp);
    //    var expType = exp.Type;
    //    if (exp is ConstantExpression testExp && expType.StripNullable().IsEnum) {
    //      var isNullable = expType.IsNullable();
    //      var enumType = expType.StripNullable();
    //      if (isNullable) {

    //      }
    //      else {
    //        Assert.That(rewrited, Is.InstanceOf<UnaryExpression>());
    //        var convert = rewrited as UnaryExpression;
    //        Assert.That(convert.NodeType, Is.EqualTo(ExpressionType.Convert));
    //        Assert.That(convert.Type, Is.EqualTo(expType));
    //        var operand = convert.Operand;
    //        Assert.That(operand, Is.InstanceOf<ConstantExpression>());
    //        var constant = operand as ConstantExpression;
    //        Assert.That(constant.Type, Is.Not.EqualTo(enumType));
    //        Assert.That(constant.Type, Is.EqualTo(Enum.GetUnderlyingType(enumType)));
    //      }
    //    }
    //    else {
    //      Assert.That(rewrited, Is.EqualTo(exp));
    //    }
    //  }
    //}
  }
}
