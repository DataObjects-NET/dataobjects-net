// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Compilation;
using Xtensive.Orm.Rse.Helpers;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers.Sql.Expressions
{
  internal sealed partial class ExpressionProcessor : ExpressionVisitor<SqlExpression>
  {
    private static readonly SqlExpression SqlFalse = SqlDml.Literal(false);
    private static readonly SqlExpression SqlTrue = SqlDml.Literal(true);

    private readonly StorageDriver driver;
    private readonly BooleanExpressionConverter booleanExpressionConverter;
    private readonly IMemberCompilerProvider<SqlExpression> memberCompilerProvider;
    private readonly List<SqlExpression>[] sourceColumns;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private readonly LambdaExpression lambda;
    private readonly HashSet<QueryParameterBinding> bindings;
    private readonly List<ParameterExpression> activeParameters;
    private readonly Dictionary<ParameterExpression, List<SqlExpression>> sourceMapping;
    private readonly SqlCompiler compiler;

    private bool fixBooleanExpressions;
    private bool emptyStringIsNull;
    private ProviderInfo providerInfo;

    private bool executed;

    public HashSet<QueryParameterBinding> Bindings { get { return bindings; } }
    
    public SqlExpression Translate()
    {
      if (executed)
        throw new InvalidOperationException();
      executed = true;
      using (new ExpressionTranslationScope(providerInfo, driver, booleanExpressionConverter)) {
        return Visit(lambda);
      }
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
        return VisitConstant(ExpressionEvaluator.Evaluate(e));
      }
      return base.Visit(e);
    }

    private SqlExpression VisitParameterAccess(Expression e, bool smartNull)
    {
      var type = e.Type;
      // In rare cases (when calculated column is just parameter access) we need to strip cast to object.
      if (e.NodeType==ExpressionType.Convert && e.Type==typeof(object))
        type = ((UnaryExpression) e).Operand.Type;
      bool optimizeBooleanParameter = type==typeof (bool);
      type = type.StripNullable();
      var typeMapping = driver.GetTypeMapping(type);
      var expression = parameterExtractor.ExtractParameter<object>(e);
      var bindingType = optimizeBooleanParameter
        ? QueryParameterBindingType.BooleanConstant
        : (smartNull
            ? QueryParameterBindingType.SmartNull
            : QueryParameterBindingType.Regular);
      var binding = new QueryParameterBinding(expression.CachingCompile(), typeMapping, bindingType);
      bindings.Add(binding);
      SqlExpression result;
      if (optimizeBooleanParameter) {
        result = SqlDml.Variant(binding, SqlFalse, SqlTrue);
        if (fixBooleanExpressions)
          result = booleanExpressionConverter.IntToBoolean(result);
      }
      else {
        result = binding.ParameterReference;
        if (type==typeof(bool) && fixBooleanExpressions)
          result = booleanExpressionConverter.IntToBoolean(result);
        else if (typeMapping.ParameterCastRequired)
          result = SqlDml.Cast(result, typeMapping.BuildSqlType());
      }
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
          return SqlDml.Cast(SqlDml.BinaryLength(operand), driver.BuildValueType(typeof (int)));
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
      var sourceType = cast.Operand.Type.StripNullable();
      var targetType = cast.Type.StripNullable();
      if (sourceType==targetType || targetType==typeof(object) || sourceType==typeof(object))
        return operand;
      // Special case for boolean cast
      if (fixBooleanExpressions && IsBooleanExpression(cast.Operand)) {
        var result = SqlDml.Case();
        result.Add(operand, 1);
        result.Else = 0;
        operand = result;
      }
      return SqlDml.Cast(operand, driver.BuildValueType(targetType));
    }

    protected override SqlExpression VisitBinary(BinaryExpression expression)
    {
      // handle x.CompareTo(y) > 0 and similar comparisons
      SqlExpression result = TryTranslateCompareExpression(expression);
      if (!result.IsNullReference())
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
        left = Visit(GetOperand(expression.Left), isEqualityCheck);
        right = Visit(GetOperand(expression.Right), isEqualityCheck);
      }
      else if (IsCharToIntConvert(expression.Left) && IsIntConstant(expression.Right)) {
        // another case of char comparison
        left = Visit(GetOperand(expression.Left), isEqualityCheck);
        right = ConvertIntConstantToSingleCharString(expression.Right);
      }
      else if (IsIntConstant(expression.Left) && IsCharToIntConvert(expression.Right)) {
        // another case of char comparison
        left = ConvertIntConstantToSingleCharString(expression.Left);
        right = Visit(GetOperand(expression.Right), isEqualityCheck);
      }
      else if (isBooleanFixRequired) {
        // boolean expressions should be compared as integers
        left = booleanExpressionConverter.BooleanToInt(Visit(expression.Left, isEqualityCheck));
        right = booleanExpressionConverter.BooleanToInt(Visit(expression.Right, isEqualityCheck));
      }
      else {
        // regular case
        left = Visit(expression.Left, isEqualityCheck);
        right = Visit(expression.Right, isEqualityCheck);
      }
      
      // handle special cases
      result = TryTranslateBinaryExpressionSpecialCases(expression, left, right);
      if (!result.IsNullReference())
        return result;

      // handle overloaded operators
      if (expression.Method!=null)
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
          coalesce = booleanExpressionConverter.IntToBoolean(coalesce);
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
      var boolCheck = fixBooleanExpressions
        ? booleanExpressionConverter.BooleanToInt(check)
        : check;
      var varCheck = boolCheck as SqlVariant;
      if (!varCheck.IsNullReference())
        return SqlDml.Variant(varCheck.Id, ifFalse, ifTrue);
      if (fixBooleanExpressions && IsBooleanExpression(expression)) {
        var c = SqlDml.Case();
        c[check] = booleanExpressionConverter.BooleanToInt(ifTrue);
        c.Else = booleanExpressionConverter.BooleanToInt(ifFalse);
        return booleanExpressionConverter.IntToBoolean(c);
      }
      else {
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
          ? booleanExpressionConverter.IntToBoolean(SqlDml.Null)
          : SqlDml.Null;
      var type = expression.Type;
      if (type==typeof(object))
        type = expression.Value.GetType();
      type = type.StripNullable();
      if (fixBooleanExpressions && type==typeof (bool))
        return (bool) expression.Value ? booleanExpressionConverter.IntToBoolean(1) : booleanExpressionConverter.IntToBoolean(0);
      return SqlDml.LiteralOrContainer(expression.Value);
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
      SqlExpression result;
      var applyParameter = tupleAccess.GetApplyParameter();
      if (applyParameter != null) {
        result = VisitOuterParameterReference(columnIndex, applyParameter);
      }
      else {
        var queryRef = sourceMapping[(ParameterExpression)tupleAccess.Object];
        result = queryRef[columnIndex];
      }
      if (fixBooleanExpressions && IsBooleanExpression(tupleAccess))
        result = booleanExpressionConverter.IntToBoolean(result);
      return result;
    }

    private SqlExpression VisitOuterParameterReference(int columnIndex, ApplyParameter parameter)
    {
      if (compiler==null)
        throw Exceptions.InternalError(Strings.ExOuterParameterReferenceFoundButNoSqlCompilerProvided, Log.Instance);

      ExecutableProvider provider = compiler.OuterReferences[parameter];

      // TODO: Check out this sh..t
      var sqlProvider = (SqlProvider) provider;
      var permanentReference = sqlProvider.PermanentReference;
      if (permanentReference.Columns.Count!=sqlProvider.Request.Statement.Columns.Count)
        return compiler.ExtractColumnExpressions(sqlProvider.Request.Statement, sqlProvider.Origin)[columnIndex];
      return permanentReference[columnIndex];
    }

    protected override SqlExpression VisitLambda(LambdaExpression l)
    {
      if (activeParameters.Count>0)
        throw new InvalidOperationException();
      activeParameters.AddRange(l.Parameters);
      for (int i = 0; i < l.Parameters.Count; i++) {
        var p = l.Parameters[i];
        sourceMapping[p] = sourceColumns[i];
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


    // Constructors

    public ExpressionProcessor(
      LambdaExpression lambda, HandlerAccessor handlers, SqlCompiler compiler, params List<SqlExpression>[] sourceColumns)
    {
      ArgumentValidator.EnsureArgumentNotNull(lambda, "lambda");
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      ArgumentValidator.EnsureArgumentNotNull(sourceColumns, "sourceColumns");

      this.compiler = compiler; // This might be null, check before use!
      this.lambda = lambda;
      this.sourceColumns = sourceColumns;

      providerInfo = handlers.ProviderInfo;
      driver = handlers.StorageDriver;

      fixBooleanExpressions = !providerInfo.Supports(ProviderFeatures.FullFeaturedBooleanExpressions);
      emptyStringIsNull = providerInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull);
      memberCompilerProvider = handlers.DomainHandler.GetMemberCompilerProvider<SqlExpression>();

      bindings = new HashSet<QueryParameterBinding>();
      activeParameters = new List<ParameterExpression>();
      evaluator = new ExpressionEvaluator(lambda);
      parameterExtractor = new ParameterExtractor(evaluator);

      if (fixBooleanExpressions)
        booleanExpressionConverter = new BooleanExpressionConverter(driver);
      if (lambda.Parameters.Count!=sourceColumns.Length)
        throw Exceptions.InternalError(Strings.ExParametersCountIsNotSameAsSourceColumnListsCount, Log.Instance);
      if (sourceColumns.Any(list => list.Any(c => c.IsNullReference())))
        throw Exceptions.InternalError(Strings.ExSourceColumnListContainsNullValues, Log.Instance);
      sourceMapping = new Dictionary<ParameterExpression, List<SqlExpression>>();
    }
  }
}
