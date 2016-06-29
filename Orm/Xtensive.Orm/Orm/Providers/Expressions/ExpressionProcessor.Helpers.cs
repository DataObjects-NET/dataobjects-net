// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.09.26

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  partial class ExpressionProcessor
  {
    private SqlExpression TryTranslateCompareExpression(BinaryExpression expression)
    {
      bool isGoodExpression =
        expression.Left.NodeType==ExpressionType.Call && expression.Right.NodeType==ExpressionType.Constant ||
        expression.Right.NodeType==ExpressionType.Call && expression.Left.NodeType==ExpressionType.Constant;

      if (!isGoodExpression)
        return null;

      MethodCallExpression callExpression;
      ConstantExpression constantExpression;
      bool swapped;

      if (expression.Left.NodeType==ExpressionType.Call) {
        callExpression = (MethodCallExpression) expression.Left;
        constantExpression = (ConstantExpression) expression.Right;
        swapped = false;
      }
      else {
        callExpression = (MethodCallExpression) expression.Right;
        constantExpression = (ConstantExpression) expression.Left;
        swapped = true;
      }

      var method = (MethodInfo) callExpression.Method.GetInterfaceMember() ?? callExpression.Method;
      var methodType = method.DeclaringType;

      // There no methods in IComparable except CompareTo so checking only DeclatingType.
      bool isCompareTo = methodType==typeof (IComparable)
        || methodType.IsGenericType && methodType.GetGenericTypeDefinition()==typeof (IComparable<>);

      bool isVbStringCompare = method.DeclaringType.FullName=="Microsoft.VisualBasic.CompilerServices.Operators" 
        && method.Name=="CompareString" 
        && method.GetParameters().Length==3 
        && method.IsStatic;

      bool isCompare = method.Name=="Compare" && method.GetParameters().Length==2 && method.IsStatic;

      if (!isCompareTo && !isCompare && !isVbStringCompare)
        return null;

      if (constantExpression.Value==null)
        return null;

      if (!(constantExpression.Value is int))
        return null;

      var constant = (int) constantExpression.Value;

      SqlExpression leftComparand = null;
      SqlExpression rightComparand = null;

      if (isCompareTo) {
        leftComparand = Visit(callExpression.Object);
        rightComparand = Visit(callExpression.Arguments[0]);
      }

      if (isCompare || isVbStringCompare) {
        leftComparand = Visit(callExpression.Arguments[0]);
        rightComparand = Visit(callExpression.Arguments[1]);
      }

      if (swapped) {
        var tmp = leftComparand;
        leftComparand = rightComparand;
        rightComparand = tmp;
      }

      if (constant > 0)
        switch (expression.NodeType) {
        case ExpressionType.Equal:
        case ExpressionType.GreaterThan:
        case ExpressionType.GreaterThanOrEqual:
          return SqlDml.GreaterThan(leftComparand, rightComparand);
        case ExpressionType.NotEqual:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.LessThan:
          return SqlDml.LessThanOrEquals(leftComparand, rightComparand);
        default:
          return null;
        }

      if (constant < 0)
        switch (expression.NodeType) {
        case ExpressionType.NotEqual:
        case ExpressionType.GreaterThan:
        case ExpressionType.GreaterThanOrEqual:
          return SqlDml.GreaterThanOrEquals(leftComparand, rightComparand);
        case ExpressionType.Equal:
        case ExpressionType.LessThanOrEqual:
        case ExpressionType.LessThan:
          return SqlDml.LessThan(leftComparand, rightComparand);
        default:
          return null;
        }

      switch (expression.NodeType) {
      case ExpressionType.GreaterThan:
        return SqlDml.GreaterThan(leftComparand, rightComparand);
      case ExpressionType.GreaterThanOrEqual:
        return SqlDml.GreaterThanOrEquals(leftComparand, rightComparand);
      case ExpressionType.Equal:
        return SqlDml.Equals(leftComparand, rightComparand);
      case ExpressionType.NotEqual:
        return SqlDml.NotEquals(leftComparand, rightComparand);
      case ExpressionType.LessThanOrEqual:
        return SqlDml.LessThanOrEquals(leftComparand, rightComparand);
      case ExpressionType.LessThan:
        return SqlDml.LessThan(leftComparand, rightComparand);
      default:
        return null;
      }
    }

    private SqlExpression TryTranslateBinaryExpressionSpecialCases(Expression expression, SqlExpression left, SqlExpression right)
    {
      SqlExpression result;
      switch (expression.NodeType) {
      case ExpressionType.Equal:
        return TryTranslateEqualitySpecialCases(left, right)
            ?? TryTranslateEqualitySpecialCases(right, left);
      case ExpressionType.NotEqual:
        return TryTranslateInequalitySpecialCases(left, right)
            ?? TryTranslateInequalitySpecialCases(right, left);
      default:
        return null;
      }
    }

    private SqlExpression TryTranslateEqualitySpecialCases(SqlExpression left, SqlExpression right)
    {
      if (right.NodeType==SqlNodeType.Null || emptyStringIsNull && IsEmptyStringLiteral(right))
        return SqlDml.IsNull(left);

      object id = null;
      if (right.NodeType==SqlNodeType.Placeholder)
        id = ((SqlPlaceholder) right).Id;
      else if (right.NodeType==SqlNodeType.Cast && ((SqlCast) (right)).Operand.NodeType==SqlNodeType.Placeholder)
        id = ((SqlPlaceholder) ((SqlCast) (right)).Operand).Id;
      if (id!=null) 
        return SqlDml.Variant(id, SqlDml.Equals(left, right), SqlDml.IsNull(left));
      
      return null;
    }

    private SqlExpression TryTranslateInequalitySpecialCases(SqlExpression left, SqlExpression right)
    {
      if (right.NodeType==SqlNodeType.Null || emptyStringIsNull && IsEmptyStringLiteral(right))
        return SqlDml.IsNotNull(left);
      
      object id = null;
      if (right.NodeType==SqlNodeType.Placeholder)
        id = ((SqlPlaceholder) right).Id;
      else if (right.NodeType==SqlNodeType.Cast && ((SqlCast) (right)).Operand.NodeType==SqlNodeType.Placeholder)
        id = ((SqlPlaceholder) ((SqlCast) (right)).Operand).Id;
      if (id!=null) 
        return SqlDml.Variant(id, SqlDml.NotEquals(left, right), SqlDml.IsNotNull(left));

      return null;
    }

    private SqlExpression CompileMember(MemberInfo member, SqlExpression instance, params SqlExpression[] arguments)
    {
      var memberCompiler = memberCompilerProvider.GetCompiler(member);
      if (memberCompiler==null)
        throw new NotSupportedException(string.Format(Strings.ExMemberXIsNotSupported, member.GetFullName(true)));
      return memberCompiler.Invoke(instance, arguments);
    }
    
    private static bool IsCharToIntConvert(Expression e)
    {
      return
        e.NodeType==ExpressionType.Convert &&
        e.Type==typeof (int) &&
        ((UnaryExpression) e).Operand.Type==typeof (char);
    }

    private static bool IsIntConstant(Expression expression)
    {
      return expression.NodeType==ExpressionType.Constant && expression.Type==typeof (int);
    }

    private static bool IsBooleanExpression(Expression expression)
    {
      return StripObjectCasts(expression).Type.StripNullable()==typeof (bool);
    }

    private static bool IsDateTimeExpression(Expression expression)
    {
      return StripObjectCasts(expression).Type.StripNullable()==typeof (DateTime);
    }

    private static bool IsDateTimeOffsetExpression(Expression expression)
    {
      return StripObjectCasts(expression).Type.StripNullable()==typeof (DateTimeOffset);
    }

    private static bool IsComparisonExpression(Expression expression)
    {
      return expression.NodeType.In(
        ExpressionType.Equal,
        ExpressionType.NotEqual,
        ExpressionType.GreaterThan,
        ExpressionType.LessThan,
        ExpressionType.GreaterThanOrEqual,
        ExpressionType.LessThanOrEqual);
    }

    private static Expression StripObjectCasts(Expression expression)
    {
      while (expression.Type==typeof (object) && expression.NodeType==ExpressionType.Convert)
        expression = GetOperand(expression);
      return expression;
    }

    private static SqlExpression ConvertIntConstantToSingleCharString(Expression expression)
    {
      var value = (int) ((ConstantExpression) expression).Value;
      return SqlDml.Literal(new string(new[] {(char) value}));
    }

    private static Expression GetOperand(Expression expression)
    {
      return ((UnaryExpression) expression).Operand;
    }

    private static bool IsEmptyStringLiteral(SqlExpression expression)
    {
      if (expression.NodeType!=SqlNodeType.Literal)
        return false;
      var value = ((SqlLiteral) expression).GetValue();
      return value!=null && value.Equals(string.Empty);
    }

    private bool IsEnumUnderlyingType(Type enumType, Type numericType)
    {
      return enumType.IsEnum && Enum.GetUnderlyingType(enumType)==numericType;
    }

    private QueryParameterIdentity GetParameterIdentity(TypeMapping mapping,
      Expression<Func<object>> accessor, QueryParameterBindingType bindingType)
    {
      var expression = accessor.Body;

      // Strip cast to object
      if (expression.NodeType==ExpressionType.Convert)
        expression = ((UnaryExpression) expression).Operand;

      // Check for closure member access
      if (expression.NodeType!=ExpressionType.MemberAccess)
        return null;

      var memberAccess = (MemberExpression) expression;
      var operand = memberAccess.Expression;
      if (operand==null || !operand.Type.IsClosure())
        return null;
      var fieldName = memberAccess.Member.Name;

      // Check for raw closure
      if (operand.NodeType==ExpressionType.Constant) {
        var closureObject = ((ConstantExpression) operand).Value;
        return new QueryParameterIdentity(mapping, closureObject, fieldName, bindingType);
      }

      // Check for parameterized closure
      if (operand.NodeType==ExpressionType.MemberAccess) {
        memberAccess = (MemberExpression) operand;
        operand = memberAccess.Expression;
        var isParameter = operand!=null
          && operand.NodeType==ExpressionType.Constant
          && typeof (Parameter).IsAssignableFrom(operand.Type)
          && memberAccess.Member.Name=="Value";
        if (isParameter) {
          var parameterObject = ((ConstantExpression) operand).Value;
          return new QueryParameterIdentity(mapping, parameterObject, fieldName, bindingType);
        }
      }

      return null;
    }

    private QueryParameterBinding RegisterParameterBinding(TypeMapping mapping,
      Expression<Func<object>> accessor, QueryParameterBindingType bindingType)
    {
      QueryParameterBinding result;
      var identity = GetParameterIdentity(mapping, accessor, bindingType);

      if (identity==null) {
        result = new QueryParameterBinding(mapping, accessor.CachingCompile(), bindingType);
        otherBindings.Add(result);
        return result;
      }

      if (bindingsWithIdentity.TryGetValue(identity, out result))
        return result;

      result = new QueryParameterBinding(mapping, accessor.CachingCompile(), bindingType);
      bindingsWithIdentity.Add(identity, result);
      return result;
    }
  }
}