// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Linq.Materialization;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using FieldInfo=System.Reflection.FieldInfo;

namespace Xtensive.Storage.Linq
{
  internal sealed partial class Translator
  {
    protected override Expression VisitTypeIs(TypeBinaryExpression tb)
    {
      Type expressionType = tb.Expression.Type;
      Type operandType = tb.TypeOperand;
      if (operandType.IsAssignableFrom(expressionType))
        return Expression.Constant(true);

      // Structure
      if (tb.Expression.GetMemberType()==MemberType.Structure
        && typeof (Structure).IsAssignableFrom(operandType))
        return Expression.Constant(false);

      // Entity
      if (tb.Expression.GetMemberType()==MemberType.Entity
        && typeof (IEntity).IsAssignableFrom(operandType)) {
        TypeInfo typeInfo = context.Model.Types[operandType];
        IEnumerable<int> typeIds = typeInfo.GetDescendants().AddOne(typeInfo).Select(ti => ti.TypeId);
        MemberExpression memberExpression = Expression.Property(tb.Expression, WellKnown.TypeIdFieldName);
        Expression boolExpression = null;
        foreach (int typeId in typeIds)
          boolExpression = MakeBinaryExpression(boolExpression, memberExpression, Expression.Constant(typeId),
            ExpressionType.Equal, ExpressionType.OrElse);

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
              || !typeof (Entity).IsAssignableFrom(u.Operand.Type))
            return base.VisitUnary(u);
          throw new InvalidOperationException(String.Format(Strings.ExDowncastFromXToXNotSupportedUseOfTypeOrAsOperatorInstead, u, u.Operand.Type, u.Type));
        }
        break;
      }
      return u.Type == typeof(IQueryable) 
        ? Visit(u.Operand) 
        : base.VisitUnary(u);
    }

    protected override Expression VisitLambda(LambdaExpression le)
    {
      using (state.CreateLambdaScope(le)) {
        Expression body = Visit(le.Body);
        ParameterExpression parameter = le.Parameters[0];
        ProjectionExpression projection = context.Bindings[parameter];
        if (body.NodeType!=ExpressionType.New
          && body.NodeType!=ExpressionType.MemberInit
            && !(body.NodeType==ExpressionType.Constant && state.BuildingProjection))
          body = body.IsProjection()
            ? BuildSubqueryResult((ProjectionExpression) body, le.Body.Type)
            : ProcessProjectionElement(body);
        if (state.CalculatedColumns.Count > 0) {
          RecordSet dataSource = projection.ItemProjector.DataSource.Calculate(
            !state.BuildingProjection,
            state.CalculatedColumns.ToArray());
          var itemProjector = new ItemProjectorExpression(body, dataSource, context);
          context.Bindings.ReplaceBound(parameter, new ProjectionExpression(
            projection.Type,
            state.BuildingProjection
              ? itemProjector
              : projection.ItemProjector.Remap(dataSource, 0),
            projection.TupleParameterBindings,
            projection.ResultType));
          return itemProjector;
        }
        return new ItemProjectorExpression(body, projection.ItemProjector.DataSource, context);
      }
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
      var left = Visit(binaryExpression.Left);
      var right = Visit(binaryExpression.Right);
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
      left = left.NodeType == ExpressionType.Convert
               ? Expression.Convert(((UnaryExpression)left).Operand, underlyingType)
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
      var customCompiler = context.CustomCompilerProvider.GetCompiler(ma.Member);
      if (customCompiler!=null)
        return Visit(customCompiler.Invoke(ma.Expression, ArrayUtils<Expression>.EmptyArray));

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
          if (rootPoint != null)
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
      else if (ma.Expression.GetMemberType()==MemberType.Entity && ma.Member.Name!="Key")
        if (!context.Model.Types[ma.Expression.Type].Fields.Contains(ma.Member.Name))
          throw new NotSupportedException(String.Format(Strings.ExFieldMustBePersistent, ma.ToString(true)));
      Expression source;
      using (state.CreateScope()) {
        state.BuildingProjection = false;
        source = Visit(ma.Expression);
      }
      
      var result = GetMember(source, ma.Member, ma);
      return result ?? base.VisitMemberAccess(ma);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      var customCompiler = context.CustomCompilerProvider.GetCompiler(mc.Method);
      if (customCompiler!=null)
        return Visit(customCompiler.Invoke(mc.Object, mc.Arguments.ToArray()));

      // Visit Query.
      if (mc.Method.DeclaringType==typeof (Query)) {
        // Query.All<T>
        if (mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition() == WellKnownMembers.Query.All)
          return ConstructQueryable(mc);
        // Query.FreeText<T>
        if (mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition().In(WellKnownMembers.Query.FreeTextString, WellKnownMembers.Query.FreeTextExpression))
          return CosntructFreeTextQueryRoot(mc.Method.GetGenericArguments()[0], mc.Arguments[0]);
        // Query.Single<T> & Query.SingleOrDefault<T>
        if (mc.Method.IsGenericMethod && mc.Method.GetGenericMethodDefinition().In(
          WellKnownMembers.Query.SingleKey,
          WellKnownMembers.Query.SingleOrDefaultKey))
          return VisitQuerySingle(mc);
        throw new InvalidOperationException(String.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
      }

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
        else
          throw new InvalidOperationException(String.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
      // Visit Collection extensions
      if (mc.Method.DeclaringType == typeof(CollectionExtensions))
        if (mc.Method.Name == WellKnownMembers.Collection.ExtensionContainsAny.Name)
          return VisitContainsAny(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), mc.Method.GetGenericArguments()[0]);
        else if (mc.Method.Name == WellKnownMembers.Collection.ExtensionContainsAll.Name)
          return VisitContainsAll(mc.Arguments[0], mc.Arguments[1], context.IsRoot(mc), mc.Method.GetGenericArguments()[0]);
        else if (mc.Method.Name == WellKnownMembers.Collection.ExtensionContainsNone.Name)
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
      if (result != mc && result.NodeType == ExpressionType.Call) {
        var visitedMethodCall = (MethodCallExpression) result;
        if (visitedMethodCall.Arguments.Any(arg => arg.IsProjection()))
          throw new InvalidOperationException(String.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));
      }
      return result;
    }

    private Expression CosntructFreeTextQueryRoot(Type elementType, Expression searchCriteria)
    {
      TypeInfo type;
      if (!context.Model.Types.TryGetValue(elementType, out type))
        throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));
      var fullTextIndex = type.FullTextIndex;
      if (fullTextIndex==null)
        throw new InvalidOperationException(String.Format(Strings.ExEntityDoesNotHasFullTextIndex, elementType.FullName));

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
      var rankExpression = ColumnExpression.Create(typeof (double), fullFeatured ? entityExpression.Fields.Max(field=>field.Mapping.Offset + field.Mapping.Length) : entityExpression.Key.Mapping.Length);
      var freeTextExpression = new FreeTextExpression(fullTextIndex, entityExpression, rankExpression, null);
      var dataSource = new FreeTextProvider(fullTextIndex, compiledParameter, context.GetNextColumnAlias(), fullFeatured).Result;
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
      List<Expression> arguments = VisitNewExpressionArguments(newExpression);
      if (newExpression.IsAnonymousConstructor()) {
        return newExpression.Members==null
          ? Expression.New(newExpression.Constructor, arguments)
          : Expression.New(newExpression.Constructor, arguments, newExpression.Members);
      }
      var constructorParameters = newExpression.Constructor.GetParameters();
      if (constructorParameters.Length!=arguments.Count)
        throw Exceptions.InternalError(Strings.ExInvalidNumberOfParametersInNewExpression, Log.Instance);
      ISet<MemberInfo> duplicateMembers = new SetSlim<MemberInfo>();
      var bindings = new Dictionary<MemberInfo, Expression>();
      for (int i = 0; i < constructorParameters.Length; i++) {
        int parameterIndex = i;
        var members = newExpression
          .Type
          .GetMembers()
          .Where(memberInfo => (memberInfo.MemberType==MemberTypes.Field || memberInfo.MemberType==MemberTypes.Property)
            && String.Equals(memberInfo.Name, constructorParameters[parameterIndex].Name, StringComparison.InvariantCultureIgnoreCase))
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
      switch (left.GetMemberType()) {
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
        Expression leftEntityExpression = (Expression) (left as EntityExpression) ?? left as EntityFieldExpression;
        Expression rightEntityExpression = (Expression) (right as EntityExpression) ?? right as EntityFieldExpression;
        if (leftEntityExpression==null && rightEntityExpression==null)
          throw new NotSupportedException(String.Format(Strings.ExBothLeftAndRightPartOfBinaryExpressionXAreNULLOrNotEntityExpressionEntityFieldExpression, binaryExpression));


        IEnumerable<Type> keyFieldTypes = context
          .Model
          .Types[(leftEntityExpression ?? rightEntityExpression).Type]
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
          Expression arrayExpression = Visit(left);
          Expression arrayIndex = Visit(right);
          return Expression.ArrayIndex(arrayExpression, arrayIndex);
        }

        // If array compares to null use standart routine. 
        if ((rightIsConstant && ExpressionEvaluator.Evaluate(right).Value==null)
          || (rightIsConstant && ExpressionEvaluator.Evaluate(right).Value==null)
          || (right.Type==typeof(byte[]) && (left is FieldExpression || left is ColumnExpression || right is FieldExpression || right is ColumnExpression)))
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

      if (leftExpressions.Count!=rightExpressions.Count
        || leftExpressions.Count==0)
        throw Exceptions.InternalError(Strings.ExMistmatchCountOfLeftAndRightExpressions, Log.Instance);

      // Combine new binary expression from subexpression pairs.
      Expression resultExpression = null;
      for (int i = 0; i < leftExpressions.Count; i++) {
        BinaryExpression pairExpression;
        Expression leftItem = leftExpressions[i];
        Expression rightItem = rightExpressions[i];
        bool leftIsNullable = leftItem.Type.IsNullable();
        bool rightIsNullable = rightItem.Type.IsNullable();
        leftItem = leftIsNullable
          ? leftItem
          : leftItem.LiftToNullable();
        rightItem = rightIsNullable
          ? rightItem
          : rightItem.LiftToNullable();

//        if (leftIsNullable ^ rightIsNullable) {
//          leftItem = leftIsNullable
//            ? leftItem
//            : leftItem.LiftToNullable();
//          rightItem = rightIsNullable
//            ? rightItem
//            : rightItem.LiftToNullable();
//        }
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
        Expression visitedResultExpression = VisitBinaryRecursive(pairExpression, originalBinaryExpression);

        // Combine expression chain with AndAlso
        resultExpression = resultExpression==null
          ? visitedResultExpression
          : Expression.AndAlso(resultExpression, visitedResultExpression);
      }

      // Return result.
      return resultExpression;
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
      else {
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
      Type originalBodyType = body.Type;
      Expression reduceCastBody = body.StripCasts();
      if (state.CalculateExpressions
        && reduceCastBody.GetMemberType()==MemberType.Unknown
          && reduceCastBody.NodeType!=ExpressionType.ArrayIndex
            && (ExtendedExpressionType) reduceCastBody.NodeType!=ExtendedExpressionType.Constructor
              && reduceCastBody.NodeType!=ExpressionType.New) {
        if (body.Type.IsEnum)
          body = Expression.Convert(body, Enum.GetUnderlyingType(body.Type));
        UnaryExpression convertExpression = Expression.Convert(body, typeof (object));

        LambdaExpression calculator = ExpressionMaterializer.MakeLambda(convertExpression, context);
        var ccd = new CalculatedColumnDescriptor(context.GetNextColumnAlias(), body.Type,
          (Expression<Func<Tuple, object>>) calculator);
        state.CalculatedColumns.Add(ccd);
        ParameterExpression parameter = state.Parameters[0];
        int position = context.Bindings[parameter].ItemProjector.DataSource.Header.Length +
          state.CalculatedColumns.Count - 1;
        body = ColumnExpression.Create(originalBodyType, position);
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
      var itemProjector = new ItemProjectorExpression(entityExpression, IndexProvider.Get(index).Result, context);
      return new ProjectionExpression(typeof (IQueryable<>).MakeGenericType(elementType), itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());

//      var constExpression = rootPoint.Expression as ConstantExpression;
//      if (constExpression!=null && constExpression.Value==rootPoint) {
//        Type elementType = rootPoint.ElementType;
//        TypeInfo type;
//        if (!context.Model.Types.TryGetValue(elementType, out type))
//          throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));
//
//        IndexInfo index = type.Indexes.PrimaryIndex;
//        EntityExpression entityExpression = EntityExpression.Create(type, 0, false);
//        var itemProjector = new ItemProjectorExpression(entityExpression, IndexProvider.Get(index).Result, context);
//        return new ProjectionExpression(typeof (IQueryable<>).MakeGenericType(elementType), itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
//      }
//      return VisitSequence(rootPoint.Expression);
    }

//    private Expression ConstructQueryable(IQueryable rootPoint)
//    {
      // Special case. Constructing Queryable<T>.
//      var constExpression = rootPoint.Expression as ConstantExpression;
//      if (constExpression!=null && constExpression.Value==rootPoint) {
//        Type elementType = rootPoint.ElementType;
//        TypeInfo type;
//        if (!context.Model.Types.TryGetValue(elementType, out type))
//          throw new InvalidOperationException(String.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));
//
//        IndexInfo index = type.Indexes.PrimaryIndex;
//        EntityExpression entityExpression = EntityExpression.Create(type, 0, false);
//        var itemProjector = new ItemProjectorExpression(entityExpression, IndexProvider.Get(index).Result, context);
//        return new ProjectionExpression(typeof (IQueryable<>).MakeGenericType(elementType), itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
//      }
//      return VisitSequence(rootPoint.Expression);
//    }

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
      var constructor = mi.NewExpression.Constructor;
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
      if (expression == null)
        return null;
      expression = expression.StripCasts();
      bool isMarker = expression.TryGetMarker(out markerType);
      expression = expression.StripMarkers();
      expression = expression.StripCasts();
      if (expression.IsAnonymousConstructor()) {
        var newExpression = (NewExpression) expression;
        if (member.MemberType==MemberTypes.Property)
          member = ((PropertyInfo) member).GetGetMethod();
        int memberIndex = newExpression.Members.IndexOf(member);
        if (memberIndex < 0)
          throw new InvalidOperationException(
            string.Format(Strings.ExCouldNotGetMemberXFromExpression,
              member));
        Expression argument = newExpression.Arguments[memberIndex];
        return isMarker
          ? new MarkerExpression(argument, markerType)
          : argument;
      }
      var extendedExpression = expression as ExtendedExpression;
      if (extendedExpression==null)
        return null;
      Expression result = null;
      Func<PersistentFieldExpression, bool> propertyFilter = f => f.Name==member.Name;
      switch (extendedExpression.ExtendedType) {
      case ExtendedExpressionType.FreeText:
        switch (member.Name) {
        case "Rank":
          return ((FreeTextExpression) expression).RankExpression;
        case "Entity":
          return ((FreeTextExpression) expression).EntityExpression;
        }
        break;
      case ExtendedExpressionType.Grouping:
        if (member.Name=="Key")
          return ((GroupingExpression) expression).KeyExpression;
        break;
      case ExtendedExpressionType.Constructor:
        if (!((ConstructorExpression) extendedExpression).Bindings.TryGetValue(member, out result))
          throw new InvalidOperationException(String.Format(Strings.ExMemberXOfTypeYIsNotInitializedCheckIfConstructorArgumentIsCorrectOrFieldInitializedThroughInitializer, member.Name, member.ReflectedType.Name));
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
        var fieldExpression = (FieldExpression) expression;
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
      if (state.BuildingProjection && result is EntityFieldExpression) {
        var entityFieldExpression = (EntityFieldExpression) result;
        EnsureEntityReferenceIsJoined(entityFieldExpression);
        result = entityFieldExpression.Entity;
      }
      return isMarker
        ? new MarkerExpression(result, markerType)
        : result;
    }

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private Expression VisitTypeAs(Expression source, Type targetType)
    {
      if (source.GetMemberType()!=MemberType.Entity)
        throw new NotSupportedException(Strings.ExAsOperatorSupportsEntityOnly);

      // Expression is already of requested type.
      Expression visitedSource = Visit(source);
      if (source.Type==targetType)
        return visitedSource;

      // Call convert to parent type.
      if (!targetType.IsSubclassOf(source.Type))
        return Visit(Expression.Convert(source, targetType));

      // Cast to subclass.
      using (state.CreateScope()) {
        TypeInfo targetTypeInfo = context.Model.Types[targetType];
        ParameterExpression parameter = state.Parameters[0];
        var entityExpression = visitedSource.StripCasts() as IEntityExpression;

        if (entityExpression==null)
          throw new InvalidOperationException(Strings.ExAsOperatorSupportsEntityOnly);

        // Replace original recordset. New recordset is left join with old recordset
        ProjectionExpression originalResultExpression = context.Bindings[parameter];
        RecordSet originalRecordset = originalResultExpression.ItemProjector.DataSource;
        int offset = originalRecordset.Header.Columns.Count;

        // Join primary index of target type
        IndexInfo joinedIndex = targetTypeInfo.Indexes.PrimaryIndex;
        RecordSet joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
        IEnumerable<int> keySegment = entityExpression.Key.Mapping.GetItems();
        Pair<int>[] keyPairs = keySegment
          .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
          .ToArray();

        // Replace recordset.
        RecordSet joinedRecordSet = originalRecordset.LeftJoin(joinedRs, JoinAlgorithm.Default, keyPairs);
        var itemProjectorExpression = new ItemProjectorExpression(originalResultExpression.ItemProjector.Item,
          joinedRecordSet,
          context);
        var projectionExpression = new ProjectionExpression(originalResultExpression.Type, itemProjectorExpression, originalResultExpression.TupleParameterBindings);
        context.Bindings.ReplaceBound(parameter, projectionExpression);

        // return new EntityExpression
        EntityExpression result = EntityExpression.Create(context.Model.Types[targetType], offset, false);
        return result;
      }
    }

    private void EnsureEntityFieldsAreJoined(EntityExpression entityExpression)
    {
      ItemProjectorExpression itemProjector = entityExpression.OuterParameter==null
        ? context.Bindings[state.Parameters[0]].ItemProjector
        : context.Bindings[entityExpression.OuterParameter].ItemProjector;
      EnsureEntityFieldsAreJoined(entityExpression, itemProjector, false);
    }

    public static void EnsureEntityFieldsAreJoined(EntityExpression entityExpression, ItemProjectorExpression itemProjector, bool leftJoin)
    {
      TypeInfo typeInfo = entityExpression.PersistentType;
      if (
        typeInfo.Fields.All(fieldInfo => entityExpression.Fields.Any(entityField => entityField.Name==fieldInfo.Name)))
        return; // All fields are already joined
      IndexInfo joinedIndex = typeInfo.Indexes.PrimaryIndex;
      RecordSet joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(itemProjector.Context.GetNextAlias());
      Segment<int> keySegment = entityExpression.Key.Mapping;
      Pair<int>[] keyPairs = keySegment.GetItems()
        .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
        .ToArray();
      int offset = itemProjector.DataSource.Header.Length;
      itemProjector.DataSource =
        leftJoin
          ? itemProjector.DataSource.LeftJoin(joinedRs, JoinAlgorithm.Default, keyPairs)
          : itemProjector.DataSource.Join(joinedRs, JoinAlgorithm.Default, keyPairs);
      EntityExpression.Fill(entityExpression, offset);
    }

    private void EnsureEntityReferenceIsJoined(EntityFieldExpression entityFieldExpression)
    {
      if (entityFieldExpression.Entity!=null)
        return;
      TypeInfo typeInfo = entityFieldExpression.PersistentType;
      IndexInfo joinedIndex = typeInfo.Indexes.PrimaryIndex;
      RecordSet joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
      Segment<int> keySegment = entityFieldExpression.Mapping;
      Pair<int>[] keyPairs = keySegment.GetItems()
        .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
        .ToArray();
      ItemProjectorExpression originalItemProjector = entityFieldExpression.OuterParameter==null
        ? context.Bindings[state.Parameters[0]].ItemProjector
        : context.Bindings[entityFieldExpression.OuterParameter].ItemProjector;
      int offset = originalItemProjector.DataSource.Header.Length;
      originalItemProjector.DataSource = originalItemProjector.DataSource.LeftJoin(joinedRs, JoinAlgorithm.Default, keyPairs);
      entityFieldExpression.RegisterEntityExpression(offset);
    }

    private static Expression MakeBinaryExpression(Expression previous, Expression left, Expression right,
      ExpressionType operationType, ExpressionType concatenationExpression)
    {
      BinaryExpression newExpression = operationType==ExpressionType.Equal
        ? Expression.Equal(left, right)
        : Expression.NotEqual(left, right);

      if (previous==null)
        return newExpression;

      switch (concatenationExpression) {
      case ExpressionType.AndAlso:
        return Expression.AndAlso(previous, newExpression);
      case ExpressionType.OrElse:
        return Expression.OrElse(previous, newExpression);
      default:
        throw new ArgumentOutOfRangeException("concatenationExpression");
      }
    }

    private static ProjectionExpression CreateLocalCollectionProjectionExpression(Type itemType, object value, Translator translator, Expression sourceExpression)
    {
      var itemToTupleConverter = ItemToTupleConverter.BuildConverter(itemType, value, translator.context.Model, sourceExpression);
      var rsHeader = new RecordSetHeader(itemToTupleConverter.TupleDescriptor, itemToTupleConverter.TupleDescriptor.Select(x => new SystemColumn(translator.context.GetNextColumnAlias(), 0, x)).Cast<Column>());
      var rawProvider = new RawProvider(rsHeader, itemToTupleConverter.GetEnumerable());
      var recordset = new StoreProvider(rawProvider).Result;
      var itemProjector = new ItemProjectorExpression(itemToTupleConverter.Expression, recordset, translator.context);
      if (translator.state.JoinLocalCollectionEntity)
        itemProjector = EntityExpressionJoiner.JoinEntities(itemProjector);
      return new ProjectionExpression(itemType, itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
    }

    #endregion
  }
}