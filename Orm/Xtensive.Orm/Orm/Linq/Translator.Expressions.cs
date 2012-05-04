// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.27

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Parameters;
using Xtensive.Reflection;
using Xtensive.Orm.Linq.Rewriters;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers.Compilable;
using FieldInfo = System.Reflection.FieldInfo;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm.Linq
{
  internal sealed partial class Translator
  {
    protected override Expression VisitTypeIs(TypeBinaryExpression tb)
    {
      var expression = tb.Expression;
      Type expressionType = expression.Type;
      Type operandType = tb.TypeOperand;
      if (operandType.IsAssignableFrom(expressionType))
        return Expression.Constant(true);

      // Structure
      var memberType = expression.GetMemberType();
      if (memberType==MemberType.Structure
        && typeof (Structure).IsAssignableFrom(operandType))
        return Expression.Constant(false);

      // Entity
      if (memberType==MemberType.Entity
        && typeof (IEntity).IsAssignableFrom(operandType)) {
        TypeInfo typeInfo = context.Model.Types[operandType];
        IEnumerable<int> typeIds = typeInfo.GetDescendants(true).Union(typeInfo.GetImplementors(true)).AddOne(typeInfo).Select(ti => ti.TypeId);
        MemberExpression memberExpression = Expression.MakeMemberAccess(expression, WellKnownMembers.TypeId);
        Expression boolExpression = null;
        foreach (int typeId in typeIds)
          boolExpression = MakeBinaryExpression(
            boolExpression,
            memberExpression,
            Expression.Constant(typeId),
            ExpressionType.Equal,
            ExpressionType.OrElse);

        return Visit(boolExpression);
      }

      throw new NotSupportedException(Strings.ExTypeIsMethodSupportsOnlyEntitiesAndStructures);
    }

    protected override Expression Visit(Expression e)
    {
      if (e==null)
        return null;
      if (e.IsProjection())
        return e;
      if (context.Evaluator.CanBeEvaluated(e)) {
        if (typeof (IQueryable).IsAssignableFrom(e.Type))
          return base.Visit(ExpressionEvaluator.Evaluate(e));
        return context.ParameterExtractor.IsParameter(e)
          ? e
          : ExpressionEvaluator.Evaluate(e);
      }
      return base.Visit(e);
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
        if (u.GetMemberType()==MemberType.Entity)
          return VisitTypeAs(u.Operand, u.Type);
        break;
      case ExpressionType.Convert:
      case ExpressionType.ConvertChecked:
        if (u.GetMemberType()==MemberType.Entity) {
          if (u.Type==u.Operand.Type
            || u.Type.IsAssignableFrom(u.Operand.Type)
            || !typeof (IEntity).IsAssignableFrom(u.Operand.Type))
            return base.VisitUnary(u);
          throw new InvalidOperationException(String.Format(Strings.ExDowncastFromXToXNotSupportedUseOfTypeOrAsOperatorInstead, u, u.Operand.Type, u.Type));
        }
        break;
      }
      return u.Type==typeof (IQueryable)
        ? Visit(u.Operand)
        : base.VisitUnary(u);
    }

    protected override Expression VisitLambda(LambdaExpression le)
    {
      using (state.CreateLambdaScope(le)) {
        state.AllowCalculableColumnCombine = false;
        Expression body = le.Body;
        if (!state.IsTailMethod)
          body = NullComparsionRewriter.Rewrite(body);
        body = Visit(body);
        ParameterExpression parameter = le.Parameters[0];
        if (body.NodeType!=ExpressionType.New
          && body.NodeType!=ExpressionType.MemberInit
            && !(body.NodeType==ExpressionType.Constant && state.BuildingProjection))
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
      using (state.CreateScope()) {
        state.CalculateExpressions = false;
        expression = Visit(ma.Expression);
      }

      expression = expression.IsProjection() 
        ? BuildSubqueryResult((ProjectionExpression) expression, ma.Expression.Type) 
        : ProcessProjectionElement(expression);

      if (expression!=ma.Expression)
        return Expression.Bind(ma.Member, expression);
      return ma;
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
      Expression left;
      Expression right;
      MemberType memberType = binaryExpression.Left.Type==typeof (object)
        ? binaryExpression.Right.GetMemberType()
        : binaryExpression.Left.GetMemberType();
      if (memberType==MemberType.EntitySet) {
        if (context.Evaluator.CanBeEvaluated(binaryExpression.Left)) {
          left = ExpressionEvaluator.Evaluate(binaryExpression.Left);
        }
        else {
          var leftMemberAccess = binaryExpression.Left as MemberExpression;
          left = leftMemberAccess!=null && leftMemberAccess.Member.ReflectedType.IsClosure()
            ? ExpressionEvaluator.Evaluate(leftMemberAccess)
            : Visit(binaryExpression.Left);
        }
        if (context.Evaluator.CanBeEvaluated(binaryExpression.Right)) {
          right = ExpressionEvaluator.Evaluate(binaryExpression.Right);
        }
        else {
          var rightMemberAccess = binaryExpression.Right as MemberExpression;
          right = rightMemberAccess!=null && rightMemberAccess.Member.ReflectedType.IsClosure()
            ? ExpressionEvaluator.Evaluate(rightMemberAccess)
            : Visit(binaryExpression.Right);
        }
      }
      else {
        left = Visit(binaryExpression.Left);
        right = Visit(binaryExpression.Right);
      }
      if (left.Type.StripNullable().IsEnum)
        left = ConvertEnum(left);
      if (right.Type.StripNullable().IsEnum)
        right = ConvertEnum(right);
      var resultBinaryExpression = Expression.MakeBinary(binaryExpression.NodeType,
        left,
        right,
        binaryExpression.IsLiftedToNull,
        binaryExpression.Method);

      if (binaryExpression.NodeType==ExpressionType.Equal
        || binaryExpression.NodeType==ExpressionType.NotEqual)
        return VisitBinaryRecursive(resultBinaryExpression, binaryExpression);

      if (binaryExpression.NodeType==ExpressionType.ArrayIndex) {
        var newArrayExpression = left.StripCasts() as NewArrayExpression;
        var indexExpression = right.StripCasts() as ConstantExpression;
        if (newArrayExpression!=null && indexExpression!=null && indexExpression.Type==typeof (int))
          return newArrayExpression.Expressions[(int) indexExpression.Value];

        throw new NotSupportedException(String.Format(Strings.ExBinaryExpressionXOfTypeXIsNotSupported, binaryExpression.ToString(true), binaryExpression.NodeType));
      }

      return resultBinaryExpression;
    }

    private Expression ConvertEnum(Expression left)
    {
      var underlyingType = Enum.GetUnderlyingType(left.Type.StripNullable());
      if (left.Type.IsNullable())
        underlyingType = underlyingType.ToNullable();
      left = left.NodeType==ExpressionType.Convert
        ? Expression.Convert(((UnaryExpression) left).Operand, underlyingType)
        : Expression.Convert(left, underlyingType);
      return left;
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected override Expression VisitParameter(ParameterExpression p)
    {
      bool isInnerParameter = state.Parameters.Contains(p);
      bool isOuterParameter = state.OuterParameters.Contains(p);

      if (!isInnerParameter && !isOuterParameter)
        throw new InvalidOperationException(Strings.ExLambdaParameterIsOutOfScope);
      ItemProjectorExpression itemProjector = context.Bindings[p].ItemProjector;
      if (isOuterParameter)
        return context.GetBoundItemProjector(p, itemProjector).Item;
      return itemProjector.Item;
    }

    protected override Expression VisitMemberAccess(MemberExpression ma)
    {
      if (ma.Expression != null)
        if (ma.Expression.Type!=ma.Member.ReflectedType
          && ma.Member is PropertyInfo
          && !ma.Member.ReflectedType.IsInterface)
          ma = Expression.MakeMemberAccess(
            ma.Expression, ma.Expression.Type.GetProperty(ma.Member.Name, ma.Member.GetBindingFlags()));
      var customCompiler = context.CustomCompilerProvider.GetCompiler(ma.Member);

      // Reflected type doesn't have custom compiler defined, so falling back to base class compiler
      var declaringType = ma.Member.DeclaringType;
      Type reflectedType = ma.Member.ReflectedType;
      if (customCompiler == null && declaringType != reflectedType && declaringType.IsAssignableFrom(reflectedType)) {
        var root = declaringType;
        var current = reflectedType;
        while (current != root && customCompiler == null) {
          current = current.BaseType;
          var member = current.GetProperty(ma.Member.Name, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
          customCompiler = context.CustomCompilerProvider.GetCompiler(member);
        }
      }

      if (customCompiler!=null) {
        var member = ma.Member;
        var expression = customCompiler.Invoke(ma.Expression, ArrayUtils<Expression>.EmptyArray);
        if (expression == null) {
          if (member.ReflectedType.IsInterface)
            return Visit(BuildInterfaceExpression(ma));
          if (member.ReflectedType.IsClass)
            return Visit(BuildHierarchyExpression(ma));
        }
        else
          return Visit(expression);
      }

      if (context.Evaluator.CanBeEvaluated(ma) && context.ParameterExtractor.IsParameter(ma)) {
        if (typeof (IQueryable).IsAssignableFrom(ma.Type)) {
          Func<IQueryable> lambda = Expression.Lambda<Func<IQueryable>>(ma).CachingCompile();
          IQueryable rootPoint = lambda();
          if (rootPoint!=null)
            return base.Visit(rootPoint.Expression);
        }
        return ma;
      }
      if (ma.Expression==null) {
        if (typeof (IQueryable).IsAssignableFrom(ma.Type)) {
          var lambda = Expression.Lambda<Func<IQueryable>>(ma).CachingCompile();
          var rootPoint = lambda();
          if (rootPoint!=null)
            return VisitSequence(rootPoint.Expression);
        }
      }
      else if (ma.Expression.NodeType==ExpressionType.Constant) {
        var rfi = ma.Member as FieldInfo;
        if (rfi!=null && (rfi.FieldType.IsGenericType && typeof (IQueryable).IsAssignableFrom(rfi.FieldType))) {
          var lambda = Expression.Lambda<Func<IQueryable>>(ma).CachingCompile();
          var rootPoint = lambda();
          if (rootPoint!=null)
            return VisitSequence(rootPoint.Expression);
        }
      }
      else if (ma.Expression.GetMemberType() == MemberType.Entity && ma.Member.Name != "Key") {
        var type = ma.Expression.Type;
        var parameter = ma.Expression as ParameterExpression;
        if (parameter != null) {
          var projection = context.Bindings[parameter];
          type = projection.ItemProjector.Item.Type;
        }
        if (!context.Model.Types[type].Fields.Contains(context.Domain.Handlers.NameBuilder.BuildFieldName((PropertyInfo) ma.Member)))
          throw new NotSupportedException(String.Format(Strings.ExFieldMustBePersistent, ma.ToString(true)));
      }
      Expression source;
      using (state.CreateScope()) {
//        state.BuildingProjection = false;
        source = Visit(ma.Expression);
      }

      var result = GetMember(source, ma.Member, ma);
      return result ?? base.VisitMemberAccess(ma);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      using (state.CreateScope()) {
        state.IsTailMethod = mc==context.Query && mc.IsQuery();
        var customCompiler = context.CustomCompilerProvider.GetCompiler(mc.Method);
        if (customCompiler!=null)
          return Visit(customCompiler.Invoke(mc.Object, mc.Arguments.ToArray()));

        // Visit Query. Deprecated.
#pragma warning disable 612,618
        if (mc.Method.DeclaringType==typeof (Query)) {
          // Query.All<T>
          if (mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition()==WellKnownMembers.Query.All)
            return ConstructQueryable(mc);
          // Query.FreeText<T>
          if (mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition().In(WellKnownMembers.Query.FreeTextString, WellKnownMembers.Query.FreeTextExpression))
            return ConstructFreeTextQueryRoot(mc.Method.GetGenericArguments()[0], mc.Arguments[0]);
          // Query.Single<T> & Query.SingleOrDefault<T>
          if (mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition().In(
            WellKnownMembers.Query.SingleKey,
            WellKnownMembers.Query.SingleOrDefaultKey))
            return VisitQuerySingle(mc);
          throw new InvalidOperationException(String.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
        }
        // Visit QueryEndpoint.
        if (mc.Method.DeclaringType == typeof(QueryEndpoint)) {
          // Query.All<T>
          if (mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition() == WellKnownMembers.QueryEndpoint.All)
            return ConstructQueryable(mc);
          // Query.FreeText<T>
          if (mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition().In(WellKnownMembers.QueryEndpoint.FreeTextString, WellKnownMembers.QueryEndpoint.FreeTextExpression))
            return ConstructFreeTextQueryRoot(mc.Method.GetGenericArguments()[0], mc.Arguments[0]);
          // Query.Single<T> & Query.SingleOrDefault<T>
          if (mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition().In(
            WellKnownMembers.QueryEndpoint.SingleKey,
            WellKnownMembers.QueryEndpoint.SingleOrDefaultKey))
            return VisitQuerySingle(mc);
          throw new InvalidOperationException(String.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
        }
#pragma warning restore 612,618

        // Visit Queryable extensions.
        if (mc.Method.DeclaringType==typeof (QueryableExtensions))
          if (mc.Method.Name==WellKnownMembers.Queryable.ExtensionLeftJoin.Name)
            return VisitLeftJoin(mc);
          else if (mc.Method.Name=="In")
            return VisitIn(mc);
          else if (mc.Method.Name==WellKnownMembers.Queryable.ExtensionLock.Name)
            return VisitLock(mc);
          else if (mc.Method.Name==WellKnownMembers.Queryable.ExtensionTake.Name)
            return VisitTake(mc.Arguments[0], mc.Arguments[1]);
          else if (mc.Method.Name==WellKnownMembers.Queryable.ExtensionSkip.Name)
            return VisitSkip(mc.Arguments[0], mc.Arguments[1]);
          else if (mc.Method.Name==WellKnownMembers.Queryable.ExtensionElementAt.Name)
            return VisitElementAt(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), mc.Method.ReturnType, false);
          else if (mc.Method.Name==WellKnownMembers.Queryable.ExtensionElementAtOrDefault.Name)
            return VisitElementAt(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), mc.Method.ReturnType, true);
          else if (mc.Method.Name == WellKnownMembers.Queryable.ExtensionCount.Name)
            return VisitAggregate(mc.Arguments[0], mc.Method, null, context.IsRoot(mc), mc);
          else
            throw new InvalidOperationException(String.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
        // Visit Collection extensions
        if (mc.Method.DeclaringType==typeof (CollectionExtensions))
          if (mc.Method.Name==WellKnownMembers.Collection.ExtensionContainsAny.Name)
            return VisitContainsAny(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), mc.Method.GetGenericArguments()[0]);
          else if (mc.Method.Name==WellKnownMembers.Collection.ExtensionContainsAll.Name)
            return VisitContainsAll(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), mc.Method.GetGenericArguments()[0]);
          else if (mc.Method.Name==WellKnownMembers.Collection.ExtensionContainsNone.Name)
            return VisitContainsNone(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), mc.Method.GetGenericArguments()[0]);

        // Process local collections
        if (mc.Object.IsLocalCollection(context)) {
          // IList.Contains
          // List.Contains 
          // Array.Contains
          ParameterInfo[] parameters = mc.Method.GetParameters();
          if (mc.Method.Name=="Contains" && parameters.Length==1)
            return VisitContains(mc.Object, mc.Arguments[0], false);
        }

        var result = base.VisitMethodCall(mc);
        if (result!=mc && result.NodeType==ExpressionType.Call) {
          var visitedMethodCall = (MethodCallExpression) result;
          if (visitedMethodCall.Arguments.Any(arg => arg.IsProjection()))
            throw new InvalidOperationException(String.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
        }
        return result;
      }
    }

    private Expression ConstructFreeTextQueryRoot(Type elementType, Expression searchCriteria)
    {
      TypeInfo type;
      if (!context.Model.Types.TryGetValue(elementType, out type))
        throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));
      var fullTextIndex = type.FullTextIndex;
      if (fullTextIndex==null)
        throw new InvalidOperationException(String.Format(Strings.ExEntityDoesNotHaveFullTextIndex, elementType.FullName));

      if (QueryCachingScope.Current!=null
        && searchCriteria.NodeType==ExpressionType.Constant
          && searchCriteria.Type==typeof (string))
        throw new InvalidOperationException(String.Format(Strings.ExFreeTextNotSupportedInCompiledQueries, ((ConstantExpression) searchCriteria).Value));


      // Prepare parameter

      Func<string> compiledParameter;
      if (searchCriteria.NodeType==ExpressionType.Quote)
        searchCriteria = searchCriteria.StripQuotes();
      if (searchCriteria.Type==typeof (Func<string>)) {
        if (QueryCachingScope.Current==null)
          compiledParameter = ((Expression<Func<string>>) searchCriteria).CachingCompile();
        else {
          var replacer = QueryCachingScope.Current.QueryParameterReplacer;
          var queryParameter = QueryCachingScope.Current.QueryParameter;
          var newSearchCriteria = replacer.Replace(searchCriteria);
          compiledParameter = ((Expression<Func<string>>) newSearchCriteria).CachingCompile();
        }
      }
      else {
        Expression<Func<string>> parameter = context.ParameterExtractor.ExtractParameter<string>(searchCriteria);
        compiledParameter = parameter.CachingCompile();
      }

      var fullFeatured = context.ProviderInfo.Supports(ProviderFeatures.FullFeaturedFullText);
      var entityExpression = EntityExpression.Create(type, 0, !fullFeatured);
      var dataSource = new FreeTextProvider(fullTextIndex, compiledParameter, context.GetNextColumnAlias(), fullFeatured);
      var rankExpression = ColumnExpression.Create(typeof (double), dataSource.Header.Columns.Count - 1);
      var freeTextExpression = new FullTextExpression(fullTextIndex, entityExpression, rankExpression, null);
      var itemProjector = new ItemProjectorExpression(freeTextExpression, dataSource, context);
      return new ProjectionExpression(typeof (IQueryable<>).MakeGenericType(elementType), itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected override Expression VisitNew(NewExpression newExpression)
    {
      if (newExpression.Members==null) {
        if (newExpression.IsGroupingExpression() ||
          newExpression.IsSubqueryExpression() ||
            newExpression.Type==typeof (TimeSpan) ||
              newExpression.Type==typeof (DateTime))
          return base.VisitNew(newExpression);
      }
      var arguments = VisitNewExpressionArguments(newExpression);
      if (newExpression.IsAnonymousConstructor()) {
        return newExpression.Members==null
          ? Expression.New(newExpression.Constructor, arguments)
          : Expression.New(newExpression.Constructor, arguments, newExpression.Members);
      }
      var constructorParameters = newExpression.GetConstructorParameters();
      if (constructorParameters.Length!=arguments.Count)
        throw Exceptions.InternalError(Strings.ExInvalidNumberOfParametersInNewExpression, Log.Instance);
      var duplicateMembers = new SetSlim<MemberInfo>();
      var bindings = new Dictionary<MemberInfo, Expression>();
      for (int i = 0; i < constructorParameters.Length; i++) {
        int parameterIndex = i;
        var constructorParameter = constructorParameters[parameterIndex];
        var members = newExpression
          .Type
          .GetMembers()
          .Where(mi => FilterBindings(mi, constructorParameter.Name, constructorParameter.ParameterType))
          .ToList();
        if (members.Count()==1 && !duplicateMembers.Contains(members[0])) {
          if (bindings.ContainsKey(members[0])) {
            bindings.Remove(members[0]);
            duplicateMembers.Add(members[0]);
          }
          else
            bindings.Add(members[0], arguments[parameterIndex]);
        }
      }

      return new ConstructorExpression(newExpression.Type, bindings, newExpression.Constructor, arguments);
    }

    internal static bool FilterBindings(MemberInfo mi, string name, Type type)
    {
      var result = String.Equals(mi.Name, name, StringComparison.InvariantCultureIgnoreCase);
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

      IList<Expression> leftExpressions;
      IList<Expression> rightExpressions;

      // Split left and right arguments to subexpressions.
      MemberType memberType = left.Type==typeof (object)
        ? right.GetMemberType()
        : left.GetMemberType();
      switch (memberType) {
      case MemberType.EntitySet:
        if ((leftIsConstant && ExpressionEvaluator.Evaluate(left).Value==null)
          || left is ConstantExpression && ((ConstantExpression) left).Value==null)
          return Expression.Constant(binaryExpression.NodeType==ExpressionType.NotEqual, typeof (bool));
        if ((rightIsConstant && ExpressionEvaluator.Evaluate(right).Value==null)
          || right is ConstantExpression && ((ConstantExpression) right).Value==null)
          return Expression.Constant(binaryExpression.NodeType==ExpressionType.NotEqual, typeof (bool));
        var leftEntitySetExpression = left as EntitySetExpression;
        var rightEntitySetExpression = right as EntitySetExpression;
        if (leftEntitySetExpression!=null && rightEntitySetExpression!=null) {
          if (leftEntitySetExpression.Field!=rightEntitySetExpression.Field)
            return Expression.Constant(false, typeof (bool));
          var binary = Expression.MakeBinary(binaryExpression.NodeType,
            (Expression) leftEntitySetExpression.Owner,
            (Expression) rightEntitySetExpression.Owner,
            binaryExpression.IsLiftedToNull,
            binaryExpression.Method);
          return VisitBinaryRecursive(binary, originalBinaryExpression);
        }
        if (rightEntitySetExpression!=null && left is ConstantExpression) {
          var leftEntitySet = (EntitySetBase) ((ConstantExpression) left).Value;
          var binary = Expression.MakeBinary(binaryExpression.NodeType,
            Expression.Constant(leftEntitySet.Owner),
            (Expression) rightEntitySetExpression.Owner,
            binaryExpression.IsLiftedToNull,
            binaryExpression.Method);
          return VisitBinaryRecursive(binary, originalBinaryExpression);
        }
        if (leftEntitySetExpression!=null && right is ConstantExpression) {
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
        if (leftKeyExpression==null && rightKeyExpression==null)
          throw new InvalidOperationException(String.Format(Strings.ExBothLeftAndRightPartOfBinaryExpressionXAreNULLOrNotKeyExpression, originalBinaryExpression.ToString(true)));
        // Check key compatibility
        leftKeyExpression.EnsureKeyExpressionCompatible(rightKeyExpression, originalBinaryExpression);
        // Key split to it's fields.
        IEnumerable<Type> keyFields = (leftKeyExpression ?? rightKeyExpression)
          .KeyFields
          .Select(fieldExpression => fieldExpression.Type);
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
         var type = left.Type == typeof(object)
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
        leftExpressions = GetAnonymousArguments(left);
        rightExpressions = GetAnonymousArguments(right);
        break;
      case MemberType.Structure:
        if ((leftIsConstant && ExpressionEvaluator.Evaluate(left).Value==null)
          || left is ConstantExpression && ((ConstantExpression) left).Value==null)
          return Expression.Constant(binaryExpression.NodeType==ExpressionType.NotEqual, typeof (bool));
        if ((rightIsConstant && ExpressionEvaluator.Evaluate(right).Value==null)
          || right is ConstantExpression && ((ConstantExpression) right).Value==null)
          return Expression.Constant(binaryExpression.NodeType==ExpressionType.NotEqual, typeof (bool));
        // Structure split to it's fields.
        var leftStructureExpression = left as StructureFieldExpression;
        var rightStructureExpression = right as StructureFieldExpression;
        if (leftStructureExpression==null && rightStructureExpression==null)
          throw new NotSupportedException(String.Format(Strings.ExBothLeftAndRightPartOfBinaryExpressionXAreNULLOrNotStructureExpression, binaryExpression));

        StructureFieldExpression structureFieldExpression = (leftStructureExpression ?? rightStructureExpression);
        leftExpressions = GetStructureFields(left, structureFieldExpression.Fields, structureFieldExpression.Type);
        rightExpressions = GetStructureFields(right, structureFieldExpression.Fields, structureFieldExpression.Type);
        break;
      case MemberType.Array:
        // Special case. ArrayIndex expression. 
        if (binaryExpression.NodeType==ExpressionType.ArrayIndex) {
          var arrayExpression = Visit(left);
          var arrayIndex = Visit(right);
          return Expression.ArrayIndex(arrayExpression, arrayIndex);
        }

        // If array compares to null use standart routine. 
        if ((rightIsConstant && ExpressionEvaluator.Evaluate(right).Value==null)
          || (rightIsConstant && ExpressionEvaluator.Evaluate(right).Value==null)
            || (right.Type==typeof (byte[]) && (left is FieldExpression || left is ColumnExpression || right is FieldExpression || right is ColumnExpression)))
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
        return binaryExpression;
      }

      if (leftExpressions.Count!=rightExpressions.Count || leftExpressions.Count==0)
        throw Exceptions.InternalError(Strings.ExMistmatchCountOfLeftAndRightExpressions, Log.Instance);

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
        resultExpression = resultExpression==null
          ? visitedResultExpression
          : Expression.AndAlso(resultExpression, visitedResultExpression);
      }

      // Return result.
      return resultExpression;
    }

    private static bool IsConditionalOrWellknown(Expression expression, bool isRoot = true)
    {
      var conditionalExpression = expression as ConditionalExpression;
      if (conditionalExpression != null)
        return IsConditionalOrWellknown(conditionalExpression.IfTrue, false)
               && IsConditionalOrWellknown(conditionalExpression.IfFalse, false);
      if (isRoot)
        return false;
      if (expression.NodeType == ExpressionType.Constant)
        return true;
      if (expression.NodeType == ExpressionType.Convert) {
        var unary = (UnaryExpression)expression;
        return IsConditionalOrWellknown(unary.Operand, false);
      }
      if (!(expression is ExtendedExpression))
        return false;

      var memberType = expression.GetMemberType();
      switch (memberType)
      {
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

    private IList<Expression> GetStructureFields(
      Expression expression,
      IEnumerable<PersistentFieldExpression> structureFields,
      Type structureType)
    {
      expression = expression.StripCasts();
      if (expression is IPersistentExpression)
        return ((IPersistentExpression) expression)
          .Fields
          .Where(field => field.GetMemberType()==MemberType.Primitive)
          .Select(e => (Expression) e)
          .ToList();

      ConstantExpression nullExpression = Expression.Constant(null, structureType);
      BinaryExpression isNullExpression = Expression.Equal(expression, nullExpression);

      var result = new List<Expression>();
      foreach (PersistentFieldExpression fieldExpression in structureFields) {
        if (!structureType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Contains(fieldExpression.UnderlyingProperty))
          continue;

        Type nullableType = fieldExpression.Type.ToNullable();

        var memberExpression = (Expression) Expression.Condition(
          isNullExpression,
          Expression.Constant(null, nullableType),
          Expression.Convert(
            Expression.MakeMemberAccess(Expression.Convert(expression, structureType), fieldExpression.UnderlyingProperty),
            nullableType));

        switch (fieldExpression.GetMemberType()) {
        case MemberType.Entity:
          IEnumerable<Type> keyFieldTypes = context
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

    private static IList<Expression> GetEntityFields(Expression expression, IEnumerable<Type> keyFieldTypes)
    {
      expression = expression.StripCasts();
      if (expression is IEntityExpression)
        return GetKeyFields(((IEntityExpression) expression).Key, null);


      Expression keyExpression;

      if (expression.IsNull())
        keyExpression = Expression.Constant(null, typeof (Key));
      else if (IsConditionalOrWellknown(expression))
        return keyFieldTypes
          .Select((type, index) => GetConditionalKeyField(expression, type, index))
          .ToList();
      else
      {
        ConstantExpression nullEntityExpression = Expression.Constant(null, expression.Type);
        BinaryExpression isNullExpression = Expression.Equal(expression, nullEntityExpression);
        if (!typeof (IEntity).IsAssignableFrom(expression.Type))
          expression = Expression.Convert(expression, typeof (IEntity));
        keyExpression = Expression.Condition(
          isNullExpression,
          Expression.Constant(null, typeof (Key)),
          Expression.MakeMemberAccess(expression, WellKnownMembers.IEntityKey));
      }
      return GetKeyFields(keyExpression, keyFieldTypes);
    }

    private static Expression GetConditionalKeyField(Expression expression, Type keyFieldType, int index)
    {
      var ce = expression as ConditionalExpression;
      if (ce != null)
        return Expression.Condition(
          ce.Test,
          GetConditionalKeyField(ce.IfTrue, keyFieldType, index),
          GetConditionalKeyField(ce.IfFalse, keyFieldType, index));
      if (expression.IsNull())
        return Expression.Constant(null, keyFieldType.ToNullable());
      var ee = (IEntityExpression)expression.StripCasts();
      return ee.Key.KeyFields[index].LiftToNullable();
    }

    public static IList<Expression> GetKeyFields(Expression expression, IEnumerable<Type> keyFieldTypes)
    {
      expression = expression.StripCasts();
      if (expression is KeyExpression)
        return ((KeyExpression) expression)
          .KeyFields
          .Select(fieldExpression => (Expression) fieldExpression)
          .ToList();

      if (expression.IsNull())
        return keyFieldTypes
          .Select(type => (Expression) Expression.Constant(null, type.ToNullable()))
          .ToList();
      ConstantExpression nullExpression = Expression.Constant(null, expression.Type);
      BinaryExpression isNullExpression = Expression.Equal(expression, nullExpression);
      MemberExpression keyTupleExpression = Expression.MakeMemberAccess(expression, WellKnownMembers.Key.Value);

      return keyFieldTypes
        .Select((type, index) => {
          Type nullableType = type.ToNullable();
          return (Expression) Expression.Condition(
            isNullExpression,
            Expression.Constant(null, nullableType),
            keyTupleExpression.MakeTupleAccess(nullableType, index));
        })
        .ToList();
    }

    private Expression ProcessProjectionElement(Expression body)
    {
      var originalBodyType = body.Type;
      var reduceCastBody = body.StripCasts();
      if (state.CalculateExpressions
        && reduceCastBody.GetMemberType()==MemberType.Unknown
          && reduceCastBody.NodeType!=ExpressionType.ArrayIndex
            && (ExtendedExpressionType) reduceCastBody.NodeType!=ExtendedExpressionType.Constructor
              && reduceCastBody.NodeType!=ExpressionType.New) {
        var found = false;
        var entityFinder = new ExtendedExpressionReplacer(
          e => {
            if ((int) e.NodeType==(int) ExtendedExpressionType.Entity) {
              found = true;
              return e;
            }
            return null;
          });
        entityFinder.Replace(reduceCastBody);
        if (found)
          return body;

        body = new EnumRewriter().Visit(body);
        var convertExpression = Expression.Convert(body, typeof (object));

        var calculator = ExpressionMaterializer.MakeLambda(convertExpression, context);
        var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), body.Type,
          (Expression<Func<Tuple, object>>) calculator);

        var lambdaParameter = state.Parameters[0];
        var oldResult = context.Bindings[lambdaParameter];
        var isInlined = !state.BuildingProjection && !state.GroupingKey;

        var dataSource = oldResult.ItemProjector.DataSource;

        SortProvider sortProvider = null; 
        if (dataSource is SortProvider) {
          sortProvider = (SortProvider) dataSource;
          dataSource = sortProvider.Source;
        }

        var columns = new List<CalculatedColumnDescriptor>();
        if (state.AllowCalculableColumnCombine && dataSource is CalculateProvider && isInlined==((CalculateProvider) dataSource).IsInlined) {
          var calculateProvider = ((CalculateProvider) dataSource);
          var presentColumns = calculateProvider
            .CalculatedColumns
            .Select(cc => new CalculatedColumnDescriptor(cc.Name, cc.Type, cc.Expression));
          columns.AddRange(presentColumns);
          dataSource = calculateProvider.Source;
        }
        columns.Add(ccd);
        dataSource = dataSource.Calculate(
          isInlined,
          columns.ToArray());

        if (sortProvider!=null)
          dataSource = dataSource.OrderBy(sortProvider.Order);

        var newItemProjector = oldResult.ItemProjector.Remap(dataSource, 0);
        var newResult = new ProjectionExpression(oldResult.Type, newItemProjector, oldResult.TupleParameterBindings);
        context.Bindings.ReplaceBound(lambdaParameter, newResult);

        body = ColumnExpression.Create(originalBodyType, dataSource.Header.Length - 1);
        state.AllowCalculableColumnCombine = true;
      }
      return body;
    }

    private Expression ConstructQueryable(MethodCallExpression mc)
    {
      var elementType = mc.Method.GetGenericArguments()[0];
      TypeInfo type;
      if (!context.Model.Types.TryGetValue(elementType, out type))
        throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));
      var index = type.Indexes.PrimaryIndex;
      var entityExpression = EntityExpression.Create(type, 0, false);
      var itemProjector = new ItemProjectorExpression(entityExpression, index.GetQuery(), context);
      return new ProjectionExpression(typeof (IQueryable<>).MakeGenericType(elementType), itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
    }

    private Expression BuildSubqueryResult(ProjectionExpression subQuery, Type resultType)
    {
      if (state.Parameters.Length==0)
        throw Exceptions.InternalError(String.Format(Strings.ExUnableToBuildSubqueryResultForExpressionXStateContainsNoParameters, subQuery), Log.Instance);

      if (!resultType.IsOfGenericInterface(typeof (IEnumerable<>)))
        throw Exceptions.InternalError(String.Format(Strings.ExUnableToBuildSubqueryResultForExpressionXResultTypeIsNotIEnumerable, subQuery), Log.Instance);

      ApplyParameter applyParameter = context.GetApplyParameter(context.Bindings[state.Parameters[0]]);
      if (subQuery.Type!=resultType)
        subQuery = new ProjectionExpression(
          resultType,
          subQuery.ItemProjector,
          subQuery.TupleParameterBindings,
          subQuery.ResultType);
      return new SubQueryExpression(resultType, state.Parameters[0], false, subQuery, applyParameter);
    }

    private static IList<Expression> GetAnonymousArguments(Expression expression)
    {
      if (expression.NodeType==ExpressionType.New) {
        var newExpression = ((NewExpression) expression);
        IEnumerable<Expression> arguments = newExpression
          .Members
          .Select((methodInfo, index) => new {methodInfo.Name, Argument = newExpression.Arguments[index]})
          .OrderBy(a => a.Name)
          .Select(a => a.Argument);
        return arguments.ToList();
      }

      return expression
        .Type
        .GetProperties()
        .OrderBy(property => property.Name)
        .Select(p => Expression.MakeMemberAccess(expression, p))
        .Select(e => (Expression) e)
        .ToList();
    }

    protected override Expression VisitMemberInit(MemberInitExpression mi)
    {
      var newExpression = mi.NewExpression;
      var arguments = VisitNewExpressionArguments(newExpression);
      var bindings = VisitBindingList(mi.Bindings).Cast<MemberAssignment>();
      var constructorExpression = (ConstructorExpression) VisitNew(mi.NewExpression);
      foreach (var binding in bindings)
        constructorExpression.Bindings[binding.Member] = binding.Expression;
      return constructorExpression;
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private Expression GetMember(Expression expression, MemberInfo member, Expression sourceExpression)
    {
      MarkerType markerType;
      if (expression==null)
        return null;
      expression = expression.StripCasts();
      bool isMarker = expression.TryGetMarker(out markerType);
      expression = expression.StripMarkers();
      expression = expression.StripCasts();
      if (expression.IsAnonymousConstructor()) {
        var newExpression = (NewExpression) expression;
#if !NET40
        if (member.MemberType==MemberTypes.Property)
          member = ((PropertyInfo) member).GetGetMethod();
#endif
        int memberIndex = newExpression.Members.IndexOf(member);
        if (memberIndex < 0)
          throw new InvalidOperationException(
            string.Format(Strings.ExCouldNotGetMemberXFromExpression,
              member));
        Expression argument = Visit(newExpression.Arguments[memberIndex]);
        return isMarker
          ? new MarkerExpression(argument, markerType)
          : argument;
      }
      var extendedExpression = expression as ExtendedExpression;
      if (extendedExpression == null)
        return IsConditionalOrWellknown(expression)
          ? GetConditionalMember(expression, member, sourceExpression)
          : null;
      Expression result = null;
      Func<PersistentFieldExpression, bool> propertyFilter = f => f.Name==context.Domain.Handlers.NameBuilder.BuildFieldName((PropertyInfo) member);
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
        if (member.Name=="Key")
          return ((GroupingExpression) expression).KeyExpression;
        break;
      case ExtendedExpressionType.Constructor:
        if (!((ConstructorExpression) extendedExpression).Bindings.TryGetValue(member, out result))
          throw new InvalidOperationException(String.Format(Strings.ExMemberXOfTypeYIsNotInitializedCheckIfConstructorArgumentIsCorrectOrFieldInitializedThroughInitializer, member.Name, member.ReflectedType.Name));
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
        if (result==null) {
          EnsureEntityFieldsAreJoined(entityExpression);
          result = entityExpression.Fields.First(propertyFilter);
        }
        break;
      case ExtendedExpressionType.Field:
        if (isMarker && ((markerType & MarkerType.Single)==MarkerType.Single))
          throw new InvalidOperationException(String.Format(Strings.ExUseMethodXOnFirstInsteadOfSingle, sourceExpression.ToString(true), member.Name));
        return Expression.MakeMemberAccess(expression, member);
      case ExtendedExpressionType.EntityField:
        var entityFieldExpression = (EntityFieldExpression) expression;
        result = entityFieldExpression.Fields.FirstOrDefault(propertyFilter);
        if (result==null) {
          EnsureEntityReferenceIsJoined(entityFieldExpression);
          result = entityFieldExpression.Entity.Fields.First(propertyFilter);
        }
        break;
      }
      return isMarker
        ? new MarkerExpression(result, markerType)
        : result;
    }

    private Expression GetConditionalMember(Expression expression, MemberInfo member, Expression sourceExpression)
    {
      var ce = expression as ConditionalExpression;
      if (ce != null) {
        var ifTrue = GetConditionalMember(ce.IfTrue, member, sourceExpression);
        var ifFalse = GetConditionalMember(ce.IfFalse, member, sourceExpression);
        if (ifTrue == null || ifFalse == null)
          return null;
        return Expression.Condition(ce.Test,ifTrue,ifFalse);
      }
      if (expression.IsNull()) {
        var mt = member.MemberType;
        Type valueType;
        switch (mt)
        {
          case MemberTypes.Field:
            var fi = (FieldInfo)member;
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
      if (source.GetMemberType()!=MemberType.Entity)
        throw new NotSupportedException(Strings.ExAsOperatorSupportsEntityOnly);

      // Expression is already of requested type.
      var visitedSource = Visit(source);
      if (source.Type==targetType)
        return visitedSource;

      // Call convert to parent type.
      if (targetType.IsAssignableFrom(source.Type))
        return Visit(Expression.Convert(source, targetType));

      // Cast to subclass or interface.
      using (state.CreateScope()) {
        var targetTypeInfo = context.Model.Types[targetType];
        var parameter = state.Parameters[0];
        var entityExpression = visitedSource.StripCasts() as IEntityExpression;

        if (entityExpression==null)
          throw new InvalidOperationException(Strings.ExAsOperatorSupportsEntityOnly);

        // Replace original recordset. New recordset is left join with old recordset
        ProjectionExpression originalResultExpression = context.Bindings[parameter];
        var originalRecordset = originalResultExpression.ItemProjector.DataSource;
        int offset = originalRecordset.Header.Columns.Count;

        // Join primary index of target type
        IndexInfo joinedIndex = targetTypeInfo.Indexes.PrimaryIndex;
        var joinedRs = joinedIndex.GetQuery().Alias(context.GetNextAlias());
        IEnumerable<int> keySegment = entityExpression.Key.Mapping.GetItems();
        Pair<int>[] keyPairs = keySegment
          .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
          .ToArray();

        // Replace recordset.
        var joinedRecordQuery = originalRecordset.LeftJoin(joinedRs, keyPairs);
        var itemProjectorExpression = new ItemProjectorExpression(originalResultExpression.ItemProjector.Item,
          joinedRecordQuery,
          context);
        var projectionExpression = new ProjectionExpression(originalResultExpression.Type, itemProjectorExpression, originalResultExpression.TupleParameterBindings);
        context.Bindings.ReplaceBound(parameter, projectionExpression);

        // return new EntityExpression
        var result = EntityExpression.Create(context.Model.Types[targetType], offset, false);
        result.IsNullable = true;
        return result;
      }
    }

    private void EnsureEntityFieldsAreJoined(EntityExpression entityExpression)
    {
      ItemProjectorExpression itemProjector = entityExpression.OuterParameter==null
        ? context.Bindings[state.Parameters[0]].ItemProjector
        : context.Bindings[entityExpression.OuterParameter].ItemProjector;
      EnsureEntityFieldsAreJoined(entityExpression, itemProjector);
    }

    public void EnsureEntityFieldsAreJoined(EntityExpression entityExpression, ItemProjectorExpression itemProjector)
    {
      TypeInfo typeInfo = entityExpression.PersistentType;
      if (
        typeInfo.Fields.All(fieldInfo => entityExpression.Fields.Any(entityField => entityField.Name==fieldInfo.Name)))
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
      if (entityFieldExpression.Entity!=null)
        return;
      TypeInfo typeInfo = entityFieldExpression.PersistentType;
      IndexInfo joinedIndex = typeInfo.Indexes.PrimaryIndex;
      var joinedRs = joinedIndex.GetQuery().Alias(context.GetNextAlias());
      Segment<int> keySegment = entityFieldExpression.Mapping;
      Pair<int>[] keyPairs = keySegment.GetItems()
        .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
        .ToArray();
      ItemProjectorExpression originalItemProjector = entityFieldExpression.OuterParameter==null
        ? context.Bindings[state.Parameters[0]].ItemProjector
        : context.Bindings[entityFieldExpression.OuterParameter].ItemProjector;
      int offset = originalItemProjector.DataSource.Header.Length;
      var oldDataSource = originalItemProjector.DataSource;
      var newDataSource = entityFieldExpression.IsNullable
        ? originalItemProjector.DataSource.LeftJoin(joinedRs, keyPairs)
        : originalItemProjector.DataSource.Join(joinedRs, keyPairs);
      originalItemProjector.DataSource = newDataSource;
      entityFieldExpression.RegisterEntityExpression(offset);
      context.RebindApplyParameter(oldDataSource, newDataSource);
    }

    private static Expression MakeBinaryExpression(Expression previous, Expression left, Expression right,
      ExpressionType operationType, ExpressionType concatenationExpression)
    {
      var binaryExpression = operationType==ExpressionType.Equal
        ? Expression.Equal(left, right)
        : Expression.NotEqual(left, right);

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
      var itemToTupleConverter = ItemToTupleConverter.BuildConverter(itemType, value, translator.context.Model, sourceExpression);
      var rsHeader = new RecordSetHeader(itemToTupleConverter.TupleDescriptor, itemToTupleConverter.TupleDescriptor.Select(x => new SystemColumn(translator.context.GetNextColumnAlias(), 0, x)).Cast<Column>());
      var rawProvider = new RawProvider(rsHeader, itemToTupleConverter.GetEnumerable());
      var recordset = new StoreProvider(rawProvider);
      var itemProjector = new ItemProjectorExpression(itemToTupleConverter.Expression, recordset, translator.context);
      if (translator.state.JoinLocalCollectionEntity)
        itemProjector = EntityExpressionJoiner.JoinEntities(translator, itemProjector);
      return new ProjectionExpression(itemType, itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
    }

    private Expression BuildInterfaceExpression(MemberExpression ma)
    {
      var @interface = ma.Expression.Type;
      var property = (PropertyInfo)ma.Member;
      var implementors = context.Model.Types[@interface].GetImplementors(true);
      var fields = implementors
        .Select(im => im.UnderlyingType.GetProperty(property.Name, BindingFlags.Instance|BindingFlags.Public))
        .Concat(implementors
          .Select(im => im.UnderlyingType.GetProperty(string.Format("{0}.{1}", @interface.Name, property.Name), BindingFlags.Instance|BindingFlags.NonPublic)))
        .Where(f => f != null);

      return BuildExpression(ma, fields);
    }

    private Expression BuildHierarchyExpression(MemberExpression ma)
    {
      var ancestor = ma.Expression.Type;
      var property = (PropertyInfo)ma.Member;
      var descendants = context.Model.Types[ancestor].GetDescendants(true);
      var fields = descendants
        .Select(im => im.UnderlyingType.GetProperty(property.Name, BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic))
        .Where(f => f != null);

      return BuildExpression(ma, fields);
    }

    private Expression BuildExpression(MemberExpression ma, IEnumerable<PropertyInfo> fields)
    {
      object defaultValue = null;
      var propertyType = ((PropertyInfo)ma.Member).PropertyType;
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

    #endregion
  }
}