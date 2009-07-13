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
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers.Sql.Resources;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Helpers;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal class ExpressionProcessor : ExpressionVisitor<SqlExpression>
  {
    private readonly IMemberCompilerProvider<SqlExpression> memberCompilerProvider;
    private readonly DomainModel model;
    private readonly SqlSelect[] selects;
    private readonly SqlQueryRef[] queryRefs;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private readonly LambdaExpression lambda;
    private readonly HashSet<SqlFetchParameterBinding> bindings;
    private readonly List<ParameterExpression> activeParameters;
    private readonly Dictionary<ParameterExpression, SqlSelect> selectParameterMapping;
    private readonly Dictionary<ParameterExpression, SqlQueryRef> queryRefParameterMapping;
    private readonly SqlValueTypeMapper valueTypeMapper;
    private readonly ICompiler compiler;

    private bool fixBooleanExpressions;
    private bool executed;
    private bool useSelect;
    
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
      type = StripNullable(type);
      var typeMapping = valueTypeMapper.GetTypeMapping(type);
      var expression = parameterExtractor.ExtractParameter<object>(e);
      var binding = new SqlFetchParameterBinding(expression.CachingCompile(), typeMapping, smartNull);
      bindings.Add(binding);
      SqlExpression result = binding.ParameterReference;
      if (fixBooleanExpressions && type==typeof(bool))
        result = IntToBoolean(result);
      else if (typeMapping.ParameterCastRequired)
        result = SqlDml.Cast(result, typeMapping.BuildSqlType());
      return result;
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
          return SqlDml.Cast(SqlDml.BinaryLength(operand), SqlType.Int32);
        case ExpressionType.Negate:
        case ExpressionType.NegateChecked:
          return SqlDml.Negate(operand);
        case ExpressionType.UnaryPlus:
          return operand;
        case ExpressionType.Not:
          return IsBooleanExpression(expression.Operand)
            ? SqlDml.Not(operand)
            : SqlDml.BitNot(operand);
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
          return VisitCast(expression, operand);
      }
      return operand;
    }

    private SqlExpression VisitCast(UnaryExpression cast, SqlExpression operand)
    {
      var sourceType = StripNullable(cast.Operand.Type);
      var targetType = StripNullable(cast.Type);
      if (sourceType==targetType || targetType==typeof(object))
        return operand;
      return SqlDml.Cast(operand, valueTypeMapper.BuildSqlValueType(targetType, null));
    }

    protected override SqlExpression VisitBinary(BinaryExpression expression)
    {
      SqlExpression result = TryTranslateCompareExpression(expression);
      if (result != null)
        return result;

      SqlExpression left;
      SqlExpression right;

      bool isEqualityCheck = expression.NodeType==ExpressionType.Equal
                          || expression.NodeType==ExpressionType.NotEqual;

      bool isBooleanFixRequired = fixBooleanExpressions
        && (isEqualityCheck || expression.NodeType==ExpressionType.Coalesce)
        && IsBooleanExpression(expression.Left)
        && IsBooleanExpression(expression.Right);

      if (IsCharToIntConvert(expression.Left) && IsCharToIntConvert(expression.Right)) {
        // chars are compared as integers, but we store them as strings and should compare them like strings.
        left = Visit(((UnaryExpression) expression.Left).Operand, isEqualityCheck);
        right = Visit(((UnaryExpression) expression.Right).Operand, isEqualityCheck);
      } else if (isBooleanFixRequired) {
        // boolean expressions should be compared as integers
        left = BooleanToInt(Visit(expression.Left, isEqualityCheck));
        right = BooleanToInt(Visit(expression.Right, isEqualityCheck));
      } else {
        left = Visit(expression.Left, isEqualityCheck);
        right = Visit(expression.Right, isEqualityCheck);
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
          return SqlDml.Add(left, right);
        case ExpressionType.And:
          return IsBooleanExpression(expression.Left)
            ? SqlDml.And(left, right)
            : SqlDml.BitAnd(left, right);
        case ExpressionType.AndAlso:
          return SqlDml.And(left, right);
        case ExpressionType.Coalesce:
          SqlExpression coalesce = SqlDml.Coalesce(left, right);
          if (isBooleanFixRequired)
            coalesce = IntToBoolean(coalesce);
          return coalesce;
        case ExpressionType.Divide:
          return SqlDml.Divide(left, right);
        case ExpressionType.Equal:
          return SqlDml.Equals(left, right);
        case ExpressionType.ExclusiveOr:
          return SqlDml.BitXor(left, right);
        case ExpressionType.GreaterThan:
          return SqlDml.GreaterThan(left, right);
        case ExpressionType.GreaterThanOrEqual:
          return SqlDml.GreaterThanOrEquals(left, right);
        case ExpressionType.LessThan:
          return SqlDml.LessThan(left, right);
        case ExpressionType.LessThanOrEqual:
          return SqlDml.LessThanOrEquals(left, right);
        case ExpressionType.Modulo:
          return SqlDml.Modulo(left, right);
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked:
          return SqlDml.Multiply(left, right);
        case ExpressionType.NotEqual:
          return SqlDml.NotEquals(left, right);
        case ExpressionType.Or:
          return IsBooleanExpression(expression.Left)
            ? SqlDml.Or(left, right)
            : SqlDml.BitOr(left, right);
        case ExpressionType.OrElse:
          return SqlDml.Or(left, right);
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked:
          return SqlDml.Subtract(left, right);
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
      if (fixBooleanExpressions && IsBooleanExpression(expression)) {
        var c = SqlDml.Case();
        c[check] = BooleanToInt(ifTrue);
        c.Else = BooleanToInt(ifFalse);
        return IntToBoolean(c);
      } else {
        var c = SqlDml.Case();
        c[check] = ifTrue;
        c.Else = ifFalse;
        return c;
      }
    }

    protected override SqlExpression VisitConstant(ConstantExpression expression)
    {
      if (expression.Value==null)
        return fixBooleanExpressions && expression.Type==typeof (bool?)
          ? IntToBoolean(SqlDml.Null)
          : SqlDml.Null;
      var type = expression.Type;
      if (type==typeof(object))
        type = expression.Value.GetType();
      type = StripNullable(type);
      if (fixBooleanExpressions && type==typeof (bool))
        return (bool) expression.Value ? IntToBoolean(1) : IntToBoolean(0);
      return SqlDml.LiteralOrContainer(expression.Value, type);
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
      if (mc.AsTupleAccess(activeParameters) != null)
        return VisitTupleAccess(mc);

      var arguments = mc.Arguments.Select(a => Visit(a)).ToArray();
      var mi = mc.Method;

      if (mc.Object!=null && mc.Object.Type!=mi.ReflectedType)
        mi = mc.Object.Type.GetMethod(mi.Name, mi.GetParameterTypes());

      return CompileMember(mi, Visit(mc.Object), arguments);
    }

    private SqlExpression VisitTupleAccess(MethodCallExpression tupleAccess)
    {
      int columnIndex = tupleAccess.GetTupleAccessArgument();
      var parameter = tupleAccess.GetApplyParameter();
      if (parameter!=null) {
        ExecutableProvider provider = compiler.CompiledSources[parameter];
        if (!compiler.IsCompatible(provider)) {
          provider = compiler.ToCompatible(provider);
          compiler.CompiledSources.ReplaceBound(parameter, provider);
        }
        var sqlProvider = (SqlProvider) provider;
        return sqlProvider.PermanentReference[columnIndex];
      }
      SqlExpression result;
      if (useSelect) {
        var sqlSelect = selectParameterMapping[(ParameterExpression) tupleAccess.Object];
        result = sqlSelect[columnIndex];
      } else {
        var queryRef = queryRefParameterMapping[(ParameterExpression) tupleAccess.Object];
        result = queryRef[columnIndex];
      }
      if (fixBooleanExpressions && IsBooleanExpression(tupleAccess))
        result = IntToBoolean(result);
      return result;
    }
    
    protected override SqlExpression VisitLambda(LambdaExpression l)
    {
      if (activeParameters.Count>0)
        throw new InvalidOperationException();
      activeParameters.AddRange(l.Parameters);
      for (int i = 0; i < l.Parameters.Count; i++) {
        var p = l.Parameters[i];
        if (useSelect)
          selectParameterMapping[p] = selects[i];
        else
          queryRefParameterMapping[p] = queryRefs[i];
      }
      return Visit(l.Body);
    }

    protected override SqlExpression VisitNew(NewExpression n)
    {
      return CompileMember(n.Constructor, null, n.Arguments.Select(a => Visit(a)).ToArray());
    }

    protected override SqlExpression VisitNewArray(NewArrayExpression expression)
    {
      if (expression.NodeType!=ExpressionType.NewArrayInit)
        throw new NotSupportedException();
      var expressions = expression.Expressions.Select(e => Visit(e)).ToArray();
      return SqlDml.Container(expressions);
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

    private static SqlExpression TryTranslateEqualitySpecialCases(SqlExpression left, SqlExpression right)
    {
      if (right.NodeType == SqlNodeType.Null)
        return SqlDml.IsNull(left);
      if (right.NodeType == SqlNodeType.Parameter)
        return SqlDml.Variant(SqlDml.Equals(left, right), SqlDml.IsNull(left), ((SqlParameterRef) right).Parameter);
      return null;
    }

    private static SqlExpression TryTranslateInequalitySpecialCases(SqlExpression left, SqlExpression right)
    {
      if (right.NodeType == SqlNodeType.Null)
        return SqlDml.IsNotNull(left);
      if (right.NodeType == SqlNodeType.Parameter)
        return SqlDml.Variant(SqlDml.NotEquals(left, right), SqlDml.IsNotNull(left), ((SqlParameterRef)right).Parameter);
      return null;
    }

    private SqlExpression CompileMember(MemberInfo member, SqlExpression instance, params SqlExpression[] arguments)
    {
      var memberCompiler = memberCompilerProvider.GetCompiler(member);
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

    private static Type StripNullable(Type type)
    {
      return type.IsNullable() ? type.GetGenericArguments()[0] : type;
    }

    private static bool IsBooleanExpression(Expression expression)
    {
      return StripNullable(expression.Type)==typeof (bool);
    }

    #endregion

    public static SqlExpression IntToBoolean(SqlExpression expression)
    {
      // optimization: omitting IntToBoolean(BooleanToInt(x)) sequences
      if (expression.NodeType==SqlNodeType.Cast) {
        var operand = ((SqlCast) expression).Operand;
        if (operand.NodeType==SqlNodeType.Case) {
          var _case = (SqlCase) operand;
          if (_case.Count == 1) {
            var firstCaseItem = _case.First();
            var whenTrue = firstCaseItem.Value as SqlLiteral<int>;
            var whenFalse = _case.Else as SqlLiteral<int>;
            if (!ReferenceEquals(whenTrue, null)
             && !ReferenceEquals(whenFalse, null)
             && whenTrue.Value==1
             && whenFalse.Value==0)
              return firstCaseItem.Key;
          }
        }
      }

      return SqlDml.NotEquals(expression, 0);
    }

    public static SqlExpression BooleanToInt(SqlExpression expression)
    {
      // optimization: omitting BooleanToInt(IntToBoolean(x)) sequences
      if (expression.NodeType==SqlNodeType.NotEquals) {
        var binary = (SqlBinary) expression;
        var left = binary.Left;
        var right = binary.Right as SqlLiteral<int>;
        if (!ReferenceEquals(right, null) && right.Value==0)
          return left;
      }

      var result = SqlDml.Case();
      result.Add(expression, 1);
      result.Else = 0;
      return SqlDml.Cast(result, SqlType.Boolean);
    }
    
    // Constructors

    public ExpressionProcessor(LambdaExpression le, ICompiler compiler, HandlerAccessor handlers,
      bool fixBooleanExpressions, params SqlSelect[] selects)
      : this(le, compiler, handlers, fixBooleanExpressions)
    {
      ArgumentValidator.EnsureArgumentNotNull(selects, "selects");
      if (le.Parameters.Count!=selects.Length)
        throw new InvalidOperationException();
      this.selects = selects;
      selectParameterMapping = new Dictionary<ParameterExpression, SqlSelect>();
      useSelect = true;
    }

    public ExpressionProcessor(LambdaExpression le, ICompiler compiler, HandlerAccessor handlers,
      bool fixBooleanExpressions, params SqlQueryRef[] queryRefs)
      : this(le, compiler, handlers, fixBooleanExpressions)
    {
      ArgumentValidator.EnsureArgumentNotNull(queryRefs, "queryRefs");
      if (le.Parameters.Count!=queryRefs.Length)
        throw new InvalidOperationException();
      this.queryRefs = queryRefs;
      queryRefParameterMapping = new Dictionary<ParameterExpression, SqlQueryRef>();
      useSelect = false;
    }

    private ExpressionProcessor(LambdaExpression le, ICompiler compiler, HandlerAccessor handlers, bool fixBooleanExpressions)
    {
      this.compiler = compiler;
      this.fixBooleanExpressions = fixBooleanExpressions;
      memberCompilerProvider = handlers.DomainHandler.GetMemberCompilerProvider<SqlExpression>();
      valueTypeMapper = ((DomainHandler) handlers.DomainHandler).ValueTypeMapper;
      model = handlers.Domain.Model;
      lambda = le;
      bindings = new HashSet<SqlFetchParameterBinding>();
      activeParameters = new List<ParameterExpression>();
      evaluator = new ExpressionEvaluator(le);
      parameterExtractor = new ParameterExtractor(evaluator);
    }
  }
}
