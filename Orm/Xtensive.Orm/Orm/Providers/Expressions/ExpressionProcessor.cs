// Copyright (C) 2008-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kochetov
// Created:    2008.09.05

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.MemberCompilation;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.Providers
{
  internal sealed partial class ExpressionProcessor : ExpressionVisitor<SqlExpression>
  {
    [Flags]
    private enum ProcessorOptions
    {
      None = 0,
      FixBooleanExpressions = 1 << 0,
      PreferCaseOverVariant = 1 << 1,
      EmptyStringIsNull = 1 << 2,
      DateTimeEmulation = 1 << 3,
      DateTimeOffsetEmulation = 1 << 4,
      SpecialByteArrayComparison = 1 << 5
    }

    private static readonly SqlExpression SqlFalse = SqlDml.Literal(false);
    private static readonly SqlExpression SqlTrue = SqlDml.Literal(true);

    private readonly SqlCompiler compiler;
    private readonly LambdaExpression lambda;
    private readonly StorageDriver driver;
    private readonly BooleanExpressionConverter booleanExpressionConverter;
    private readonly IMemberCompilerProvider<SqlExpression> memberCompilerProvider;
    private readonly IReadOnlyList<SqlExpression>[] sourceColumns;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private readonly ProviderInfo providerInfo;
    private readonly ProcessorOptions options;

    private readonly List<ParameterExpression> activeParameters = new();
    private readonly Dictionary<ParameterExpression, IReadOnlyList<SqlExpression>> sourceMapping = new();
    private readonly Dictionary<QueryParameterIdentity, QueryParameterBinding> bindingsWithIdentity = new();
    private readonly List<QueryParameterBinding> otherBindings = new();

    private bool executed;

    private bool FixBooleanExpressions => (options & ProcessorOptions.FixBooleanExpressions) !=0;
    private bool PreferCaseOverVariant => options.HasFlag(ProcessorOptions.PreferCaseOverVariant);
    private bool EmptyStringIsNull => (options & ProcessorOptions.EmptyStringIsNull) != 0;
    private bool DateTimeEmulation => (options & ProcessorOptions.DateTimeEmulation) != 0;
    private bool DateTimeOffsetEmulation => (options & ProcessorOptions.DateTimeOffsetEmulation) != 0;
    private bool SpecialByteArrayComparison => (options & ProcessorOptions.SpecialByteArrayComparison) != 0;


    public SqlExpression Translate()
    {
      if (executed)
        throw new InvalidOperationException();
      executed = true;
      using (new ExpressionTranslationScope(providerInfo, driver, booleanExpressionConverter)) {
        return Visit(lambda);
      }
    }

    public IEnumerable<QueryParameterBinding> GetBindings()
    {
      return bindingsWithIdentity.Values.Concat(otherBindings);
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
      if (e.NodeType==ExpressionType.Convert && e.Type==WellKnownTypes.Object)
        type = ((UnaryExpression) e).Operand.Type;
      bool optimizeBooleanParameter = type==WellKnownTypes.Bool;
      type = type.StripNullable();
      var typeMapping = driver.GetTypeMapping(type);
      var expression = ParameterAccessorFactory.CreateAccessorExpression<object>(e);
      var bindingType = optimizeBooleanParameter
        ? QueryParameterBindingType.BooleanConstant
        : (smartNull
            ? QueryParameterBindingType.SmartNull
            : QueryParameterBindingType.Regular);
      var binding = RegisterParameterBinding(typeMapping, expression, bindingType);
      SqlExpression result;
      if (optimizeBooleanParameter) {
        result = SqlDml.Variant(binding, SqlFalse, SqlTrue);
        if (FixBooleanExpressions)
          result = booleanExpressionConverter.IntToBoolean(result);
      }
      else {
        result = binding.ParameterReference;
        if (type == WellKnownTypes.Bool && FixBooleanExpressions)
          result = booleanExpressionConverter.IntToBoolean(result);
        else if (typeMapping.ParameterCastRequired)
          result = SqlDml.Cast(result, typeMapping.MapType());
      }
      return result;
    }

    protected override SqlExpression VisitUnary(UnaryExpression expression)
    {
      var operand = Visit(expression.Operand);

      if (expression.Method!=null)
        return CompileMember(expression.Method, null, operand);

      switch (expression.NodeType) {
        case ExpressionType.ArrayLength:
          if (expression.Operand.Type!=WellKnownTypes.ByteArray)
            throw new NotSupportedException(string.Format(Strings.ExTypeXIsNotSupported, expression.Operand.Type));
          return SqlDml.Cast(SqlDml.BinaryLength(operand), driver.MapValueType(WellKnownTypes.Int32));
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
      if (sourceType==targetType || targetType==WellKnownTypes.Object || sourceType==WellKnownTypes.Object)
        return operand;
      if (IsEnumUnderlyingType(sourceType, targetType) || IsEnumUnderlyingType(targetType, sourceType))
        return operand;
      // Special case for boolean cast
      if (FixBooleanExpressions && IsBooleanExpression(cast.Operand)) {
        var result = SqlDml.Case();
        _ = result.Add(operand, 1);
        result.Else = 0;
        operand = result;
      }
      return SqlDml.Cast(operand, driver.MapValueType(targetType));
    }

    protected override SqlExpression VisitBinary(BinaryExpression expression)
    {
      // handle x.CompareTo(y) > 0 and similar comparisons
      var result = TryTranslateCompareExpression(expression);
      if (result is not null) {
        return result;
      }

      SqlExpression left;
      SqlExpression right;

      var expressionNodeType = expression.NodeType;
      var expressionLeft = expression.Left;
      var expressionRight = expression.Right;

      var isEqualityCheck = expressionNodeType is ExpressionType.Equal or ExpressionType.NotEqual;

      var isBooleanFixRequired = FixBooleanExpressions
        && (isEqualityCheck || expressionNodeType == ExpressionType.Coalesce)
        && (IsBooleanExpression(expressionLeft) || IsBooleanExpression(expressionRight));

      var isLeftCharToIntConvert = IsCharToIntConvert(expressionLeft);

      if (isLeftCharToIntConvert && IsCharToIntConvert(expressionRight)) {
        // chars are compared as integers, but we store them as strings and should compare them like strings.
        left = Visit(GetOperand(expressionLeft), isEqualityCheck);
        right = Visit(GetOperand(expressionRight), isEqualityCheck);
      }
      else if (isLeftCharToIntConvert && IsIntConstant(expressionRight)) {
        // another case of char comparison
        left = Visit(GetOperand(expressionLeft), isEqualityCheck);
        right = ConvertIntConstantToSingleCharString(expressionRight);
      }
      else if (IsIntConstant(expressionLeft) && IsCharToIntConvert(expressionRight)) {
        // another case of char comparison
        left = ConvertIntConstantToSingleCharString(expressionLeft);
        right = Visit(GetOperand(expressionRight), isEqualityCheck);
      }
      else {
        // regular case
        left = Visit(expressionLeft, isEqualityCheck);
        right = Visit(expressionRight, isEqualityCheck);
      }
      if (isBooleanFixRequired) {
        // boolean expressions should be compared as integers.
        // additional check is required because some type information might be lost.
        // we assume they already have correct format in that case.
        if (IsBooleanExpression(expressionLeft)) {
          left = booleanExpressionConverter.BooleanToInt(left);
        }
        if (IsBooleanExpression(expressionRight)) {
          right = booleanExpressionConverter.BooleanToInt(right);
        }
      }

      //handle SQLite DateTime comparsion
      if (DateTimeEmulation
          && left.NodeType != SqlNodeType.Null
          && right.NodeType != SqlNodeType.Null
          && IsComparisonExpression(expression)) {
        if (IsDateTimeExpression(expression.Left) || IsDateTimeExpression(expression.Right)) {
          left = SqlDml.Cast(left, SqlType.DateTime);
          right = SqlDml.Cast(right, SqlType.DateTime);
        }
        else if (IsDateOnlyExpression(expression.Left) || IsDateOnlyExpression(expression.Right)) {
          left = SqlDml.Cast(left, SqlType.Date);
          right = SqlDml.Cast(right, SqlType.Date);
        }
        else if (IsTimeOnlyExpression(expression.Left) || IsDateOnlyExpression(expression.Right)) {
          left = SqlDml.Cast(left, SqlType.Time);
          right = SqlDml.Cast(right, SqlType.Time);
        }
      }

      //handle SQLite DateTimeOffset comparsion
      if (DateTimeOffsetEmulation
          && left.NodeType != SqlNodeType.Null
          && right.NodeType != SqlNodeType.Null
          && IsComparisonExpression(expression)
          && (IsDateTimeOffsetExpression(expressionLeft) || IsDateTimeOffsetExpression(expressionRight))) {
        left = SqlDml.Cast(left, SqlType.DateTimeOffset);
        right = SqlDml.Cast(right, SqlType.DateTimeOffset);
      }

      //handle Oracle special syntax of BLOB comparison
      if (SpecialByteArrayComparison
        && (IsExpressionOf(expression.Left, WellKnownTypes.ByteArray) || IsExpressionOf(expression.Left, WellKnownTypes.ByteArray))) {
        var comparison = BuildByteArraySyntaxComparison(left, right);
        left = comparison.left;
        right = comparison.right;
      }

      // handle special cases
      result = TryTranslateBinaryExpressionSpecialCases(expression, left, right);
      if (result is not null) {
        return result;
      }

      // handle overloaded operators
      if (expression.Method != null) {
        return CompileMember(expression.Method, null, left, right);
      }

      //handle wrapped enums
      if (left is SqlContainer leftContainer) {
        left = TryUnwrapEnum(leftContainer);
      }
      if (right is SqlContainer rightContainer) {
        right = TryUnwrapEnum(rightContainer);
      }

      switch (expressionNodeType) {
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
          var coalesce = (SqlExpression) SqlDml.Coalesce(left, right);
          if (isBooleanFixRequired) {
            coalesce = booleanExpressionConverter.IntToBoolean(coalesce);
          }
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

      if (ifTrue is SqlContainer ifTrueContainer)
        ifTrue = TryUnwrapEnum(ifTrueContainer);
      if (ifFalse is SqlContainer ifFalseContainer)
        ifFalse = TryUnwrapEnum(ifFalseContainer);

      var fixExpressions = FixBooleanExpressions;

      var boolCheck = fixExpressions
        ? booleanExpressionConverter.BooleanToInt(check)
        : check;
      var varCheck = boolCheck as SqlVariant;
      if (!PreferCaseOverVariant && varCheck is not null) {
        return SqlDml.Variant(varCheck.Id, ifFalse, ifTrue);
      }
      var @case = SqlDml.Case();
      if (fixExpressions && IsBooleanExpression(expression)) {
        @case[check] = booleanExpressionConverter.BooleanToInt(ifTrue);
        @case.Else = booleanExpressionConverter.BooleanToInt(ifFalse);
        return booleanExpressionConverter.IntToBoolean(@case);
      }
      else {
        @case[check] = ifTrue;
        @case.Else = ifFalse;
        return @case;
      }
    }

    protected override SqlExpression VisitConstant(ConstantExpression expression)
    {
      if (expression.Value==null)
        return FixBooleanExpressions && expression.Type==WellKnownTypes.NullableBool
          ? booleanExpressionConverter.IntToBoolean(SqlDml.Null)
          : SqlDml.Null;
      var type = expression.Type;
      if (type==WellKnownTypes.Object)
        type = expression.Value.GetType();
      type = type.StripNullable();
      if (FixBooleanExpressions && type==WellKnownTypes.Bool) {
        var literal = SqlDml.Literal((bool) expression.Value);
        return booleanExpressionConverter.IntToBoolean(literal);
      }
      return SqlDml.LiteralOrContainer(expression.Value);
    }

    protected override SqlExpression VisitDefault(DefaultExpression d) => throw new NotSupportedException();

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

      var arguments = mc.Arguments.SelectToArray(a => Visit(a));
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
      if (applyParameter!=null) {
        if (compiler==null)
          throw Exceptions.InternalError(Strings.ExOuterParameterReferenceFoundButNoSqlCompilerProvided, OrmLog.Instance);
        result = compiler.GetOuterExpression(applyParameter, columnIndex);
      }
      else {
        var queryRef = sourceMapping[(ParameterExpression) tupleAccess.Object];
        result = queryRef[columnIndex];
      }
      if (FixBooleanExpressions && IsBooleanExpression(tupleAccess))
        result = booleanExpressionConverter.IntToBoolean(result);
      return result;
    }

    protected override SqlExpression VisitLambda(LambdaExpression l)
    {
      if (activeParameters.Count>0)
        throw new InvalidOperationException();
      activeParameters.AddRange(l.Parameters);
      for (int i = 0, count = l.Parameters.Count; i < count; i++) {
        var p = l.Parameters[i];
        sourceMapping[p] = sourceColumns[i];
      }
      var body = Visit(l.Body);
      var sqlContainer = body as SqlContainer;
      if (sqlContainer!=null)
        return TryUnwrapEnum(sqlContainer);
      return body;
    }

    protected override SqlExpression VisitNew(NewExpression n)
    {
      return CompileMember(n.Constructor, null, n.Arguments.SelectToArray(a => Visit(a)));
    }

    protected override SqlExpression VisitNewArray(NewArrayExpression expression)
    {
      if (expression.NodeType!=ExpressionType.NewArrayInit)
        throw new NotSupportedException();
      var expressions = expression.Expressions.SelectToArray(e => Visit(e));
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

    private SqlExpression TryUnwrapEnum(SqlContainer container)
    {
      var valueType = container.Value.GetType();
      if (valueType.IsEnum)
        return SqlDml.Literal(Convert.ChangeType(container.Value, Enum.GetUnderlyingType(valueType)));
      return container;
    }


    // Constructors

    public ExpressionProcessor(LambdaExpression lambda,
      HandlerAccessor handlers,
      SqlCompiler compiler,
      in bool preferCaseOverVariant,
      params IReadOnlyList<SqlExpression>[] sourceColumns)
    {
      ArgumentValidator.EnsureArgumentNotNull(lambda, "lambda");
      ArgumentValidator.EnsureArgumentNotNull(handlers, "handlers");
      ArgumentValidator.EnsureArgumentNotNull(sourceColumns, "sourceColumns");

      if (lambda.Parameters.Count != sourceColumns.Length)
        throw Exceptions.InternalError(Strings.ExParametersCountIsNotSameAsSourceColumnListsCount, OrmLog.Instance);
      if (sourceColumns.Any(list => list.Any(c => c is null)))
        throw Exceptions.InternalError(Strings.ExSourceColumnListContainsNullValues, OrmLog.Instance);

      this.compiler = compiler; // This might be null, check before use!
      this.lambda = lambda;
      this.sourceColumns = sourceColumns;

      providerInfo = handlers.ProviderInfo;
      driver = handlers.StorageDriver;
      memberCompilerProvider = handlers.DomainHandler.GetMemberCompilerProvider<SqlExpression>();

      evaluator = new ExpressionEvaluator(lambda);
      parameterExtractor = new ParameterExtractor(evaluator);

      options = ProcessorOptions.None;
      if (!providerInfo.Supports(ProviderFeatures.FullFeaturedBooleanExpressions)) {
        options |= ProcessorOptions.FixBooleanExpressions;
        booleanExpressionConverter = new BooleanExpressionConverter(driver);
      }
      if (providerInfo.Supports(ProviderFeatures.TreatEmptyStringAsNull))
        options |= ProcessorOptions.EmptyStringIsNull;
      if (providerInfo.Supports(ProviderFeatures.DateTimeEmulation))
        options |= ProcessorOptions.DateTimeEmulation;
      if (providerInfo.Supports(ProviderFeatures.DateTimeOffsetEmulation))
        options |= ProcessorOptions.DateTimeOffsetEmulation;
      if (providerInfo.ProviderName.Equals(WellKnown.Provider.Oracle))
        options |= ProcessorOptions.SpecialByteArrayComparison;
      if (preferCaseOverVariant)
        options |= ProcessorOptions.PreferCaseOverVariant;
    }
  }
}
