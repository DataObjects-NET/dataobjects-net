// Copyright (C) 2009 Xtensive LLC.
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
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Linq;
using Xtensive.Core.Parameters;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Materialization;
using Xtensive.Storage.Model;
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
      if (e.IsLocalCollection(context)) {
        Type itemType = e
          .Type
          .GetInterfaces()
          .AddOne(e.Type)
          .Single(type => type.IsGenericType && type.GetGenericTypeDefinition()==typeof (IEnumerable<>))
          .GetGenericArguments()[0];
        MethodInfo method = VisitLocalCollectionSequenceMethodInfo.MakeGenericMethod(itemType);
        return (ProjectionExpression) method.Invoke(this, new[] {e});
      }
      if (context.Evaluator.CanBeEvaluated(e)) {
        if (typeof (IQueryable).IsAssignableFrom(e.Type))
          return base.Visit(e);
        return context.ParameterExtractor.IsParameter(e)
          ? e
          : context.Evaluator.Evaluate(e);
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
          if (u.Type==u.Operand.Type || u.Type.IsAssignableFrom(u.Operand.Type))
            return base.VisitUnary(u);
          throw new NotSupportedException(
            String.Format(Strings.ExDowncastFromXToXNotSupportedUseOfTypeOrAsOperatorInstead, u.Operand.Type, u.Type));
        }
        break;
      }
      return base.VisitUnary(u);
    }

    protected override Expression VisitLambda(LambdaExpression le)
    {
      using (state.CreateLambdaScope(le)) {
        if (!state.BuildingProjection && le.Parameters.Count > 1)
          throw new InvalidOperationException(String.Format(Strings.ExLambdaXMustHaveOnlyOneParameter, le.ToString(true)));

        Expression body = Visit(le.Body);
        ParameterExpression parameter = le.Parameters[0];
        ProjectionExpression projection = context.Bindings[parameter];
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
      Expression left = Visit(binaryExpression.Left);
      Expression right = Visit(binaryExpression.Right);
      BinaryExpression resultBinaryExpression = Expression.MakeBinary(binaryExpression.NodeType,
        left,
        right,
        binaryExpression.IsLiftedToNull,
        binaryExpression.Method);

      if (binaryExpression.NodeType==ExpressionType.Equal
        || binaryExpression.NodeType==ExpressionType.NotEqual)
        return VisitBinaryRecursive(resultBinaryExpression);

      if (binaryExpression.NodeType==ExpressionType.ArrayIndex) {
        var newArrayExpression = left.StripCasts() as NewArrayExpression;
        var indexExpression = right.StripCasts() as ConstantExpression;
        if (newArrayExpression!=null && indexExpression!=null && indexExpression.Type==typeof (int))
          return newArrayExpression.Expressions[(int) indexExpression.Value];

        throw new NotSupportedException(String.Format(Strings.ExBinaryExpressionXOfTypeXIsNotSupported, binaryExpression.ToString(true), binaryExpression.NodeType));
      }

      return resultBinaryExpression;
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
          Func<IQueryable> lambda = Expression.Lambda<Func<IQueryable>>(ma).CachingCompile();
          IQueryable rootPoint = lambda();
          if (rootPoint!=null)
            return ConstructQueryable(rootPoint);
        }
      }
      else if (ma.Expression.NodeType==ExpressionType.Constant) {
        var rfi = ma.Member as FieldInfo;
        if (rfi!=null && (rfi.FieldType.IsGenericType && typeof (IQueryable).IsAssignableFrom(rfi.FieldType))) {
          Func<IQueryable> lambda = Expression.Lambda<Func<IQueryable>>(ma).CachingCompile();
          IQueryable rootPoint = lambda();
          if (rootPoint!=null)
            return ConstructQueryable(rootPoint);
        }
      }
      else if (ma.Expression.GetMemberType()==MemberType.Entity && ma.Member.Name!="Key")
        if (!context.Model.Types[ma.Expression.Type].Fields.Contains(ma.Member.Name))
          throw new NotSupportedException(Strings.ExNonpersistentFieldsAreNotSupported);
      Expression source;
      using (state.CreateScope()) {
        state.BuildingProjection = false;
        source = Visit(ma.Expression);
      }
      Expression result = GetMember(source, ma.Member);
      return result ?? base.VisitMemberAccess(ma);
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      // Visit Queryable extensions.
      if (mc.Method.DeclaringType==typeof (QueryableExtensions))
        if (mc.Method.Name==WellKnownMembers.QueryableJoinLeft.Name)
          return VisitJoinLeft(mc);
        else if (mc.Method.Name==WellKnownMembers.QueryableLock.Name)
          return VisitLock(mc);
        else
          throw new InvalidOperationException(String.Format(Strings.ExMethodCallExpressionXIsNotSupported, mc.ToString(true)));

      // Process local collections
      if (mc.Object.IsLocalCollection(context)) {
        // IList.Contains
        // List.Contains
        // Array.Contains
        ParameterInfo[] parameters = mc.Method.GetParameters();
        if (mc.Method.Name=="Contains" && parameters.Length==1)
          return VisitContains(mc.Object, mc.Arguments[0], false);

        // Enumerable.Contains
        // Enumerable.IndexOf
        // Enumerable.All
        // Enumerable.Any
        // Enumerable.Where 
        // etc... All synonims to queryable methods
      }

      return base.VisitMethodCall(mc);
    }

    protected override Expression VisitNew(NewExpression n)
    {
      if (n.Members==null) {
        if (n.IsGroupingExpression() ||
          n.IsSubqueryExpression() ||
            n.Type==typeof (TimeSpan) ||
              n.Type==typeof (DateTime))
          return base.VisitNew(n);
        throw new NotSupportedException(String.Format(Strings.ExTypeXIsNotSupportedInNewExpression, n.Type));
      }
      var arguments = new List<Expression>();
      for (int i = 0; i < n.Arguments.Count; i++) {
        Expression argument = n.Arguments[i];
        Expression body;
        using (state.CreateScope()) {
          state.CalculateExpressions = false;
          body = Visit(argument);
        }
        body = body.IsProjection()
          ? BuildSubqueryResult((ProjectionExpression) body, argument.Type)
          : ProcessProjectionElement(body);
        arguments.Add(body);
      }
      ParameterInfo[] constructorParameters = n.Constructor.GetParameters();
      for (int i = 0; i < arguments.Count; i++) {
        if (arguments[i].Type!=constructorParameters[i].ParameterType)
          arguments[i] = Expression.Convert(arguments[i], constructorParameters[i].ParameterType);
      }
      return Expression.New(n.Constructor, arguments, n.Members);
    }

    #region Private helper methods

    /// <exception cref="NotSupportedException"><c>NotSupportedException</c>.</exception>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    private Expression VisitBinaryRecursive(BinaryExpression binaryExpression)
    {
      if (context.Evaluator.CanBeEvaluated(binaryExpression))
        return context.ParameterExtractor.IsParameter(binaryExpression)
          ? (Expression) binaryExpression
          : context.Evaluator.Evaluate(binaryExpression);

      Expression left = binaryExpression.Left.StripCasts();
      Expression right = binaryExpression.Right.StripCasts();

      IList<Expression> leftExpressions;
      IList<Expression> rightExpressions;

      // Split left and right arguments to subexpressions.
      switch (left.GetMemberType()) {
      case MemberType.Key:
        var leftKeyExpression = left as KeyExpression;
        var rightKeyExpression = right as KeyExpression;
        if (leftKeyExpression==null && rightKeyExpression==null)
          throw new NotSupportedException(String.Format(Strings.ExBothLeftAndRightPartOfBinaryExpressionXAreNULLOrNotKeyExpression, binaryExpression));
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
          .Hierarchy
          .KeyInfo
          .Fields
          .Select(keyInfo => keyInfo.Key.ValueType);

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
        var leftStructureExpression = left as StructureExpression;
        var rightStructureExpression = right as StructureExpression;
        if (leftStructureExpression==null && rightStructureExpression==null)
          throw new NotSupportedException(String.Format(Strings.ExBothLeftAndRightPartOfBinaryExpressionXAreNULLOrNotStructureExpression, binaryExpression));

        StructureExpression structureExpression = (leftStructureExpression ?? rightStructureExpression);
        leftExpressions = GetStructureFields(left, structureExpression.Fields, structureExpression.Type);
        rightExpressions = GetStructureFields(right, structureExpression.Fields, structureExpression.Type);
        break;
      case MemberType.Array:
        // Special case. ArrayIndex expression. 
        if (binaryExpression.NodeType==ExpressionType.ArrayIndex) {
          Expression arrayExpression = Visit(left);
          Expression arrayIndex = Visit(right);
          return Expression.ArrayIndex(arrayExpression, arrayIndex);
        }

        // If array compares to null use standart routine. 
        if (right.NodeType==ExpressionType.Constant
          && ((ConstantExpression) right).Value==null)
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
        throw Exceptions.InternalError(Strings.ExMistmatchCountOfLeftAndRightExpressions, LogTemplate<Log>.Instance);

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
        Expression visitedResultExpression = VisitBinaryRecursive(pairExpression);

        // Combine expression chain with AndAlso
        resultExpression = resultExpression==null
          ? visitedResultExpression
          : Expression.AndAlso(resultExpression, visitedResultExpression);
      }

      // Return result.
      return resultExpression;
    }

    private static IList<Expression> GetStructureFields(
      Expression expression,
      IEnumerable<PersistentFieldExpression> structureFieldTypes,
      Type structureType)
    {
      expression = expression.StripCasts();
      if (expression is StructureExpression)
        return ((StructureExpression) expression)
          .Fields
          .Select(e => (Expression) e)
          .ToList();

      ConstantExpression nullExpression = Expression.Constant(null, structureType);
      BinaryExpression isNullExpression = Expression.Equal(expression, nullExpression);

      var result = new List<Expression>();
      foreach (PersistentFieldExpression fieldExpression in structureFieldTypes) {
        Type nullableType = fieldExpression.Type.ToNullable();
        var item = (Expression) Expression.Condition(
          isNullExpression,
          Expression.Constant(null, nullableType),
          Expression.Convert(
            Expression.MakeMemberAccess(Expression.Convert(expression, structureType),
              fieldExpression.UnderlyingProperty),
            nullableType));
        result.Add(item);
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

        keyExpression = Expression.Condition(
          isNullExpression,
          Expression.Constant(null, typeof (Key)),
          Expression.MakeMemberAccess(expression, WellKnownMembers.IEntityKey));
      }
      return GetKeyFields(keyExpression, keyFieldTypes);
    }

    private static IList<Expression> GetKeyFields(Expression expression, IEnumerable<Type> keyFieldTypes)
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
      MemberExpression keyTupleExpression = Expression.MakeMemberAccess(expression, WellKnownMembers.KeyValue);

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
          && reduceCastBody.NodeType!=ExpressionType.ArrayIndex) {
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

    private Expression ConstructQueryable(IQueryable rootPoint)
    {
      var constExpression = rootPoint.Expression as ConstantExpression;
      if (constExpression!=null && constExpression.Value==rootPoint) {
        // Special case. Constructing Queryable<T>.
        Type elementType = rootPoint.ElementType;
        TypeInfo type;
        if (!context.Model.Types.TryGetValue(elementType, out type))
          throw new NotSupportedException(String.Format(Strings.ExTypeNotFoundInModel, elementType.FullName));

        IndexInfo index = type.Indexes.PrimaryIndex;
        EntityExpression entityExpression = EntityExpression.Create(type, 0, false);
        var itemProjector = new ItemProjectorExpression(entityExpression, IndexProvider.Get(index).Result, context);
        return new ProjectionExpression(typeof (IQueryable<>).MakeGenericType(elementType), itemProjector, new Dictionary<Parameter<Tuple>, Tuple>());
      }
      return VisitSequence(rootPoint.Expression);
    }

    private Expression BuildSubqueryResult(ProjectionExpression subQuery, Type resultType)
    {
      if (state.Parameters.Length==0)
        throw Exceptions.InternalError(String.Format(Strings.ExUnableToBuildSubqueryResultForExpressionXStateContainsNoParameters, subQuery), LogTemplate<Log>.Instance);

      if (!resultType.IsOfGenericInterface(typeof (IEnumerable<>)))
        throw Exceptions.InternalError(String.Format(Strings.ExUnableToBuildSubqueryResultForExpressionXResultTypeIsNotIEnumerable, subQuery), LogTemplate<Log>.Instance);

      ApplyParameter applyParameter = context.GetApplyParameter(context.Bindings[state.Parameters[0]]);
      if (subQuery.Type!=resultType)
        subQuery = new ProjectionExpression(resultType, subQuery.ItemProjector, subQuery.TupleParameterBindings, subQuery.ResultType);
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

    private Expression GetMember(Expression expression, MemberInfo member)
    {
      MarkerType markerType;
      expression = expression.StripCasts();
      bool isMarker = expression.TryGetMarker(out markerType);
      expression = expression.StripMarkers();
      expression = expression.StripCasts();
      if (expression.IsGroupingExpression() && member.Name=="Key")
        return ((GroupingExpression) expression).KeyExpression;
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
      case ExtendedExpressionType.Structure:
        var persistentExpression = (IPersistentExpression) expression;
        result = persistentExpression.Fields.First(propertyFilter);
        break;
      case ExtendedExpressionType.Entity:
        var entityExpression = (EntityExpression) expression;
        result = entityExpression.Fields.FirstOrDefault(propertyFilter);
        if (result==null) {
          EnsureEntityFieldsAreJoined(entityExpression);
          result = entityExpression.Fields.First(propertyFilter);
        }
        break;
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
        RecordSet joinedRecordSet = originalRecordset.JoinLeft(joinedRs, JoinAlgorithm.Default, keyPairs);
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
      TypeInfo typeInfo = entityExpression.PersistentType;
      if (
        typeInfo.Fields.All(fieldInfo => entityExpression.Fields.Any(entityField => entityField.Name==fieldInfo.Name)))
        return; // All fields are already 
      IndexInfo joinedIndex = typeInfo.Indexes.PrimaryIndex;
      RecordSet joinedRs = IndexProvider.Get(joinedIndex).Result.Alias(context.GetNextAlias());
      Segment<int> keySegment = entityExpression.Key.Mapping;
      Pair<int>[] keyPairs = keySegment.GetItems()
        .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
        .ToArray();
      ItemProjectorExpression originalItemProjector = entityExpression.OuterParameter==null
        ? context.Bindings[state.Parameters[0]].ItemProjector
        : context.Bindings[entityExpression.OuterParameter].ItemProjector;
      int offset = originalItemProjector.DataSource.Header.Length;
      originalItemProjector.DataSource = originalItemProjector.DataSource.Join(joinedRs, JoinAlgorithm.Default, keyPairs);
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
      originalItemProjector.DataSource = originalItemProjector.DataSource.JoinLeft(joinedRs, JoinAlgorithm.Default, keyPairs);
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

    #endregion
  }
}