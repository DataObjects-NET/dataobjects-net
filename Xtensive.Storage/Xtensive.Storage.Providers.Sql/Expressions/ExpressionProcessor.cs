// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Expressions;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal class ExpressionProcessor : ExpressionVisitor<SqlExpression>
  {
    private readonly IMemberCompilerProvider<SqlExpression> mappingsProvider;
    private readonly DomainModel model;
    private readonly SqlSelect[] selects;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private readonly LambdaExpression lambda;
    private readonly HashSet<SqlFetchParameterBinding> bindings;
    private readonly Dictionary<ParameterExpression, SqlSelect> parameterMapping;
    private readonly SqlValueTypeMapper valueTypeMapper;
    private readonly ICompiler compiler;
    private bool executed;

    public HashSet<SqlFetchParameterBinding> Bindings { get { return bindings; } }
    public DomainModel Model { get { return model; } }

    
    public SqlExpression Translate()
    {
      if (executed)
        throw new InvalidOperationException();
      executed = true;
      return Visit(lambda);
    }

    protected override SqlExpression Visit(Expression e)
    {
      return Visit(e, false);
    }

    private SqlExpression Visit(Expression e, bool smartNull)
    {
      if (e == null)
        return null;
      if (evaluator.CanBeEvaluated(e)) {
        if (parameterExtractor.IsParameter(e))
          return VisitParameterAccess(e, smartNull);
        return VisitConstant(evaluator.Evaluate(e));
      }
      return base.Visit(e);
    }

    private SqlExpression VisitParameterAccess(Expression e, bool smartNull)
    {
      var type = e.Type;
      // In rare cases (when calculated column is just parameter access) we need to strip cast to object.
      if (e.NodeType == ExpressionType.Convert && e.Type == typeof(object))
        type = ((UnaryExpression) e).Operand.Type;
      if (type.IsNullable())
        type = type.GetGenericArguments()[0];
      var typeMapping = valueTypeMapper.GetTypeMapping(type);
      var expression = parameterExtractor.ExtractParameter<object>(e);
      var binding = new SqlFetchParameterBinding(expression.CachingCompile(), typeMapping, smartNull);
      bindings.Add(binding);
      return binding.ParameterReference;
    }

    protected override SqlExpression VisitUnary(UnaryExpression expression)
    {
      var operand = Visit(expression.Operand);

      if (expression.Method != null)
        return CompileMember(expression.Method, null, operand);

      switch (expression.NodeType) {
        case ExpressionType.ArrayLength:
          if (expression.Operand.Type != typeof(byte[]))
            throw new NotSupportedException(string.Format(Strings.ExTypeXIsNotSupported, expression.Operand.Type));
          return SqlFactory.Cast(SqlFactory.Length(operand), SqlDataType.Int32);
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
          return SqlFactory.Negate(operand);
        case ExpressionType.UnaryPlus:
          return operand;
        case ExpressionType.Not:
          if ((expression.Operand.Type!=typeof (bool)) && (expression.Operand.Type!=typeof (bool?)))
            return SqlFactory.BitNot(operand);
          return SqlFactory.Not(operand);
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
          if (expression.Operand.Type == expression.Type)
            return operand;
          var mapping = valueTypeMapper.TryGetTypeMapping(expression.Type);
          if (mapping == null)
            return operand;
          return SqlFactory.Cast(operand, mapping.DataTypeInfo.SqlType);
      }
      return operand;
    }

    protected override SqlExpression VisitBinary(BinaryExpression expression)
    {
      SqlExpression result = TryTranslateCompareExpression(expression);
      if (result != null)
        return result;

      SqlExpression left;
      SqlExpression right;

      bool smartNull = expression.NodeType==ExpressionType.Equal || expression.NodeType==ExpressionType.NotEqual;

      // chars are compared as integers, but we store them as strings and should compare them like strings.
      if (IsCharToIntConvert(expression.Left) && IsCharToIntConvert(expression.Right)) {
        left = Visit(((UnaryExpression) expression.Left).Operand, smartNull);
        right = Visit(((UnaryExpression)expression.Right).Operand, smartNull);
      }
      else {
        left = Visit(expression.Left, smartNull);
        right = Visit(expression.Right, smartNull);
      }
      
      // handle special cases
      if (expression.NodeType == ExpressionType.Equal) {
        result = TryTranslateEqualitySpecialCases(left, right);
        if (result != null)
          return result;
        result = TryTranslateEqualitySpecialCases(right, left);
        if (result != null)
          return result;
      }

      if (expression.NodeType == ExpressionType.NotEqual) {
        result = TryTranslateInequalitySpecialCases(left, right);
        if (result != null)
          return result;
        result = TryTranslateInequalitySpecialCases(right, left);
        if (result != null)
          return result;
      }

      if (expression.Method != null)
        return CompileMember(expression.Method, null, left, right);

      switch (expression.NodeType) {
        case ExpressionType.Add:
        case ExpressionType.AddChecked:
          return SqlFactory.Add(left, right);
        case ExpressionType.And:
          if ((expression.Left.Type!=typeof (bool)) && (expression.Left.Type!=typeof (bool?)))
            return SqlFactory.BitAnd(left, right);
          return SqlFactory.And(left, right);
        case ExpressionType.AndAlso:
          return SqlFactory.And(left, right);
        case ExpressionType.Coalesce:
          return SqlFactory.Coalesce(left, right);
        case ExpressionType.Divide:
          return SqlFactory.Divide(left, right);
        case ExpressionType.Equal:
          return SqlFactory.Equals(left, right);
        case ExpressionType.ExclusiveOr:
          return SqlFactory.BitXor(left, right);
        case ExpressionType.GreaterThan:
          return SqlFactory.GreaterThan(left, right);
        case ExpressionType.GreaterThanOrEqual:
          return SqlFactory.GreaterThanOrEquals(left, right);
        case ExpressionType.LessThan:
          return SqlFactory.LessThan(left, right);
        case ExpressionType.LessThanOrEqual:
          return SqlFactory.LessThanOrEquals(left, right);
        case ExpressionType.Modulo:
          return SqlFactory.Modulo(left, right);
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
          return SqlFactory.Multiply(left, right);
        case ExpressionType.NotEqual:
          return SqlFactory.NotEquals(left, right);
        case ExpressionType.Or:
          if ((expression.Left.Type!=typeof (bool)) && (expression.Left.Type!=typeof (bool?)))
            return SqlFactory.BitOr(left, right);
          return SqlFactory.Or(left, right);
        case ExpressionType.OrElse:
          return SqlFactory.Or(left, right);
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          return SqlFactory.Subtract(left, right);
        default:
          throw new ArgumentOutOfRangeException("expression");
      }
    }

    protected override SqlExpression VisitTypeIs(TypeBinaryExpression tb)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitConditional(ConditionalExpression expression)
    {
      var check = Visit(expression.Test);
      var ifTrue = Visit(expression.IfTrue);
      var ifFalse = Visit(expression.IfFalse);
      var c = SqlFactory.Case();
      c[check] = ifTrue;
      c.Else = ifFalse;
      return c;
    }

    protected override SqlExpression VisitConstant(ConstantExpression expression)
    {
      var constant = expression.Value!=null
        ? SqlFactory.Literal(expression.Value, expression.Type)
        : SqlFactory.Null;
      return constant;
    }

    protected override SqlExpression VisitParameter(ParameterExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitMemberAccess(MemberExpression m)
    {
      return CompileMember(m.Member, Visit(m.Expression));
    }

    protected override SqlExpression VisitMethodCall(MethodCallExpression mc)
    {
      var tupleAccess = mc.AsTupleAccess();
      if (tupleAccess!=null) {
        int columnIndex = tupleAccess.GetTupleAccessArgument();
        var parameter = tupleAccess.GetApplyParameter();
        if (parameter == null) {
          var sqlSelect = parameterMapping[(ParameterExpression)tupleAccess.Object];
          return sqlSelect[columnIndex];
        }
        ExecutableProvider provider;
        if (compiler.CompiledSources.TryGetValue(parameter, out provider)) {
          if (!compiler.IsCompatible(provider)) {
            provider = compiler.ToCompatible(provider);
            compiler.CompiledSources.ReplaceBound(parameter, provider);
          }
          var sqlProvider = (SqlProvider)provider;
          return sqlProvider.PermanentReference[columnIndex];
        }
      }

      var arguments = mc.Arguments.Select(a => Visit(a)).ToArray();
      var mi = mc.Method;

      if (mc.Object!=null && mc.Object.Type!=mi.ReflectedType)
        mi = mc.Object.Type.GetMethod(mi.Name, mi.GetParameterTypes());

      return CompileMember(mi, Visit(mc.Object), arguments);
    }

    protected override SqlExpression VisitLambda(LambdaExpression l)
    {
      for (int i = 0; i < l.Parameters.Count; i++) {
        var p = l.Parameters[i];
        var select = selects[i];
        parameterMapping[p] = select;
      }
      return Visit(l.Body);
    }

    protected override SqlExpression VisitNew(NewExpression n)
    {
      return CompileMember(n.Constructor, null, n.Arguments.Select(a => Visit(a)).ToArray());
    }

    protected override SqlExpression VisitNewArray(NewArrayExpression expression)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitInvocation(InvocationExpression i)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitMemberInit(MemberInitExpression mi)
    {
      throw new NotSupportedException();
    }

    protected override SqlExpression VisitListInit(ListInitExpression li)
    {
      throw new NotSupportedException();
    }

    #region Private methods

    private SqlExpression TryTranslateCompareExpression(BinaryExpression expression)
    {
      bool isGoodExpression =
        expression.Left.NodeType==ExpressionType.Call
          && expression.Right.NodeType==ExpressionType.Constant ||
            expression.Right.NodeType==ExpressionType.Call
              && expression.Left.NodeType==ExpressionType.Constant;

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

      bool isCompare = method.Name=="Compare" && method.GetParameters().Length==2 && method.IsStatic;

      if (!isCompareTo && !isCompare)
        return null;

      if (constantExpression.Value==null)
        return null;

      if (!(constantExpression.Value is int))
        return null;

      int constant = (int) constantExpression.Value;

      SqlExpression leftComparand = null;
      SqlExpression rightComparand = null;

      if (isCompareTo) {
        leftComparand = Visit(callExpression.Object);
        rightComparand = Visit(callExpression.Arguments[0]);
      }

      if (isCompare) {
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
            return SqlFactory.GreaterThan(leftComparand, rightComparand);
          case ExpressionType.NotEqual:
          case ExpressionType.LessThanOrEqual:
          case ExpressionType.LessThan:
            return SqlFactory.LessThanOrEquals(leftComparand, rightComparand);
          default:
            return null;
        }

      if (constant < 0)
        switch (expression.NodeType) {
          case ExpressionType.NotEqual:
          case ExpressionType.GreaterThan:
          case ExpressionType.GreaterThanOrEqual:
            return SqlFactory.GreaterThanOrEquals(leftComparand, rightComparand);
          case ExpressionType.Equal:
          case ExpressionType.LessThanOrEqual:
          case ExpressionType.LessThan:
            return SqlFactory.LessThan(leftComparand, rightComparand);
          default:
            return null;
        }

      switch (expression.NodeType) {
        case ExpressionType.GreaterThan:
          return SqlFactory.GreaterThan(leftComparand, rightComparand);
        case ExpressionType.GreaterThanOrEqual:
          return SqlFactory.GreaterThanOrEquals(leftComparand, rightComparand);
        case ExpressionType.Equal:
          return SqlFactory.Equals(leftComparand, rightComparand);
        case ExpressionType.NotEqual:
          return SqlFactory.NotEquals(leftComparand, rightComparand);
        case ExpressionType.LessThanOrEqual:
          return SqlFactory.LessThanOrEquals(leftComparand, rightComparand);
        case ExpressionType.LessThan:
          return SqlFactory.LessThan(leftComparand, rightComparand);
        default:
          return null;
      }
    }

    private static SqlExpression TryTranslateEqualitySpecialCases(SqlExpression left, SqlExpression right)
    {
      if (right.NodeType == SqlNodeType.Null)
        return SqlFactory.IsNull(left);
      if (right.NodeType == SqlNodeType.Parameter)
        return SqlFactory.Variant(SqlFactory.Equals(left, right), SqlFactory.IsNull(left), ((SqlParameterRef) right).Parameter);
      return null;
    }

    private static SqlExpression TryTranslateInequalitySpecialCases(SqlExpression left, SqlExpression right)
    {
      if (right.NodeType == SqlNodeType.Null)
        return SqlFactory.IsNotNull(left);
      if (right.NodeType == SqlNodeType.Parameter)
        return SqlFactory.Variant(SqlFactory.NotEquals(left, right), SqlFactory.IsNotNull(left), ((SqlParameterRef)right).Parameter);
      return null;
    }

    private SqlExpression CompileMember(MemberInfo member, SqlExpression instance, params SqlExpression[] arguments)
    {
      var memberCompiler = mappingsProvider.GetCompiler(member);
      if (memberCompiler == null)
        throw new NotSupportedException(string.Format(Strings.ExMemberXIsNotSupported, member.GetFullName(true)));
      return memberCompiler.Invoke(instance, arguments);
    }

    private static bool IsCharToIntConvert(Expression e)
    {
      return e.NodeType==ExpressionType.Convert
          && e.Type==typeof (int)
          && ((UnaryExpression) e).Operand.Type==typeof (char);
    }

    #endregion

    // Constructor

    public ExpressionProcessor(ICompiler compiler, HandlerAccessor handlers, LambdaExpression le, params SqlSelect[] selects)
    {
      this.compiler = compiler;
      if (selects==null)
        throw new ArgumentNullException("selects");
      mappingsProvider = handlers.DomainHandler.GetMemberCompilerProvider<SqlExpression>();
      valueTypeMapper = ((DomainHandler) handlers.DomainHandler).ValueTypeMapper;
      if (le.Parameters.Count!=selects.Length)
        throw new InvalidOperationException();
      model = handlers.Domain.Model;
      this.selects = selects;
      lambda = le;
      bindings = new HashSet<SqlFetchParameterBinding>();
      parameterMapping = new Dictionary<ParameterExpression, SqlSelect>();
      evaluator = new ExpressionEvaluator(le);
      parameterExtractor = new ParameterExtractor(evaluator);
    }
  }
}
