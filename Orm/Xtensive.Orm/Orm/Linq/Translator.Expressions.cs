// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.02.27

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Internals;
using Xtensive.Orm.FullTextSearchCondition.Nodes;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Linq.Rewriters;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Reflection;
using FieldInfo = System.Reflection.FieldInfo;
using Tuple = Xtensive.Tuples.Tuple;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Linq
{
  internal sealed partial class Translator
  {
    private static readonly Type OrmCollectionExtensionsType = typeof(CollectionExtensionsEx);
    private static readonly Type OrmQueryableExtensionsType = typeof(QueryableExtensions);
    private static readonly ParameterExpression ParameterContextParam = Expression.Parameter(WellKnownOrmTypes.ParameterContext, "context");
    private static readonly ConstantExpression
      NullKeyExpression = Expression.Constant(null, WellKnownOrmTypes.Key),
      FalseExpression = Expression.Constant(false),
      TrueExpression = Expression.Constant(true);

    private static IReadOnlyDictionary<Parameter<Tuple>, Tuple> EmptyTupleParameterBindings { get; } = new Dictionary<Parameter<Tuple>, Tuple>();

    protected override Expression VisitTypeIs(TypeBinaryExpression tb)
    {
      var expression = tb.Expression;
      Type expressionType = expression.Type;
      Type operandType = tb.TypeOperand;
      if (operandType.IsAssignableFrom(expressionType)) {
        return TrueExpression;
      }

      // Structure
      var memberType = expression.GetMemberType();
      if (memberType == MemberType.Structure
        && WellKnownOrmTypes.Structure.IsAssignableFrom(operandType)) {
        return FalseExpression;
      }

      // Entity
      if (memberType == MemberType.Entity
        && WellKnownOrmInterfaces.Entity.IsAssignableFrom(operandType)) {
        TypeInfo type = context.Model.Types[operandType];

        var typeInfos = type.AllDescendants.ToHashSet();
        typeInfos.UnionWith(type.AllImplementors);
        _ = typeInfos.Add(type);
        var typeIds = typeInfos.Select(context.TypeIdRegistry.GetTypeId);
        var typeIdPropAccessor = Expression.MakeMemberAccess(expression, WellKnownMembers.TypeId);
        Expression filterByTypeIdExpression = null;
        foreach (var typeId in typeIds) {
          filterByTypeIdExpression = MakeBooleanExpression(
            filterByTypeIdExpression,
            typeIdPropAccessor,
            Expression.Constant(typeId),
            ExpressionType.Equal,
            ExpressionType.OrElse);
        }

        return Visit(filterByTypeIdExpression);
      }

      throw new NotSupportedException(Strings.ExTypeIsMethodSupportsOnlyEntitiesAndStructures);
    }

    protected override Expression Visit(Expression e)
    {
      if (e == null)
        return null;
      if (e.IsProjection())
        return e;
      if (State.NonVisitableExpressions.Contains(e))
        return e;
      if (context.Evaluator.CanBeEvaluated(e)) {
        if (WellKnownInterfaces.Queryable.IsAssignableFrom(e.Type))
          return base.Visit(ExpressionEvaluator.Evaluate(e));
        return context.ParameterExtractor.IsParameter(e)
          ? e
          : ExpressionEvaluator.Evaluate(e);
      }
      return e.NodeType == ExpressionType.Index ? VisitIndex((IndexExpression) e) : base.Visit(e);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      if (e is ExtendedExpression)
        return e;
      return base.VisitUnknown(e);
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    protected override Expression VisitUnary(UnaryExpression u)
    {
      switch (u.NodeType) {
        case ExpressionType.TypeAs:
          if (u.GetMemberType() is MemberType.Entity)
            return VisitTypeAs(u.Operand, u.Type);
          break;
        case ExpressionType.Convert:
        case ExpressionType.ConvertChecked:
          if (u.GetMemberType() is MemberType.Entity) {
            if (u.Type == u.Operand.Type
              || u.Type.IsAssignableFrom(u.Operand.Type)
              || !WellKnownOrmInterfaces.Entity.IsAssignableFrom(u.Operand.Type))
              return base.VisitUnary(u);
            throw new InvalidOperationException(String.Format(Strings.ExDowncastFromXToXNotSupportedUseOfTypeOrAsOperatorInstead, u, u.Operand.Type, u.Type));
          }
          else if (u.Type == WellKnownTypes.Object && State.ShouldOmitConvertToObject) {
            var expression = u.StripCasts();
            return Visit(expression);
          }
          break;
      }
      return u.Type == WellKnownInterfaces.Queryable
        ? Visit(u.Operand)
        : base.VisitUnary(u);
    }

    protected override Expression VisitLambda(LambdaExpression le)
    {
      using (CreateLambdaScope(le, allowCalculableColumnCombine: false)) {
        Expression body = le.Body;
        if (!State.IsTailMethod)
          body = NullComparsionRewriter.Rewrite(body);
        body = Visit(body);
        ParameterExpression parameter = le.Parameters[0];
        var nodeType = body.NodeType;
        var shouldTranslate =
          (body.NodeType is not ExpressionType.New || body.IsNewExpressionSupportedByStorage())
          && body.NodeType is not ExpressionType.MemberInit
          && !(body.NodeType is ExpressionType.Constant && State.BuildingProjection);
        if (shouldTranslate)
          body = body.IsProjection()
            ? BuildSubqueryResult((ProjectionExpression) body, le.Body.Type)
            : ProcessProjectionElement(body);
        ProjectionExpression projection = context.Bindings[parameter];
        return new ItemProjectorExpression(body, projection.ItemProjector.DataSource, context);
      }
    }

    protected override MemberAssignment VisitMemberAssignment(MemberAssignment ma)
    {
      Expression expression;
      using (CreateScope(new TranslatorState(State) { CalculateExpressions = false })) {
        expression = Visit(ma.Expression);
      }

      expression = expression.IsProjection()
        ? BuildSubqueryResult((ProjectionExpression) expression, ma.Expression.Type)
        : ProcessProjectionElement(expression);

      if (expression != ma.Expression)
        return Expression.Bind(ma.Member, expression);
      return ma;
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
      Expression left;
      Expression right;
      MemberType memberType = binaryExpression.Left.Type == WellKnownTypes.Object
        ? binaryExpression.Right.GetMemberType()
        : binaryExpression.Left.GetMemberType();
      if (memberType == MemberType.EntitySet) {
        if (context.Evaluator.CanBeEvaluated(binaryExpression.Left)) {
          left = ExpressionEvaluator.Evaluate(binaryExpression.Left);
        }
        else {
          left = (binaryExpression.Left is MemberExpression leftMemberAccess
              && leftMemberAccess.Member.ReflectedType.IsClosure())
            ? ExpressionEvaluator.Evaluate(leftMemberAccess)
            : Visit(binaryExpression.Left);
        }
        if (context.Evaluator.CanBeEvaluated(binaryExpression.Right)) {
          right = ExpressionEvaluator.Evaluate(binaryExpression.Right);
        }
        else {
          right = (binaryExpression.Right is MemberExpression rightMemberAccess
              && rightMemberAccess.Member.ReflectedType.IsClosure())
            ? ExpressionEvaluator.Evaluate(rightMemberAccess)
            : Visit(binaryExpression.Right);
        }
      }
      else if (memberType is MemberType.Entity or MemberType.Structure) {
        if (binaryExpression.NodeType == ExpressionType.Coalesce) {
          if ((context.Evaluator.CanBeEvaluated(binaryExpression.Right) && !(binaryExpression.Right is ConstantExpression))
            || (context.Evaluator.CanBeEvaluated(binaryExpression.Left) && !(binaryExpression.Left is ConstantExpression)))
            throw new NotSupportedException(
              string.Format(Strings.ExXExpressionsWithConstantValuesOfYTypeNotSupported, "Coalesce", memberType.ToString()));

          return Visit(Expression.Condition(
            Expression.NotEqual(binaryExpression.Left, Expression.Constant(null)),
            binaryExpression.Left,
            binaryExpression.Right));
        }
        else {
          left = Visit(binaryExpression.Left);
          right = Visit(binaryExpression.Right);
        }
      }
      else if (EnumRewritableOperations(binaryExpression)) {
        // Following two checks for enums are here to improve result query
        // performance because they let not to cast columns to integer.
        var leftNoCasts = binaryExpression.Left.StripCasts();
        var leftNoCastsType = leftNoCasts.Type;
        var bareLeftType = leftNoCastsType.StripNullable();
        var rightNoCasts = binaryExpression.Right.StripCasts();
        var rightNoCastsType = rightNoCasts.Type;
        var bareRightType = rightNoCastsType.StripNullable();

        if (bareLeftType.IsEnum && rightNoCasts.NodeType is ExpressionType.Constant) {
          var typeToCast = leftNoCastsType.IsNullable()
            ? bareLeftType.GetEnumUnderlyingType().ToNullable()
            : leftNoCastsType.GetEnumUnderlyingType();
          left = Visit(Expression.Convert(leftNoCasts, typeToCast));
          right = Visit(Expression.Convert(binaryExpression.Right, typeToCast));
        }
        else if (bareRightType.IsEnum && leftNoCasts.NodeType is ExpressionType.Constant) {
          var typeToCast = rightNoCastsType.IsNullable()
            ? bareRightType.GetEnumUnderlyingType().ToNullable()
            : rightNoCastsType.GetEnumUnderlyingType();
          left = Visit(Expression.Convert(rightNoCasts, typeToCast));
          right = Visit(Expression.Convert(binaryExpression.Left, typeToCast));
        }
        else if (State.NonVisitableExpressions.Contains(leftNoCasts)
           || State.NonVisitableExpressions.Contains(rightNoCasts)) {
          left = binaryExpression.Left;
          right = binaryExpression.Right;
        }
        else {
          left = Visit(binaryExpression.Left);
          right = Visit(binaryExpression.Right);
        }
      }
      else {
        left = Visit(binaryExpression.Left);
        right = Visit(binaryExpression.Right);
      }
      var resultBinaryExpression = Expression.MakeBinary(binaryExpression.NodeType,
        left,
        right,
        binaryExpression.IsLiftedToNull,
        binaryExpression.Method);

      if (binaryExpression.NodeType is ExpressionType.Equal or ExpressionType.NotEqual)
        return VisitBinaryRecursive(resultBinaryExpression, binaryExpression);

      if (binaryExpression.NodeType is ExpressionType.ArrayIndex) {
        if (left.StripCasts() is NewArrayExpression newArrayExpression
            && right.StripCasts() is ConstantExpression indexExpression
            && indexExpression.Type == WellKnownTypes.Int32) {
          return newArrayExpression.Expressions[(int) indexExpression.Value];
        }

        throw new NotSupportedException(String.Format(Strings.ExBinaryExpressionXOfTypeXIsNotSupported, binaryExpression.ToString(true), binaryExpression.NodeType));
      }

      return resultBinaryExpression;

      static bool EnumRewritableOperations(BinaryExpression b)
      {
        var nt = b.NodeType;
        return nt is ExpressionType.Equal or ExpressionType.NotEqual
          or ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual
          or ExpressionType.LessThan or ExpressionType.LessThanOrEqual;
      }
    }

    protected override Expression VisitConditional(ConditionalExpression c)
    {
      var memberType = c.IfTrue.Type == WellKnownTypes.Object
        ? c.IfFalse.GetMemberType()
        : c.IfTrue.GetMemberType();
      if (memberType == MemberType.Entity || memberType == MemberType.Structure) {
        if ((context.Evaluator.CanBeEvaluated(c.IfFalse) && (c.IfFalse is not ConstantExpression))
          || (context.Evaluator.CanBeEvaluated(c.IfTrue) && (c.IfTrue is not ConstantExpression)))
          throw new NotSupportedException(string.Format(Strings.ExXExpressionsWithConstantValuesOfYTypeNotSupported, "Conditional", memberType.ToString()));
      }
      return base.VisitConditional(c);
    }

    private Expression ConvertEnum(Expression left)
    {
      var underlyingType = left.Type.IsNullable()
        ? Enum.GetUnderlyingType(left.Type.GetGenericArguments()[0]).ToNullable()
        : Enum.GetUnderlyingType(left.Type);

      left = left.NodeType is ExpressionType.Convert
        ? Expression.Convert(((UnaryExpression) left).Operand, underlyingType)
        : Expression.Convert(left, underlyingType);
      return left;
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected override Expression VisitParameter(ParameterExpression p)
    {
      bool isInnerParameter = State.Parameters.Contains(p);
      bool isOuterParameter = State.OuterParameters.Contains(p);

      if (!isInnerParameter && !isOuterParameter)
        throw new InvalidOperationException(Strings.ExLambdaParameterIsOutOfScope);
      ItemProjectorExpression itemProjector = context.Bindings[p].ItemProjector;
      if (isOuterParameter)
        return context.GetBoundItemProjector(p, itemProjector).Item;
      return itemProjector.Item;
    }

    protected override Expression VisitMemberAccess(MemberExpression ma)
    {
      var memberInfo = ma.Member;
      var sourceExpression = ma.Expression;

      if (sourceExpression != null) {
        if (sourceExpression.Type != memberInfo.ReflectedType
          && memberInfo is PropertyInfo
          && !memberInfo.ReflectedType.IsInterface) {
          ma = Expression.MakeMemberAccess(
            sourceExpression, sourceExpression.Type.GetProperty(memberInfo.Name, memberInfo.GetBindingFlags()));

          memberInfo = ma.Member;
          sourceExpression = ma.Expression;
        }
      }

      var customCompiler = context.CustomCompilerProvider.GetCompiler(memberInfo);

      // Reflected type doesn't have custom compiler defined, so falling back to base class compiler
      var declaringType = memberInfo.DeclaringType;
      var reflectedType = memberInfo.ReflectedType;
      if (customCompiler == null && declaringType != reflectedType && declaringType.IsAssignableFrom(reflectedType)) {
        var root = declaringType;
        var current = reflectedType;
        while (current != root && customCompiler == null) {
          current = current.BaseType;
          var member = current.GetProperty(memberInfo.Name, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
          customCompiler = context.CustomCompilerProvider.GetCompiler(member);
        }
      }

      if (customCompiler != null) {
        var expression = customCompiler.Invoke(sourceExpression, Array.Empty<Expression>());
        if (expression == null) {
          if (reflectedType.IsInterface)
            return Visit(BuildInterfaceExpression(ma));
          if (reflectedType.IsClass)
            return Visit(BuildHierarchyExpression(ma));
        }
        else
          return Visit(expression);
      }

      if (context.Evaluator.CanBeEvaluated(ma) && context.ParameterExtractor.IsParameter(ma)) {
        if (WellKnownInterfaces.Queryable.IsAssignableFrom(ma.Type)) {
          var lambda = FastExpression.Lambda<Func<IQueryable>>(ma).CachingCompile();
          var rootPoint = lambda();
          if (rootPoint != null)
            return base.Visit(rootPoint.Expression);
        }
        return ma;
      }
      if (ma.Expression == null) {
        if (WellKnownInterfaces.Queryable.IsAssignableFrom(ma.Type)) {
          var lambda = FastExpression.Lambda<Func<IQueryable>>(ma).CachingCompile();
          var rootPoint = lambda();
          if (rootPoint != null)
            return VisitSequence(rootPoint.Expression);
        }
      }
      else if (sourceExpression.NodeType == ExpressionType.Constant) {
        if (memberInfo is FieldInfo rfi && (rfi.FieldType.IsGenericType && WellKnownInterfaces.Queryable.IsAssignableFrom(rfi.FieldType))) {
          var lambda = FastExpression.Lambda<Func<IQueryable>>(ma).CachingCompile();
          var rootPoint = lambda();
          if (rootPoint != null)
            return VisitSequence(rootPoint.Expression);
        }
      }
      else if (sourceExpression.GetMemberType() == MemberType.Entity && memberInfo.Name != WellKnown.KeyFieldName) {
        var type = sourceExpression.Type;
        if (sourceExpression is ParameterExpression parameter) {
          var projection = context.Bindings[parameter];
          type = projection.ItemProjector.Item.Type;
        }
        if (!context.Model.Types[type].Fields.Contains(context.Domain.Handlers.NameBuilder.BuildFieldName((PropertyInfo) memberInfo))) {
          throw new NotSupportedException(string.Format(Strings.ExFieldMustBePersistent, ma.ToString(true)));
        }
      }
      Expression source;
      using (CreateScope(new TranslatorState(State) { /* BuildingProjection = false */ })) {
        source = Visit(sourceExpression);
      }

      var result = context.CheckIfQueryReusePossible(memberInfo)
        ? GetMemberWithRemap(source, memberInfo, ma)
        : GetMember(source, memberInfo, ma);

      return result ?? base.VisitMemberAccess(ma);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      using (CreateScope(new TranslatorState(State) { IsTailMethod = mc == context.Query && mc.IsQuery() })) {
        var method = mc.Method;
        var customCompiler = context.CustomCompilerProvider.GetCompiler(method);
        if (customCompiler != null) {
          return Visit(customCompiler.Invoke(mc.Object, mc.Arguments.ToArray()));
        }

        var methodDeclaringType = method.DeclaringType;
        var methodName = method.Name;

        // Visit Query. Deprecated.
#pragma warning disable 612,618
        if (methodDeclaringType == WellKnownOrmTypes.Query) {
          // Query.All<T>
          if (method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.All)) {
            return ConstructQueryable(mc);
          }

          // Query.FreeText<T>
          if (method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.FreeTextString)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.FreeTextExpression)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.FreeTextExpressionTopNByRank)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.FreeTextStringTopNByRank)) {
            return ConstructFreeTextQueryRoot(method.GetGenericArguments()[0], mc.Arguments);
          }

          // Query.ContainsTable<T>
          if (method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.ContainsTableExpr)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.ContainsTableExprWithColumns)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.ContainsTableExprTopNByRank)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.ContainsTableExprWithColumnsTopNByRank)) {
            return ConstructContainsTableQueryRoot(method.GetGenericArguments()[0], mc.Arguments);
          }

          // Query.Single<T> & Query.SingleOrDefault<T>
          if (method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.SingleKey)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.Query.SingleOrDefaultKey)) {
            return VisitQuerySingle(mc);
          }

          throw new InvalidOperationException(string.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
        }
        // Visit QueryEndpoint.
        if (methodDeclaringType == WellKnownOrmTypes.QueryEndpoint) {
          // Query.All<T>
          if (method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.All)) {
            return ConstructQueryable(mc);
          }

          // Query.FreeText<T>
          if (method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.FreeTextString)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.FreeTextExpression)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.FreeTextExpressionTopNByRank)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.FreeTextStringTopNByRank)) {
            return ConstructFreeTextQueryRoot(method.GetGenericArguments()[0], mc.Arguments);
          }

          // Query.ContainsTable<T>
          if (method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.ContainsTableExpr)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.ContainsTableExprWithColumns)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.ContainsTableExprTopNByRank)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.ContainsTableExprWithColumnsTopNByRank)) {
            return ConstructContainsTableQueryRoot(method.GetGenericArguments()[0], mc.Arguments);
          }

          // Query.Single<T> & Query.SingleOrDefault<T>
          if (method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.SingleKey)
            || method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.SingleOrDefaultKey)) {
            return VisitQuerySingle(mc);
          }

          if (method.IsGenericMethodSpecificationOf(WellKnownMembers.QueryEndpoint.Items)) {
            return VisitSequence(mc.Arguments[0].StripQuotes().Body, mc);
          }

          throw new InvalidOperationException(string.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
        }
#pragma warning restore 612,618

        // Visit Queryable extensions.
        if (methodDeclaringType == typeof(QueryableExtensions)) {
          return methodName switch {
            Reflection.WellKnown.QueryableExtensions.LeftJoin => VisitLeftJoin(mc),
            Reflection.WellKnown.QueryableExtensions.In => VisitIn(mc),
            Reflection.WellKnown.QueryableExtensions.Lock => VisitLock(mc),
            Reflection.WellKnown.QueryableExtensions.Take => VisitTake(mc.Arguments[0], mc.Arguments[1]),
            Reflection.WellKnown.QueryableExtensions.Skip => VisitSkip(mc.Arguments[0], mc.Arguments[1]),
            Reflection.WellKnown.QueryableExtensions.ElementAt => VisitElementAt(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), method.ReturnType, false),
            Reflection.WellKnown.QueryableExtensions.ElementAtOrDefault => VisitElementAt(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), method.ReturnType, true),
            Reflection.WellKnown.QueryableExtensions.Count => VisitAggregate(mc.Arguments[0], method, null, context.IsRoot(mc), mc),
            Reflection.WellKnown.QueryableExtensions.Tag => VisitTag(mc),
            _ => throw new InvalidOperationException(string.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)))
          };
        }
        // Visit Collection extensions
        if (methodDeclaringType == typeof(CollectionExtensionsEx)) {
          switch (methodName) {
            case Reflection.WellKnown.CollectionExtensions.ContainsAny:
              return VisitContainsAny(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), method.GetGenericArguments()[0]);
            case Reflection.WellKnown.CollectionExtensions.ContainsAll:
              return VisitContainsAll(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), method.GetGenericArguments()[0]);
            case Reflection.WellKnown.CollectionExtensions.ContainsNone:
              return VisitContainsNone(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), method.GetGenericArguments()[0]);
          }
        }


        // Process local collections
        if (mc.Object.IsLocalCollection(context)) {
          // IList.Contains
          // List.Contains
          // Array.Contains
          var parameters = method.GetParameters();
          if (methodName == nameof(ICollection<int>.Contains) && parameters.Length == 1)
            return VisitContains(mc.Object, mc.Arguments[0], false);
        }

        var result = base.VisitMethodCall(mc);
        if (result != mc && result.NodeType == ExpressionType.Call) {
          var visitedMethodCall = (MethodCallExpression) result;
          if (visitedMethodCall.Arguments.Any(arg => arg.IsProjection()))
            throw new InvalidOperationException(string.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
        }
        return result;
      }
    }

    private Expression ConstructFreeTextQueryRoot(Type elementType, System.Collections.ObjectModel.ReadOnlyCollection<Expression> expressions)
    {
      TypeInfo type;
      if (!context.Model.Types.TryGetValue(elementType, out type))
        throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));
      var fullTextIndex = type.FullTextIndex;
      if (fullTextIndex == null)
        throw new InvalidOperationException(String.Format(Strings.ExEntityDoesNotHaveFullTextIndex, elementType.FullName));
      var searchCriteria = expressions[0];
      if (compiledQueryScope != null
          && searchCriteria.NodeType == ExpressionType.Constant
          && searchCriteria.Type == WellKnownTypes.String)
        throw new InvalidOperationException(String.Format(Strings.ExFreeTextNotSupportedInCompiledQueries, ((ConstantExpression) searchCriteria).Value));

      // Prepare parameter
      Func<ParameterContext, string> compiledParameter;
      if (searchCriteria.NodeType == ExpressionType.Quote)
        searchCriteria = searchCriteria.StripQuotes();
      if (searchCriteria.Type == typeof(Func<string>)) {
        if (compiledQueryScope == null) {
          var originalSearchCriteria = (Expression<Func<string>>) searchCriteria;
          var body = originalSearchCriteria.Body;
          var searchCriteriaLambda = FastExpression.Lambda<Func<ParameterContext, string>>(body, ParameterContextParam);
          compiledParameter = searchCriteriaLambda.CachingCompile();
        }
        else {
          var replacer = compiledQueryScope.QueryParameterReplacer;
          var newSearchCriteria = (Expression<Func<string>>) replacer.Replace(searchCriteria);
          var searchCriteriaAccessor = ParameterAccessorFactory.CreateAccessorExpression<string>(newSearchCriteria.Body);
          compiledParameter = searchCriteriaAccessor.CachingCompile();
        }
      }
      else {
        var parameter = ParameterAccessorFactory.CreateAccessorExpression<string>(searchCriteria);
        compiledParameter = parameter.CachingCompile();
      }

      ColumnExpression rankExpression;
      FullTextExpression freeTextExpression;
      ItemProjectorExpression itemProjector;
      var fullFeatured = context.ProviderInfo.Supports(ProviderFeatures.FullFeaturedFullText);
      var entityExpression = EntityExpression.Create(type, 0, !fullFeatured);
      var rankColumnAlias = context.GetNextColumnAlias();

      FreeTextProvider dataSource;
      if (expressions.Count > 1) {
        var topNParameter = ParameterAccessorFactory.CreateAccessorExpression<int>(expressions[1]).CachingCompile();
        dataSource = new FreeTextProvider(fullTextIndex, compiledParameter, rankColumnAlias, topNParameter, fullFeatured);
      }
      else
        dataSource = new FreeTextProvider(fullTextIndex, compiledParameter, rankColumnAlias, fullFeatured);

      rankExpression = ColumnExpression.Create(WellKnownTypes.Double, dataSource.Header.Columns.Count - 1);
      freeTextExpression = new FullTextExpression(fullTextIndex, entityExpression, rankExpression, null);
      itemProjector = new ItemProjectorExpression(freeTextExpression, dataSource, context);
      return new ProjectionExpression(WellKnownInterfaces.QueryableOfT.CachedMakeGenericType(elementType), itemProjector, EmptyTupleParameterBindings);
    }

    private Expression ConstructContainsTableQueryRoot(Type elementType, System.Collections.ObjectModel.ReadOnlyCollection<Expression> parameters)
    {
      TypeInfo type;
      if (!context.Model.Types.TryGetValue(elementType, out type))
        throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));
      var fullTextIndex = type.FullTextIndex;
      if (fullTextIndex == null)
        throw new InvalidOperationException(String.Format(Strings.ExEntityDoesNotHaveFullTextIndex, elementType.FullName));
      if (!context.ProviderInfo.Supports(ProviderFeatures.SingleKeyRankTableFullText))
        throw new NotSupportedException(Strings.ExCurrentProviderDoesNotSupportContainsTableFunctionality);

      Expression rawSearchCriteria;
      Expression searchColumns;
      Expression topNByRank;
      ArrangeContainsTableParameters(parameters, out rawSearchCriteria, out searchColumns, out topNByRank);
      IList<ColumnInfo> searchableColumns = (searchColumns != null)
        ? GetColumnsToSearch((ConstantExpression) searchColumns, elementType, type)
        : new List<ColumnInfo>();

      // Prepare parameters
      Func<ParameterContext, string> compiledParameter;
      if (rawSearchCriteria.NodeType == ExpressionType.Quote)
        rawSearchCriteria = rawSearchCriteria.StripQuotes();
      if (rawSearchCriteria.Type != typeof(Func<ConditionEndpoint, IOperand>))
        throw new InvalidOperationException(string.Format(Strings.ExUnsupportedExpressionType));

      var func = ((Expression<Func<ConditionEndpoint, IOperand>>) rawSearchCriteria).Compile();
      var conditionCompiler = context.Domain.Handler.GetSearchConditionCompiler();
      func.Invoke(SearchConditionNodeFactory.CreateConditonRoot()).AcceptVisitor(conditionCompiler);

      var preparedSearchCriteria = FastExpression.Lambda<Func<ParameterContext, string>>(
        Expression.Constant(conditionCompiler.CurrentOutput), ParameterContextParam);

      if (compiledQueryScope == null) {
        compiledParameter = preparedSearchCriteria.CachingCompile();
      }
      else {
        var replacer = compiledQueryScope.QueryParameterReplacer;
        var newSearchCriteria = replacer.Replace(preparedSearchCriteria);
        compiledParameter = ((Expression<Func<ParameterContext, string>>) newSearchCriteria).CachingCompile();
      }

      ColumnExpression rankExpression;
      FullTextExpression freeTextExpression;
      ItemProjectorExpression itemProjector;

      //var fullFeatured = context.ProviderInfo.Supports(ProviderFeatures.FullFeaturedFullText);
      var fullFeatured = false;// postgre provider has no analogue functionality by now.
      var entityExpression = EntityExpression.Create(type, 0, !fullFeatured);
      var rankColumnAlias = context.GetNextColumnAlias();

      ContainsTableProvider dataSource;
      if (topNByRank != null) {
        var topNParameter = ParameterAccessorFactory.CreateAccessorExpression<int>(parameters[1]).CachingCompile();
        dataSource = new ContainsTableProvider(fullTextIndex, compiledParameter, rankColumnAlias, searchableColumns, topNParameter, fullFeatured);
      }
      else
        dataSource = new ContainsTableProvider(fullTextIndex, compiledParameter, rankColumnAlias, searchableColumns, fullFeatured);

      rankExpression = ColumnExpression.Create(WellKnownTypes.Double, dataSource.Header.Columns.Count - 1);
      freeTextExpression = new FullTextExpression(fullTextIndex, entityExpression, rankExpression, null);
      itemProjector = new ItemProjectorExpression(freeTextExpression, dataSource, context);
      return new ProjectionExpression(WellKnownInterfaces.QueryableOfT.CachedMakeGenericType(elementType), itemProjector, EmptyTupleParameterBindings);
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected override Expression VisitNew(NewExpression newExpression)
    {
      // ReSharper disable HeuristicUnreachableCode
      // ReSharper disable ConditionIsAlwaysTrueOrFalse

      if (newExpression.Members==null) {
        if (newExpression.IsGroupingExpression()
          || newExpression.IsSubqueryExpression()
          || newExpression.IsNewExpressionSupportedByStorage())
          return base.VisitNew(newExpression);
      }

      // ReSharper restore ConditionIsAlwaysTrueOrFalse
      // ReSharper restore HeuristicUnreachableCode

      var arguments = VisitNewExpressionArguments(newExpression);
      if (newExpression.IsAnonymousConstructor()) {
        return newExpression.Members==null
          ? Expression.New(newExpression.Constructor, arguments)
          : Expression.New(newExpression.Constructor, arguments, newExpression.Members);
      }

      var constructorParameters = newExpression.GetConstructorParameters();
      if (constructorParameters.Length != arguments.Count)
        throw Exceptions.InternalError(Strings.ExInvalidNumberOfParametersInNewExpression, OrmLog.Instance);

      var bindings = GetBindingsForConstructor(constructorParameters, arguments, newExpression);
      var nativeBindings = new Dictionary<MemberInfo, Expression>();
      return new ConstructorExpression(newExpression.Type, bindings, nativeBindings, newExpression.Constructor, arguments);
    }

    internal static bool FilterBindings(MemberInfo mi, string name, Type type)
    {
      var result = string.Equals(mi.Name, name, StringComparison.InvariantCultureIgnoreCase);
      if (!result)
        return false;

      result = mi.MemberType == MemberTypes.Field || mi.MemberType == MemberTypes.Property;
      if (!result)
        return false;

      var field = mi as FieldInfo;
      if (field != null)
        return field.FieldType == type && !field.IsInitOnly;
      var property = mi as PropertyInfo;
      if (property == null)
        return false;
      return property.PropertyType.IsAssignableFrom(type) && property.CanWrite;
    }

    #region Private helper methods

    private Dictionary<MemberInfo, Expression> GetBindingsForConstructor(
      ParameterInfo[] constructorParameters, IList<Expression> constructorArguments, Expression newExpression)
    {
      var bindings = new Dictionary<MemberInfo, Expression>();
      var duplicateMembers = new HashSet<MemberInfo>();
      var typeMembers = newExpression.Type.GetMembers();
      for (var parameterIndex = 0; parameterIndex < constructorParameters.Length; parameterIndex++) {
        var constructorParameter = constructorParameters[parameterIndex];
        var members = typeMembers
          .Where(mi => FilterBindings(mi, constructorParameter.Name, constructorParameter.ParameterType))
          .ToList();
        if (members.Count != 1 || duplicateMembers.Contains(members[0]))
          continue;
        if (bindings.Remove(members[0])) {
          _ = duplicateMembers.Add(members[0]);
        }
        else
          bindings.Add(members[0], constructorArguments[parameterIndex]);
      }
      return bindings;
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private Expression VisitBinaryRecursive(BinaryExpression binaryExpression, BinaryExpression originalBinaryExpression)
    {
      if (context.Evaluator.CanBeEvaluated(binaryExpression))
        return context.ParameterExtractor.IsParameter(binaryExpression)
          ? (Expression) binaryExpression
          : ExpressionEvaluator.Evaluate(binaryExpression);

      Expression left = binaryExpression.Left.StripCasts().StripMarkers();
      Expression right = binaryExpression.Right.StripCasts().StripMarkers();

      var rightIsConstant = context.Evaluator.CanBeEvaluated(right);
      var leftIsConstant = context.Evaluator.CanBeEvaluated(left);
      bool leftOrRightIsIndex = false;

      if (left is IndexExpression leftIndexExpression) {
        left = VisitIndex(leftIndexExpression);
        leftOrRightIsIndex = true;
      }
      if (right is IndexExpression rightIndexExpression) {
        right = VisitIndex(rightIndexExpression);
        leftOrRightIsIndex = true;
      }

      IList<Expression> leftExpressions;
      IList<Expression> rightExpressions;

      // Split left and right arguments to subexpressions.
      MemberType memberType = left.Type == WellKnownTypes.Object
        ? right.GetMemberType()
        : left.GetMemberType();
      switch (memberType) {
        case MemberType.EntitySet:
          if ((leftIsConstant && ExpressionEvaluator.Evaluate(left).Value == null)
            || left is ConstantExpression && ((ConstantExpression) left).Value == null)
            return SelectBoolConstantExpression(binaryExpression.NodeType == ExpressionType.NotEqual);
          if ((rightIsConstant && ExpressionEvaluator.Evaluate(right).Value == null)
            || right is ConstantExpression && ((ConstantExpression) right).Value == null)
            return SelectBoolConstantExpression(binaryExpression.NodeType == ExpressionType.NotEqual);
          var leftEntitySetExpression = left as EntitySetExpression;
          var rightEntitySetExpression = right as EntitySetExpression;
          if (leftEntitySetExpression != null && rightEntitySetExpression != null) {
            if (leftEntitySetExpression.Field != rightEntitySetExpression.Field) {
              return FalseExpression;
            }
            var binary = Expression.MakeBinary(binaryExpression.NodeType,
              (Expression) leftEntitySetExpression.Owner,
              (Expression) rightEntitySetExpression.Owner,
              binaryExpression.IsLiftedToNull,
              binaryExpression.Method);
            return VisitBinaryRecursive(binary, originalBinaryExpression);
          }
          if (rightEntitySetExpression != null && left is ConstantExpression) {
            var leftEntitySet = (EntitySetBase) ((ConstantExpression) left).Value;
            var binary = Expression.MakeBinary(binaryExpression.NodeType,
              Expression.Constant(leftEntitySet.Owner),
              (Expression) rightEntitySetExpression.Owner,
              binaryExpression.IsLiftedToNull,
              binaryExpression.Method);
            return VisitBinaryRecursive(binary, originalBinaryExpression);
          }
          if (leftEntitySetExpression != null && right is ConstantExpression) {
            var rightEntitySet = (EntitySetBase) ((ConstantExpression) right).Value;
            var binary = Expression.MakeBinary(binaryExpression.NodeType,
              (Expression) leftEntitySetExpression.Owner,
              Expression.Constant(rightEntitySet.Owner),
              binaryExpression.IsLiftedToNull,
              binaryExpression.Method);
            return VisitBinaryRecursive(binary, originalBinaryExpression);
          }
          return binaryExpression;
        case MemberType.Key:
          var leftKeyExpression = left as KeyExpression;
          var rightKeyExpression = right as KeyExpression;
          if (leftKeyExpression == null && rightKeyExpression == null)
            throw new InvalidOperationException(String.Format(Strings.ExBothLeftAndRightPartOfBinaryExpressionXAreNULLOrNotKeyExpression, originalBinaryExpression.ToString(true)));
          // Check key compatibility
          leftKeyExpression.EnsureKeyExpressionCompatible(rightKeyExpression, originalBinaryExpression);
          // Key split to it's fields.
          IReadOnlyList<FieldExpression> keyFields = (leftKeyExpression ?? rightKeyExpression)
            .KeyFields;
          leftExpressions = GetKeyFields(left, keyFields);
          rightExpressions = GetKeyFields(right, keyFields);
          break;
        case MemberType.Entity:
          // Entity split to key fields.
          var leftEntityExpression = (Expression) (left as EntityExpression) ?? left as EntityFieldExpression;
          var rightEntityExpression = (Expression) (right as EntityExpression) ?? right as EntityFieldExpression;
          if (leftEntityExpression == null && rightEntityExpression == null)
            if (!IsConditionalOrWellknown(left) && !IsConditionalOrWellknown(right))
              throw new NotSupportedException(
                String.Format(
                  Strings.ExBothLeftAndRightPartOfBinaryExpressionXAreNULLOrNotEntityExpressionEntityFieldExpression,
                  binaryExpression));
          var type = left.Type == WellKnownTypes.Object
             ? right.Type
             : left.Type;

          var keyFieldTypes = context
            .Model
            .Types[type]
            .Key
            .TupleDescriptor;

          leftExpressions = GetEntityFields(left, keyFieldTypes);
          rightExpressions = GetEntityFields(right, keyFieldTypes);
          break;
        case MemberType.Anonymous:
          // Anonymous type split to constructor arguments.
          var anonymousType = (left.Type == WellKnownTypes.Object)
            ? right.Type
            : left.Type;
          leftExpressions = GetAnonymousArguments(left, anonymousType);
          rightExpressions = GetAnonymousArguments(right, anonymousType);
          break;
        case MemberType.Structure:
          if ((leftIsConstant && ExpressionEvaluator.Evaluate(left).Value == null)
            || left is ConstantExpression && ((ConstantExpression) left).Value == null)
            return SelectBoolConstantExpression(binaryExpression.NodeType == ExpressionType.NotEqual);
          if ((rightIsConstant && ExpressionEvaluator.Evaluate(right).Value == null)
            || right is ConstantExpression && ((ConstantExpression) right).Value == null)
            return SelectBoolConstantExpression(binaryExpression.NodeType == ExpressionType.NotEqual);
          // Structure split to it's fields.
          var leftStructureExpression = left as StructureFieldExpression;
          var rightStructureExpression = right as StructureFieldExpression;
          if (leftStructureExpression == null && rightStructureExpression == null)
            throw new NotSupportedException(String.Format(Strings.ExBothLeftAndRightPartOfBinaryExpressionXAreNULLOrNotStructureExpression, binaryExpression));

          StructureFieldExpression structureFieldExpression = (leftStructureExpression ?? rightStructureExpression);
          leftExpressions = GetStructureFields(left, structureFieldExpression.Fields, structureFieldExpression.Type);
          rightExpressions = GetStructureFields(right, structureFieldExpression.Fields, structureFieldExpression.Type);
          break;
        case MemberType.Array:
          // Special case. ArrayIndex expression.
          if (binaryExpression.NodeType == ExpressionType.ArrayIndex) {
            var arrayExpression = Visit(left);
            var arrayIndex = Visit(right);
            return Expression.ArrayIndex(arrayExpression, arrayIndex);
          }

          // If array compares to null use standard routine.
          if ((rightIsConstant && ExpressionEvaluator.Evaluate(right).Value == null)
            || (rightIsConstant && ExpressionEvaluator.Evaluate(right).Value == null)
              || (right.Type == WellKnownTypes.ByteArray && (left is FieldExpression || left is ColumnExpression || right is FieldExpression || right is ColumnExpression)))
            return Expression.MakeBinary(binaryExpression.NodeType,
              left,
              right,
              binaryExpression.IsLiftedToNull,
              binaryExpression.Method);


          // Array split to it's members.
          leftExpressions = ((NewArrayExpression) left).Expressions;
          rightExpressions = ((NewArrayExpression) right).Expressions;
          break;
        default:
          // Primitive types don't has subexpressions. Use standart routine.
          if (leftOrRightIsIndex) {
            var binary = Expression.MakeBinary(binaryExpression.NodeType, left, right, binaryExpression.IsLiftedToNull, binaryExpression.Method);
            return VisitBinaryRecursive(binary, binaryExpression);
          }
          return binaryExpression;
      }

      if (leftExpressions.Count != rightExpressions.Count || leftExpressions.Count == 0)
        throw Exceptions.InternalError(Strings.ExMistmatchCountOfLeftAndRightExpressions, OrmLog.Instance);

      // Combine new binary expression from subexpression pairs.
      Expression resultExpression = null;
      for (var i = 0; i < leftExpressions.Count; i++) {
        BinaryExpression pairExpression;
        var leftItem = leftExpressions[i];
        var rightItem = rightExpressions[i];
        var leftIsNullable = leftItem.Type.IsNullable();
        var rightIsNullable = rightItem.Type.IsNullable();
        leftItem = leftIsNullable
          ? leftItem
          : leftItem.LiftToNullable();
        rightItem = rightIsNullable
          ? rightItem
          : rightItem.LiftToNullable();

        switch (binaryExpression.NodeType) {
          case ExpressionType.Equal:
            pairExpression = Expression.Equal(leftItem, rightItem);
            break;
          case ExpressionType.NotEqual:
            pairExpression = Expression.NotEqual(leftItem, rightItem);
            break;
          default:
            throw new NotSupportedException(String.Format(Strings.ExBinaryExpressionsWithNodeTypeXAreNotSupported,
              binaryExpression.NodeType));
        }

        // visit new expression recursively
        var visitedResultExpression = VisitBinaryRecursive(pairExpression, originalBinaryExpression);

        // Combine expression chain with AndAlso
        resultExpression = resultExpression == null
          ? visitedResultExpression
          : Expression.AndAlso(resultExpression, visitedResultExpression);
      }

      // Return result.
      return resultExpression;
    }

    private static ConstantExpression SelectBoolConstantExpression(bool b) =>
      b ? TrueExpression : FalseExpression;

    private Expression VisitIndex(IndexExpression ie)
    {
      var objectExpression = Visit(ie.Object).StripCasts();
      var argument = Visit(ie.Arguments[0]);
      var evaluatedArgument = (string) ExpressionEvaluator.Evaluate(argument).Value;
      var entityExpression = objectExpression as EntityExpression;
      if (entityExpression != null)
        return entityExpression.Fields.First(field => field.Name == evaluatedArgument);

      var structureExpression = objectExpression as StructureExpression;
      if (structureExpression != null)
        return structureExpression.Fields.First(field => field.Name == evaluatedArgument);

      var entityFieldExpression = objectExpression as EntityFieldExpression;
      if (entityFieldExpression != null)
        return entityFieldExpression.Fields.First(field => field.Name == evaluatedArgument);

      var structureFieldExpression = objectExpression as StructureFieldExpression;
      if (structureFieldExpression != null)
        return structureFieldExpression.Fields.First(field => field.Name == evaluatedArgument);

      var typeInfo = context.Model.Types[objectExpression.Type];
      var parameterExpression = objectExpression as ParameterExpression;
      if (objectExpression is ParameterExpression || objectExpression is ConstantExpression) {
        if (typeInfo.IsEntity) {
          entityExpression = EntityExpression.Create(typeInfo, 0, false);
          return entityExpression.Fields.First(field => field.Name == evaluatedArgument);
        }
        if (typeInfo.IsStructure) {
          structureExpression = StructureExpression.CreateLocalCollectionStructure(typeInfo, new Segment<int>(0, typeInfo.TupleDescriptor.Count));
          return structureExpression.Fields.First(field => field.Name == evaluatedArgument);
        }
      }
      var fieldInfo = typeInfo.Fields[evaluatedArgument];
      return Expression.Convert(
        Expression.Call(
          objectExpression,
          objectExpression.Type.GetProperty(Reflection.WellKnown.IndexerPropertyName).GetGetMethod(),
          new[] {Expression.Constant(evaluatedArgument)}),
        fieldInfo.ValueType);
    }

    private static bool IsConditionalOrWellknown(Expression expression)
    {
      return IsConditionalOrWellknownRecursive(expression, true);

      static bool IsConditionalOrWellknownRecursive(Expression expression, bool isRootCall)
      {
        var conditionalExpression = expression as ConditionalExpression;
        if (conditionalExpression != null)
          return IsConditionalOrWellknownRecursive(conditionalExpression.IfTrue, false)
            && IsConditionalOrWellknownRecursive(conditionalExpression.IfFalse, false);

        if (isRootCall)
          return false;

        if (expression.NodeType == ExpressionType.Constant)
          return true;

        if (expression.NodeType == ExpressionType.Convert) {
          var unary = (UnaryExpression) expression;
          return IsConditionalOrWellknownRecursive(unary.Operand, false);
        }

        if (!(expression is ExtendedExpression))
          return false;

        var memberType = expression.GetMemberType();
        switch (memberType) {
          case MemberType.Primitive:
          case MemberType.Key:
          case MemberType.Structure:
          case MemberType.Entity:
          case MemberType.EntitySet:
            return true;
          default:
            return false;
        }
      }
    }

    private IList<Expression> GetStructureFields(
      Expression expression,
      IEnumerable<PersistentFieldExpression> structureFields,
      Type structureType)
    {
      expression = expression.StripCasts();
      if (expression is IPersistentExpression persistentExpression) {
        return persistentExpression
          .Fields
          .Where(field => field.GetMemberType() == MemberType.Primitive)
          .Select(e => (Expression) e)
          .ToList();
      }

      ConstantExpression nullExpression = Expression.Constant(null, structureType);
      BinaryExpression isNullExpression = Expression.Equal(expression, nullExpression);

      var result = new List<Expression>();
      foreach (PersistentFieldExpression fieldExpression in structureFields) {
        if (!structureType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Contains(fieldExpression.UnderlyingProperty)) {
          if (!context.Model.Types[structureType].Fields[fieldExpression.Name].IsDynamicallyDefined) {
            continue;
          }
        }
        Type nullableType = fieldExpression.Type.ToNullable();
        Expression propertyAccessorExpression;
        if (fieldExpression.UnderlyingProperty != null) {
          propertyAccessorExpression = Expression.MakeMemberAccess(Expression.Convert(expression, structureType), fieldExpression.UnderlyingProperty);
        }
        else {
          var methodInfo = structureType.GetProperty(Reflection.WellKnown.IndexerPropertyName).GetGetMethod();
          propertyAccessorExpression = Expression.Call(Expression.Convert(expression, structureType), methodInfo, Expression.Constant(fieldExpression.Name));
        }
        var memberExpression = (Expression) Expression.Condition(
          isNullExpression,
          Expression.Constant(null, nullableType),
          Expression.Convert(
            propertyAccessorExpression,
            nullableType));

        switch (fieldExpression.GetMemberType()) {
          case MemberType.Entity:
            var keyFieldTypes = context
              .Model
              .Types[fieldExpression.Type]
              .Key
              .TupleDescriptor;
            result.AddRange(GetEntityFields(memberExpression, keyFieldTypes));
            break;
          case MemberType.Structure:
            var structureFieldExpression = (StructureFieldExpression) fieldExpression;
            result.AddRange(GetStructureFields(memberExpression, structureFieldExpression.Fields, structureFieldExpression.Type));
            break;
          case MemberType.Primitive:
            result.Add(memberExpression);
            break;
          default:
            throw new NotSupportedException();
        }
      }
      return result;
    }

    private IList<Expression> GetEntityFields(Expression expression, IReadOnlyList<Type> keyFieldTypes)
    {
      expression = expression.StripCasts();
      if (expression is IEntityExpression iEntityExpression) {
        var keyFields = iEntityExpression.Key.KeyFields;
        return keyFields
          .Cast<Expression>()
          .ToArray(keyFields.Count);
      }
      if (expression.IsNull()) {
        return keyFieldTypes
          .Select(type => (Expression) Expression.Constant(null, type.ToNullable()))
          .ToArray(keyFieldTypes.Count);
      }
      if (IsConditionalOrWellknown(expression)) {
        return keyFieldTypes
          .Select((type, index) => GetConditionalKeyField(expression, type, index))
          .ToArray(keyFieldTypes.Count);
      }

      var nullEntityExpression = Expression.Constant(null, expression.Type);
      var isNullExpression = Expression.Equal(expression, nullEntityExpression);
      if (!WellKnownOrmInterfaces.Entity.IsAssignableFrom(expression.Type))
        expression = Expression.Convert(expression, WellKnownOrmInterfaces.Entity);

      var resultList = new Expression[keyFieldTypes.Count];
      for(int i = 0, count = keyFieldTypes.Count; i < count; i++) {
        var keyFieldType = keyFieldTypes[i];
        var baseType = keyFieldType.StripNullable();
        var fieldType = (baseType.IsEnum ? Enum.GetUnderlyingType(baseType) : baseType).ToNullable();
        var keyTupleExpression = Expression.MakeMemberAccess(
          Expression.MakeMemberAccess(expression, WellKnownMembers.IEntityKey), WellKnownMembers.Key.Value);
        var tupleAccess = (Expression) keyTupleExpression.MakeTupleAccess(fieldType, i);

        var entityNullCheck = Expression.Condition(
          isNullExpression,
          Expression.Constant(null, keyFieldType.ToNullable()),
          tupleAccess);
        resultList[i] = entityNullCheck;
        _ = State.NonVisitableExpressions.Add(entityNullCheck);
      }
      return resultList;
    }

    private static Expression GetConditionalKeyField(Expression expression, Type keyFieldType, int index)
    {
      if (expression is ConditionalExpression ce)
        return Expression.Condition(
          ce.Test,
          GetConditionalKeyField(ce.IfTrue, keyFieldType, index),
          GetConditionalKeyField(ce.IfFalse, keyFieldType, index));
      if (expression.IsNull())
        return Expression.Constant(null, keyFieldType.ToNullable());
      var ee = (IEntityExpression) expression.StripCasts();
      return ee.Key.KeyFields[index].LiftToNullable();
    }

    private IList<Expression> GetKeyFields(Expression expression, IReadOnlyList<FieldExpression> keyFields)
    {
      expression = expression.StripCasts();

      if (expression is KeyExpression keyExpression) {
        return keyExpression
          .KeyFields
          .Cast<Expression>()
          .ToArray(keyExpression.KeyFields.Count);
      }

      if (expression.IsNull())
        return keyFields
          .Select(f => f.Type)
          .Select(type => (Expression) Expression.Constant(null, type.ToNullable()))
          .ToArray(keyFields.Count);

      var nullExpression = Expression.Constant(null, expression.Type);
      var isNullExpression = Expression.Equal(expression, nullExpression);
      var keyTupleExpression = Expression.MakeMemberAccess(expression, WellKnownMembers.Key.Value);

      return keyFields
        .Select(f => f.Type)
        .Select((type, index) => {
          var resultType = type.ToNullable();
          var baseType = type.StripNullable();
          var fieldType = (baseType.IsEnum ? Enum.GetUnderlyingType(baseType) : baseType).ToNullable();
          var tupleAccess = (Expression) keyTupleExpression.MakeTupleAccess(fieldType, index);
          if (fieldType != resultType)
            tupleAccess = Expression.Convert(tupleAccess, resultType);
          var checkForNulls = (Expression) Expression.Condition(isNullExpression, Expression.Constant(null, resultType), tupleAccess);
          _ = State.NonVisitableExpressions.Add(checkForNulls);
          return checkForNulls;
        })
        .ToArray(keyFields.Count);
    }

    private Expression ProcessProjectionElement(Expression body)
    {
      var originalBodyType = body.Type;
      var reduceCastBody = body.StripCasts();

      var canCalculate =
        State.CalculateExpressions
          && reduceCastBody.GetMemberType() == MemberType.Unknown
          && (reduceCastBody.NodeType != ExpressionType.New || reduceCastBody.IsNewExpressionSupportedByStorage())
          && reduceCastBody.NodeType != ExpressionType.ArrayIndex
          && (ExtendedExpressionType) reduceCastBody.NodeType != ExtendedExpressionType.Constructor
          && !ContainsEntityExpression(reduceCastBody);

      if (!canCalculate)
        return body;

      var lambdaParameter = State.Parameters[0];
      var calculator = ExpressionMaterializer.MakeLambda(body, context);
      var columnDescriptor = CreateCalculatedColumnDescriptor(calculator);
      return AddCalculatedColumn(lambdaParameter, columnDescriptor, originalBodyType);
    }

    private CalculatedColumnDescriptor CreateCalculatedColumnDescriptor(LambdaExpression expression)
    {
      var columnType = expression.Body.Type;
      var body = EnumRewriter.Rewrite(expression.Body);
      if (columnType != WellKnownTypes.Object)
        body = Expression.Convert(body, WellKnownTypes.Object);
      var calculator = (Expression<Func<Tuple, object>>) FastExpression.Lambda(body, expression.Parameters);
      return new CalculatedColumnDescriptor(context.GetNextColumnAlias(), columnType, calculator);
    }

    private ColumnExpression AddCalculatedColumn(ParameterExpression sourceParameter, CalculatedColumnDescriptor descriptor, Type originalColumnType)
    {
      var oldResult = context.Bindings[sourceParameter];
      var isInlined = !State.BuildingProjection && !State.GroupingKey;
      var dataSource = oldResult.ItemProjector.DataSource;

      SortProvider sortProvider = null;
      if (dataSource is SortProvider sortProvider1) {
        sortProvider = sortProvider1;
        dataSource = sortProvider1.Source;
      }

      CalculatedColumnDescriptor[] columns;
      if (State.AllowCalculableColumnCombine && dataSource is CalculateProvider calculateProvider && isInlined==calculateProvider.IsInlined) {
        columns = calculateProvider
          .CalculatedColumns
          .Select(cc => new CalculatedColumnDescriptor(cc.Name, cc.Type, cc.Expression))
          .ToArray(calculateProvider.CalculatedColumns.Length + 1);
        columns[^1] = descriptor;
        dataSource = calculateProvider.Source;
      }
      else {
        columns = new[] { descriptor };
      }
      
      dataSource = dataSource.Calculate(isInlined, columns);

      if (sortProvider != null)
        dataSource = dataSource.OrderBy(sortProvider.Order);

      var newItemProjector = oldResult.ItemProjector.Remap(dataSource, 0);
      var newResult = new ProjectionExpression(oldResult.Type, newItemProjector, oldResult.TupleParameterBindings);
      context.Bindings.ReplaceBound(sourceParameter, newResult);

      var result = ColumnExpression.Create(originalColumnType, dataSource.Header.Length - 1);
      ModifyStateAllowCalculableColumnCombine(true);

      return result;
    }

    private static bool ContainsEntityExpression(Expression expression)
    {
      var found = false;
      var entityFinder = new ExtendedExpressionReplacer(e => {
        if ((int) e.NodeType == (int) ExtendedExpressionType.Entity) {
          found = true;
          return e;
        }
        return null;
      });
      entityFinder.Replace(expression);
      return found;
    }

    private Expression ConstructQueryable(MethodCallExpression mc)
    {
      var elementType = mc.Method.GetGenericArguments()[0];
      if (!context.Model.Types.TryGetValue(elementType, out var type))
        throw new InvalidOperationException(string.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));
      var index = type.Indexes.PrimaryIndex;
      var entityExpression = EntityExpression.Create(type, 0, false);
      var itemProjector = new ItemProjectorExpression(entityExpression, index.GetQuery(), context);
      return new ProjectionExpression(mc.Type, itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
    }

    private Expression ConstructQueryable(Type elementType)
    {
      if (!context.Model.Types.TryGetValue(elementType, out var type))
        throw new InvalidOperationException(string.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));
      var index = type.Indexes.PrimaryIndex;
      var entityExpression = EntityExpression.Create(type, 0, false);
      var itemProjector = new ItemProjectorExpression(entityExpression, index.GetQuery(), context);
      return new ProjectionExpression(WellKnownInterfaces.QueryableOfT.CachedMakeGenericType(elementType),
        itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
    }

    private Expression BuildSubqueryResult(ProjectionExpression subQuery, Type resultType)
    {
      if (State.Parameters.Length == 0)
        throw Exceptions.InternalError(String.Format(Strings.ExUnableToBuildSubqueryResultForExpressionXStateContainsNoParameters, subQuery), OrmLog.Instance);

      if (!resultType.IsOfGenericInterface(WellKnownInterfaces.EnumerableOfT))
        throw Exceptions.InternalError(String.Format(Strings.ExUnableToBuildSubqueryResultForExpressionXResultTypeIsNotIEnumerable, subQuery), OrmLog.Instance);

      ApplyParameter applyParameter = context.GetApplyParameter(context.Bindings[State.Parameters[0]]);
      if (subQuery.Type != resultType)
        subQuery = new ProjectionExpression(
          resultType,
          subQuery.ItemProjector,
          subQuery.TupleParameterBindings,
          subQuery.ResultAccessMethod);
      return new SubQueryExpression(resultType, State.Parameters[0], false, subQuery, applyParameter);
    }

    private static IList<Expression> GetAnonymousArguments(Expression expression, Type anonymousTypeForNullValues = null)
    {
      if (expression.NodeType == ExpressionType.New) {
        var newExpression = ((NewExpression) expression);
        IEnumerable<Expression> arguments = newExpression
          .Members
          .Select((methodInfo, index) => (methodInfo.Name, Argument: newExpression.Arguments[index]))
          .OrderBy(a => a.Name)
          .Select(a => a.Argument);
        return arguments.ToArray(newExpression.Members.Count);
      }

      if (expression.NodeType == ExpressionType.Constant) {
        var constantExpression = expression as ConstantExpression;
        if (constantExpression.Value == null && constantExpression.Type == WellKnownTypes.Object) {
          var newConstantExpressionType = anonymousTypeForNullValues ?? constantExpression.Type;
          constantExpression = Expression.Constant(null, newConstantExpressionType);
          var orderedProps1 = constantExpression
            .Type
            .GetProperties();
          Array.Sort(orderedProps1, CompareProps);

          return orderedProps1
            .Select(p => (Expression) Expression.MakeMemberAccess(constantExpression, p))
            .ToArray(orderedProps1.Length);
        }
      }

      var orderedProps = expression
        .Type
        .GetProperties();
      Array.Sort(orderedProps, CompareProps);

      return orderedProps
        .Select(p => (Expression) Expression.MakeMemberAccess(expression, p))
        .ToArray(orderedProps.Length);

      static int CompareProps(PropertyInfo p1, PropertyInfo p2)
      {
        return StringComparer.Ordinal.Compare(p1.Name, p2.Name);
      }
    }

    protected override Expression VisitMemberInit(MemberInitExpression mi)
    {
      var newExpression = mi.NewExpression;
      VisitNewExpressionArgumentsSkipResults(newExpression);
      var bindings = VisitBindingList(mi.Bindings).Cast<MemberAssignment>();
      var constructorExpression = (ConstructorExpression) VisitNew(mi.NewExpression);
      foreach (var binding in bindings) {
        var member = binding.Member.MemberType == MemberTypes.Property
          ? TryGetActualPropertyInfo((PropertyInfo) binding.Member, mi.NewExpression.Type)
          : binding.Member;
        constructorExpression.Bindings[member] = binding.Expression;
        constructorExpression.NativeBindings[member] = binding.Expression;
      }
      return constructorExpression;
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private Expression GetMember(Expression expression, MemberInfo member, Expression sourceExpression)
    {
      if (expression == null) {
        return null;
      }

      expression = expression.StripCasts();
      var isMarker = expression.TryGetMarker(out var markerType);
      expression = expression.StripMarkers();
      expression = expression.StripCasts();

      if (expression.IsAnonymousConstructor()) {
        var newExpression = (NewExpression) expression;
        var memberIndex = newExpression.Members.IndexOf(member);
        if (memberIndex < 0)
          throw new InvalidOperationException(string.Format(Strings.ExCouldNotGetMemberXFromExpression, member));
        var argument = newExpression.Arguments[memberIndex];
        var currentLambda = State.CurrentLambda;
        if (!(currentLambda is null) && context.Bindings.TryGetValue(currentLambda.Parameters[0], out var projection)) {
          // Here, we try to detect whether the expression has already visited, in such cases
          // re-visiting it may cause issues. For instance, .OrderBy(x=>x.SomeField) translation
          // gets projection of already visited source and substitutes parameter 'x' access with 
          // that projection. For now TranslatorState.ShouldOmitConvertToObject == true, is a marker
          // of OrderBy visiting, because it is the only place that sets 'true' to the property,
          // but we can't rely on it for future-proof solution.

          // Knowing that parameter-to-projection binding happens when expression has visited,
          // we assume this check is enough for certain cases, probably not for all of them.
          // We can't deny visiting of the argument for all the cases because of chance to break
          // some cases we could not imagine at the time of changes
          switch (argument.NodeType) {
            case ExpressionType.TypeAs:
            case ExpressionType.Convert:
            case ExpressionType.ConvertChecked:
              if (argument.Type == WellKnownTypes.Object && State.ShouldOmitConvertToObject)
                argument = argument.StripCasts();
              break;
          }
        }
        else {
          argument = Visit(argument);
        }
        return isMarker ? new MarkerExpression(argument, markerType) : argument;
      }

      var extendedExpression = expression as ExtendedExpression;
      if (extendedExpression == null) {
        return IsConditionalOrWellknown(expression)
          ? GetConditionalMember(expression, member, sourceExpression)
          : null;
      }

      Expression result = null;
      bool propertyFilter(PersistentFieldExpression f)
      {
        return f.Name == context.Domain.Handlers.NameBuilder.BuildFieldName((PropertyInfo) member);
      }

      switch (extendedExpression.ExtendedType) {
        case ExtendedExpressionType.FullText:
          switch (member.Name) {
            case "Rank":
              return ((FullTextExpression) expression).RankExpression;
            case "Entity":
              return ((FullTextExpression) expression).EntityExpression;
          }
          break;
        case ExtendedExpressionType.Grouping:
          if (member.Name == WellKnown.KeyFieldName) {
            return ((GroupingExpression) expression).KeyExpression;
          }
          break;
        case ExtendedExpressionType.Constructor:
          var nativeExpression = ((ConstructorExpression) extendedExpression);
          var bindings = nativeExpression.Bindings;
          // only make sure that type has needed member
          if (!bindings.TryGetValue(member, out result)) {
            // Key in bindings might be a property/field reflected from a base type
            // but our member might be reflected from child type.
            var baseType = member.DeclaringType;
            if (baseType.IsInterface) {
              var implementor = member.GetImplementation(nativeExpression.Type);
              if (implementor == null) {
                throw new InvalidOperationException(string.Format(Strings.ExThereIsNoImplemetationOfXYMemberInZType,
                  member.DeclaringType.Name, member.Name, nativeExpression.Type.ToString()));
              }
              _ = bindings.TryGetValue(implementor, out result);
            }
            else {
              var baseMember = baseType.GetMember(member.Name).FirstOrDefault();
              if (baseMember == null) {
                throw new InvalidOperationException(string.Format(
                  Strings.ExMemberXOfTypeYIsNotInitializedCheckIfConstructorArgumentIsCorrectOrFieldInitializedThroughInitializer,
                  member.Name, member.ReflectedType.Name));
              }
            }
          }
          result = Visit(result);
          break;
        case ExtendedExpressionType.Structure:
        case ExtendedExpressionType.StructureField:
          var persistentExpression = (IPersistentExpression) expression;
          result = persistentExpression.Fields.First(propertyFilter);
          break;
        case ExtendedExpressionType.LocalCollection:
          var localCollectionExpression = (LocalCollectionExpression) expression;
          result = (Expression) localCollectionExpression.Fields[member];
          break;
        case ExtendedExpressionType.Entity:
          var entityExpression = (EntityExpression) expression;
          result = entityExpression.Fields.FirstOrDefault(propertyFilter);
          if (result == null) {
            EnsureEntityFieldsAreJoined(entityExpression);
            result = entityExpression.Fields.First(propertyFilter);
          }
          break;
        case ExtendedExpressionType.Field:
          if (isMarker && ((markerType & MarkerType.Single) == MarkerType.Single)) {
            throw new InvalidOperationException(string.Format(Strings.ExUseMethodXOnFirstInsteadOfSingle, sourceExpression.ToString(true), member.Name));
          }
          if (member.DeclaringType.IsNullable()) {
            expression = Expression.Convert(expression, member.DeclaringType);
          }
          return Expression.MakeMemberAccess(expression, member);
        case ExtendedExpressionType.EntityField:
          var entityFieldExpression = (EntityFieldExpression) expression;
          result = entityFieldExpression.Fields.FirstOrDefault(propertyFilter);
          if (result == null) {
            EnsureEntityReferenceIsJoined(entityFieldExpression);
            result = entityFieldExpression.Entity.Fields.First(propertyFilter);
          }
          break;
      }

      return isMarker
        ? new MarkerExpression(result, markerType)
        : result;
    }

    private Expression GetMemberWithRemap(Expression expression, MemberInfo memberInfo, Expression sourceExpression)
    {
      var original = GetMember(expression, memberInfo, sourceExpression);
      if (original.IsSubqueryExpression()) {
        var subquery = (SubQueryExpression) original;
        var projectionExpression = subquery.ProjectionExpression;
        var itemProjector = projectionExpression.ItemProjector;
        var columnIndexes = itemProjector.GetColumns(ColumnExtractionModes.KeepSegment).ToArray();

        var expReplacer = new ExtendedExpressionReplacer((e) => {
          if (e is GroupingExpression ge && ge.SelectManyInfo.GroupByProjection != null) {
            var geProjectionExpression = ge.ProjectionExpression;
            var geItemProjector = geProjectionExpression.ItemProjector;
            var columnIndexes = geItemProjector.GetColumns(ColumnExtractionModes.KeepSegment).ToArray();

            var newProjectionExpression = new ProjectionExpression(geProjectionExpression.Type,
              geItemProjector.Remap(geItemProjector.DataSource, columnIndexes),
              geProjectionExpression.TupleParameterBindings);

            var groupByProjection = ge.SelectManyInfo.GroupByProjection;
            var groupByProjector = groupByProjection.ItemProjector;
            var groupByColumnIndexes = groupByProjector.GetColumns(ColumnExtractionModes.KeepSegment).ToArray();

            var newGroupByProjection = new ProjectionExpression(groupByProjection.Type,
              groupByProjector.Remap(groupByProjector.DataSource, groupByColumnIndexes),
              groupByProjection.TupleParameterBindings);

            var result = new GroupingExpression(ge.Type,
              ge.OuterParameter, ge.DefaultIfEmpty, newProjectionExpression, ge.ApplyParameter, ge.KeyExpression,
              new GroupingExpression.SelectManyGroupingInfo(newGroupByProjection));
            return result;
          }
          else
            return null;
        });

        var newItem = expReplacer.Replace(itemProjector.Item);
        var staysTheSame = newItem == itemProjector.Item;
        var newItemProjector = staysTheSame
          ? itemProjector.Remap(itemProjector.DataSource, columnIndexes)
          : new ItemProjectorExpression(newItem, itemProjector.DataSource, itemProjector.Context);

        var result = new SubQueryExpression(subquery.Type,
          subquery.OuterParameter,
          subquery.DefaultIfEmpty,
          new ProjectionExpression(projectionExpression.Type, newItemProjector, projectionExpression.TupleParameterBindings),
          subquery.ApplyParameter,
          subquery.ExtendedType);
        return result;
      }
      return original;
    }

    private Expression GetConditionalMember(Expression expression, MemberInfo member, Expression sourceExpression)
    {
      if (expression is ConditionalExpression ce) {
        var ifTrue = GetConditionalMember(ce.IfTrue, member, sourceExpression);
        var ifFalse = GetConditionalMember(ce.IfFalse, member, sourceExpression);
        if (ifTrue == null || ifFalse == null)
          return null;
        return Expression.Condition(ce.Test, ifTrue, ifFalse);
      }
      if (expression.IsNull()) {
        var mt = member.MemberType;
        Type valueType;
        switch (mt) {
          case MemberTypes.Field:
            var fi = (FieldInfo) member;
            valueType = fi.FieldType;
            break;
          case MemberTypes.Property:
            var pi = (PropertyInfo) member;
            valueType = pi.PropertyType;
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }
        return Expression.Constant(null, valueType.ToNullable());
      }
      return GetMember(expression, member, sourceExpression);
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private Expression VisitTypeAs(Expression source, Type targetType)
    {
      if (source.GetMemberType() != MemberType.Entity) {
        throw new NotSupportedException(Strings.ExAsOperatorSupportsEntityOnly);
      }

      // Expression is already of requested type.
      if (source.Type == targetType) {
        return Visit(source);
      }

      // Call convert to parent type.
      if (targetType.IsAssignableFrom(source.Type)) {
        return Visit(Expression.Convert(source, targetType));
      }

      // Cast to subclass or interface.
      using (CreateScope(new TranslatorState(State))) {
        var bareVisitedSource = Visit(source).StripCasts().StripMarkers();
        if (!(bareVisitedSource is IEntityExpression entityExpression))
          throw new InvalidOperationException(Strings.ExAsOperatorSupportsEntityOnly);

        // Using of state.Parameter[0] is a very weak approach.
        // `as` operator could be applied on expression that has no relation with current parameter
        // thus the later code will fail.
        // We can't easily find real parameter that need replacement.
        // We work around this situation by supporting some known cases.
        // The simplest (and the only at moment) case is a source being chain of MemberExpressions.
        // Some known cases also can be solved by using outer parameter of visited source which is in form of IEntityExpression

        var strippedSource = source.StripMemberAccessChain();
        var currentParameter = State.Parameters[0];

        ParameterExpression parameter;
        if (strippedSource is ParameterExpression pExpression) {
          parameter = pExpression;
        }
        else if (bareVisitedSource is ParameterizedExpression pzExpression && pzExpression.OuterParameter != null) {
          parameter = pzExpression.OuterParameter;
        }
        else {
          parameter = currentParameter;
        }

        // Replace original recordset. New recordset is left join with old recordset
        var originalResultExpression = context.Bindings[parameter];
        var originalQuery = originalResultExpression.ItemProjector.DataSource;
        var offset = originalQuery.Header.Columns.Count;

        // Join primary index of target type
        var targetTypeInfo = context.Model.Types[targetType];
        var indexToJoin = targetTypeInfo.Indexes.PrimaryIndex;
        var queryToJoin = indexToJoin.GetQuery().Alias(context.GetNextAlias());
        var keySegment = entityExpression.Key.Mapping.GetItems();
        var keyPairs = keySegment
          .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
          .ToArray();

        // Replace recordset.
        var joinedRecordQuery = originalQuery.LeftJoin(queryToJoin, keyPairs);
        var itemProjectorExpression = new ItemProjectorExpression(
          originalResultExpression.ItemProjector.Item, joinedRecordQuery, context);
        var projectionExpression = new ProjectionExpression(
          originalResultExpression.Type, itemProjectorExpression, originalResultExpression.TupleParameterBindings);
        context.Bindings.ReplaceBound(parameter, projectionExpression);

        // return new EntityExpression
        var result = EntityExpression.Create(targetTypeInfo, offset, false);
        result.IsNullable = true;
        if (parameter != currentParameter) {
          result = (EntityExpression) result.BindParameter(parameter, new Dictionary<Expression, Expression>());
        }

        return result;
      }
    }

    private void EnsureEntityFieldsAreJoined(EntityExpression entityExpression)
    {
      ItemProjectorExpression itemProjector = entityExpression.OuterParameter == null
        ? context.Bindings[State.Parameters[0]].ItemProjector
        : context.Bindings[entityExpression.OuterParameter].ItemProjector;
      EnsureEntityFieldsAreJoined(entityExpression, itemProjector);
    }

    public void EnsureEntityFieldsAreJoined(EntityExpression entityExpression, ItemProjectorExpression itemProjector)
    {
      TypeInfo typeInfo = entityExpression.PersistentType;
      if (typeInfo.Fields.All(fieldInfo => entityExpression.Fields.Any(entityField => entityField.Name == fieldInfo.Name)))
        return; // All fields are already joined
      IndexInfo joinedIndex = typeInfo.Indexes.PrimaryIndex;
      var joinedRs = joinedIndex.GetQuery().Alias(itemProjector.Context.GetNextAlias());
      Segment<int> keySegment = entityExpression.Key.Mapping;
      Pair<int>[] keyPairs = keySegment.GetItems()
        .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
        .ToArray();
      int offset = itemProjector.DataSource.Header.Length;
      var oldDataSource = itemProjector.DataSource;
      var newDataSource = entityExpression.IsNullable
        ? itemProjector.DataSource.LeftJoin(joinedRs, keyPairs)
        : itemProjector.DataSource.Join(joinedRs, keyPairs);
      itemProjector.DataSource = newDataSource;
      EntityExpression.Fill(entityExpression, offset);
      context.RebindApplyParameter(oldDataSource, newDataSource);
    }

    private void EnsureEntityReferenceIsJoined(EntityFieldExpression entityFieldExpression)
    {
      if (entityFieldExpression.Entity != null)
        return;
      var typeInfo = entityFieldExpression.PersistentType;
      var joinedIndex = typeInfo.Indexes.PrimaryIndex;
      var joinedRs = joinedIndex.GetQuery().Alias(context.GetNextAlias());
      var keySegment = entityFieldExpression.Mapping;
      var keyPairs = keySegment.GetItems()
        .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
        .ToArray();
      var originalItemProjector = entityFieldExpression.OuterParameter == null
        ? context.Bindings[State.Parameters[0]].ItemProjector
        : context.Bindings[entityFieldExpression.OuterParameter].ItemProjector;

      var oldDataSource = originalItemProjector.DataSource;
      var offset = oldDataSource.Header.Length;
      var shouldUseLeftJoin = false;

      var sourceToCheck = (oldDataSource is FilterProvider filterProvider) ? filterProvider.Source : oldDataSource;
      if ((sourceToCheck is ApplyProvider applyProvider && applyProvider.ApplyType == JoinType.LeftOuter) ||
          (sourceToCheck is JoinProvider joinProvider && joinProvider.JoinType == JoinType.LeftOuter)) {
        shouldUseLeftJoin = true;
      }

      var newDataSource = entityFieldExpression.IsNullable || shouldUseLeftJoin
        ? oldDataSource.LeftJoin(joinedRs, keyPairs)
        : oldDataSource.Join(joinedRs, keyPairs);
      originalItemProjector.DataSource = newDataSource;
      entityFieldExpression.RegisterEntityExpression(offset);
      context.RebindApplyParameter(oldDataSource, newDataSource);
    }

    private static Expression MakeBooleanExpression(Expression previous, Expression left, Expression right,
      ExpressionType operationType, ExpressionType concatenationExpression)
    {
      BinaryExpression binaryExpression;
      switch (operationType) {
        case ExpressionType.Equal:
          binaryExpression = Expression.Equal(left, right);
          break;
        case ExpressionType.NotEqual:
          binaryExpression = Expression.NotEqual(left, right);
          break;
        case ExpressionType.OrElse:
          binaryExpression = Expression.OrElse(left, right);
          break;
        case ExpressionType.AndAlso:
          binaryExpression = Expression.AndAlso(left, right);
          break;
        default:
          throw new ArgumentOutOfRangeException("operationType");
      }


      if (previous==null)
        return binaryExpression;

      switch (concatenationExpression) {
      case ExpressionType.AndAlso:
        return Expression.AndAlso(previous, binaryExpression);
      case ExpressionType.OrElse:
        return Expression.OrElse(previous, binaryExpression);
      default:
        throw new ArgumentOutOfRangeException("concatenationExpression");
      }
    }

    private static ProjectionExpression CreateLocalCollectionProjectionExpression(Type itemType, object value, Translator translator, Expression sourceExpression)
    {
      var storedEntityType = translator.State.TypeOfEntityStoredInKey;
      var translatorContext = translator.context;
      var itemToTupleConverter = ItemToTupleConverter.BuildConverter(itemType, storedEntityType, value, translatorContext.Model, sourceExpression);
      var tupleDescriptor = itemToTupleConverter.TupleDescriptor;
      var columns = tupleDescriptor
        .Select(x => new SystemColumn(translatorContext.GetNextColumnAlias(), 0, x))
        .Cast<Column>()
        .ToArray(tupleDescriptor.Count);
      var rsHeader = new RecordSetHeader(tupleDescriptor, columns);
      var rawProvider = new RawProvider(rsHeader, itemToTupleConverter.GetEnumerable());
      var recordset = new StoreProvider(rawProvider);
      var itemProjector = new ItemProjectorExpression(itemToTupleConverter.Expression, recordset, translatorContext);
      if (translator.State.JoinLocalCollectionEntity)
        itemProjector = EntityExpressionJoiner.JoinEntities(translator, itemProjector);
      return new ProjectionExpression(itemType, itemProjector, TranslatedQuery.EmptyTupleParameterBindings);
    }

    private Expression BuildInterfaceExpression(MemberExpression ma)
    {
      var @interface = ma.Expression.Type;
      var property = (PropertyInfo) ma.Member;
      var implementors = context.Model.Types[@interface].AllImplementors;
      var fields = implementors
        .Select(im => im.UnderlyingType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public))
        .Concat(implementors
          .Select(im => im.UnderlyingType.GetProperty($"{@interface.Name}.{property.Name}", BindingFlags.Instance | BindingFlags.NonPublic)))
        .Where(f => f != null);

      return BuildExpression(ma, fields);
    }

    private Expression BuildHierarchyExpression(MemberExpression ma)
    {
      var ancestor = ma.Expression.Type;
      var property = (PropertyInfo) ma.Member;
      var descendants = context.Model.Types[ancestor].AllDescendants;
      var fields = descendants
        .Select(im => im.UnderlyingType.GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        .Where(f => f != null);

      return BuildExpression(ma, fields);
    }

    private Expression BuildExpression(MemberExpression ma, IEnumerable<PropertyInfo> fields)
    {
      object defaultValue = null;
      var propertyType = ((PropertyInfo) ma.Member).PropertyType;
      if (propertyType.IsValueType && !propertyType.IsNullable())
        defaultValue = System.Activator.CreateInstance(propertyType);

      Expression current = Expression.Constant(defaultValue, propertyType);
      foreach (var field in fields) {
        var compiler = context.CustomCompilerProvider.GetCompiler(field);
        if (compiler == null)
          continue;
        var expression = compiler.Invoke(Expression.TypeAs(ma.Expression, field.ReflectedType), null);
        current = Expression.Condition(Expression.TypeIs(ma.Expression, field.ReflectedType), expression, current);
      }
      return current;
    }

    private MemberInfo TryGetActualPropertyInfo(PropertyInfo propertyInfo, Type initializingType)
    {
      //if property is an indexer
      if (propertyInfo.GetIndexParameters().Length != 0)
        return propertyInfo;

      // the name of property is unique within type hierarchy
      // so we can use it.
      var actualPropertyInfo = initializingType.GetProperty(propertyInfo.Name);
      return actualPropertyInfo ?? propertyInfo;
    }

    private IList<ColumnInfo> GetColumnsToSearch(ConstantExpression arrayOfColumns, Type elementType, TypeInfo domainType)
    {
      var columnAccessLambdas = (LambdaExpression[])arrayOfColumns.Value;
      var fulltextFields = new List<ColumnInfo>(columnAccessLambdas.Length);
      foreach (var lambda in columnAccessLambdas) {
        var field = FieldExtractor.Extract(lambda, elementType, domainType);
        if (field.Column == null)
          throw new InvalidOperationException(string.Format(Strings.FieldXIsComplexAndCannotBeUsedForSearch, lambda.Body));
        fulltextFields.Add(field.Column);
      }
      return fulltextFields;
    }

    private void ArrangeContainsTableParameters(System.Collections.ObjectModel.ReadOnlyCollection<Expression> parameters, out Expression searchCriteria, out Expression columns, out Expression topNByRank)
    {
      searchCriteria = parameters[0];
      columns = null;
      topNByRank = null;
      if (parameters.Count == 2) {
        if (parameters[1].Type.IsArray)
          columns = parameters[1];
        else
          topNByRank = parameters[1];
      }
      if (parameters.Count == 3) {
        columns = parameters[1];
        topNByRank = parameters[2];
      }
    }

    #endregion
  }
}