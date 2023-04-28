// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.09.26

using System;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using TypeMapping = Xtensive.Sql.TypeMapping;

namespace Xtensive.Orm.Providers
{
  partial class ExpressionProcessor
  {
    private SqlExpression TryTranslateCompareExpression(BinaryExpression expression)
    {
      var expressionLeft = expression.Left;
      var expressionRight = expression.Right;

      bool isGoodExpression =
        expressionLeft.NodeType==ExpressionType.Call && expressionRight.NodeType==ExpressionType.Constant ||
        expressionRight.NodeType==ExpressionType.Call && expressionLeft.NodeType==ExpressionType.Constant;

      if (!isGoodExpression)
        return null;

      MethodCallExpression callExpression;
      ConstantExpression constantExpression;
      bool swapped;

      if (expressionLeft.NodeType == ExpressionType.Call) {
        callExpression = (MethodCallExpression) expressionLeft;
        constantExpression = (ConstantExpression) expressionRight;
        swapped = false;
      }
      else {
        callExpression = (MethodCallExpression) expressionRight;
        constantExpression = (ConstantExpression) expressionLeft;
        swapped = true;
      }

      var method = (MethodInfo) callExpression.Method.GetInterfaceMember() ?? callExpression.Method;
      var methodType = method.DeclaringType;

      // There no methods in IComparable except CompareTo so checking only DeclatingType.
      bool isCompareTo = methodType==WellKnownInterfaces.Comparable || methodType.IsGenericType(WellKnownInterfaces.ComparableOfT);

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

      var expressionNodeType = expression.NodeType;

      if (constant > 0)
        switch (expressionNodeType) {
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
        switch (expressionNodeType) {
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

      switch (expressionNodeType) {
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

    private (SqlExpression right, SqlExpression left) BuildByteArraySyntaxComparison(SqlExpression left, SqlExpression right)
    {
      var newLeft = (SqlExpression) SqlDml.Literal(0);
      var newRight = OracleBlobCompare(left, right);

      return (newLeft, newRight);
    }

    private static SqlExpression OracleBlobCompare(SqlExpression left, SqlExpression right)
    {
      return SqlDml.FunctionCall("dbms_lob.compare", left, right);
    }

    private SqlExpression CompileMember(MemberInfo member, SqlExpression instance, params SqlExpression[] arguments)
    {
      var memberCompiler = memberCompilerProvider.GetCompiler(member)
        ?? throw new NotSupportedException(string.Format(Strings.ExMemberXIsNotSupported, member.GetFullName(true)));
      return memberCompiler.Invoke(instance, arguments);
    }

    private static bool IsCharToIntConvert(Expression e)
    {
      return
        e.NodeType == ExpressionType.Convert &&
        e.Type == WellKnownTypes.Int32 &&
        ((UnaryExpression) e).Operand.Type == WellKnownTypes.Char;
    }

    private static bool IsIntConstant(Expression expression) =>
      expression.NodeType == ExpressionType.Constant && expression.Type == WellKnownTypes.Int32;

    private static bool IsBooleanExpression(Expression expression) =>
      IsExpressionOf(expression, WellKnownTypes.Bool);

    private static bool IsDateTimeExpression(Expression expression) =>
      IsExpressionOf(expression, WellKnownTypes.DateTime);
#if NET6_0_OR_GREATER

    private static bool IsDateOnlyExpression(Expression expression) =>
      IsExpressionOf(expression, WellKnownTypes.DateOnly);

    private static bool IsTimeOnlyExpression(Expression expression) =>
      IsExpressionOf(expression, WellKnownTypes.TimeOnly);
#endif

    private static bool IsDateTimeOffsetExpression(Expression expression) =>
      IsExpressionOf(expression, WellKnownTypes.DateTimeOffset);

    private static bool IsExpressionOf(Expression expression, Type type)
    {
      return StripObjectCasts(expression).Type.StripNullable() == type;
    }

    private static bool IsComparisonExpression(Expression expression) =>
      expression.NodeType is 
        ExpressionType.Equal or
        ExpressionType.NotEqual or
        ExpressionType.GreaterThan or
        ExpressionType.LessThan or
        ExpressionType.GreaterThanOrEqual or
        ExpressionType.LessThanOrEqual;

    private static Expression StripObjectCasts(Expression expression)
    {
      while (expression.Type == WellKnownTypes.Object && expression.NodeType == ExpressionType.Convert) {
        expression = GetOperand(expression);
      }
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

    private static QueryParameterIdentity GetParameterIdentity(TypeMapping mapping,
      Expression<Func<ParameterContext, object>> accessor, QueryParameterBindingType bindingType)
    {
      var expression = accessor.Body;
    
      // Strip cast to object
      if (expression.NodeType == ExpressionType.Convert) {
        expression = ((UnaryExpression) expression).Operand;
      }

      // Check for closure member access
      if (expression.NodeType != ExpressionType.MemberAccess) {
        return null;
      }

      var memberAccess = (MemberExpression) expression;
      var operand = memberAccess.Expression;
      if (operand == null || !operand.Type.IsClosure()) {
        return null;
      }

      var fieldName = memberAccess.Member.Name;

      // Check for raw closure
      if (operand.NodeType == ExpressionType.Constant) {
        var closureObject = ((ConstantExpression) operand).Value;
        return new QueryParameterIdentity(mapping, closureObject, fieldName, bindingType);
      }

      // Check for parameterized closure
      if (operand.NodeType==ExpressionType.Call) {
        var callExpression = (MethodCallExpression) operand;
        if (string.Equals(callExpression.Method.Name, nameof(ParameterContext.GetValue), StringComparison.Ordinal)) {
          operand = callExpression.Object;
          if (operand!=null && WellKnownOrmTypes.ParameterContext == operand.Type) {
            operand = callExpression.Arguments[0];
          }
        }

        var isParameter = operand != null && operand.NodeType == ExpressionType.Constant;
        if (isParameter) {
          var parameterObject = ((ConstantExpression) operand).Value;
          return new QueryParameterIdentity(mapping, parameterObject, fieldName, bindingType);
        }
      }

      return null;
    }

    private QueryParameterBinding RegisterParameterBinding(TypeMapping mapping,
      Expression<Func<ParameterContext, object>> accessor, QueryParameterBindingType bindingType)
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
