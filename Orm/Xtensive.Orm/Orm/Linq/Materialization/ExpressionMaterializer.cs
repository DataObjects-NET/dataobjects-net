// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.21

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Parameters;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;
using System.Reflection;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Linq.Rewriters;
using Xtensive.Collections;
using Xtensive.Reflection;

namespace Xtensive.Orm.Linq.Materialization
{
  [Serializable]
  internal class ExpressionMaterializer : PersistentExpressionVisitor
  {
    private static readonly MethodInfo BuildPersistentTupleMethodInfo;
    private static readonly MethodInfo GetTupleSegmentMethodInfo;
    private readonly TranslatorContext context;
    private readonly ParameterExpression tupleParameter;
    private readonly ParameterExpression itemMaterializationContextParameter;
    private readonly Dictionary<IEntityExpression, int> entityRegistry = new Dictionary<IEntityExpression, int>();
    private readonly HashSet<Parameter<Tuple>> tupleParameters;

    #region Public static methods

    public static LambdaExpression MakeLambda(Expression expression, TranslatorContext context)
    {
      var tupleParameter = Expression.Parameter(typeof (Tuple), "tuple");
      var visitor = new ExpressionMaterializer(tupleParameter, context, null, EnumerableUtils<Parameter<Tuple>>.Empty);
      var processedExpression = OwnerRemover.RemoveOwner(expression);
      return FastExpression.Lambda(visitor.Visit(processedExpression), tupleParameter);
    }

    public static MaterializationInfo MakeMaterialization(ItemProjectorExpression projector, TranslatorContext context, 
      IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      var tupleParameter = Expression.Parameter(typeof (Tuple), "tuple");
      var materializationContextParameter = Expression.Parameter(typeof (ItemMaterializationContext), "mc");
      var visitor = new ExpressionMaterializer(tupleParameter, context, materializationContextParameter, tupleParameters);
      var lambda = FastExpression.Lambda(visitor.Visit(projector.Item), tupleParameter, materializationContextParameter);
      var count = visitor.entityRegistry.Count;
      return new MaterializationInfo(count, lambda);
    }

    #endregion

    #region Visitor methods overrsides

    protected override Expression VisitFreeTextExpression(FullTextExpression expression)
    {
      var rankMaterializer = Visit(expression.RankExpression);
      var entityMaterializer = Visit(expression.EntityExpression);
      var constructorInfo = typeof (FullTextMatch<>)
        .MakeGenericType(expression.EntityExpression.Type)
        .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, new[] {typeof (double), expression.EntityExpression.Type});

      return Expression.New(
        constructorInfo,
        rankMaterializer,
        entityMaterializer);
    }

    protected override Expression VisitMarker(MarkerExpression expression)
    {
      var target = expression.Target;
      var processedTarget = Visit(target);
      if (expression.MarkerType!=MarkerType.None && (expression.MarkerType & MarkerType.Default)==MarkerType.None) {
        if (itemMaterializationContextParameter==null)
          return processedTarget;
        var columns = ColumnGatherer.GetColumns(target, ColumnExtractionModes.Distinct | ColumnExtractionModes.Ordered).ToArray();
        var sequenceCheck = Expression.Call(MaterializationHelper.IsNullMethodInfo, tupleParameter, Expression.Constant(columns));
        var throwException = Expression.Convert(Expression.Call(MaterializationHelper.ThrowEmptySequenceExceptionMethodInfo), target.Type);
        return Expression.Condition(sequenceCheck, throwException, processedTarget);
      }
      return processedTarget;
    }

    protected override Expression VisitGroupingExpression(GroupingExpression groupingExpression)
    {
      // 1. Prepare subquery parameters.
      Parameter<Tuple> parameterOfTuple;
      Type elementType;
      ProjectionExpression projection;
      var translatedQuery = PrepareSubqueryParameters(groupingExpression, out parameterOfTuple, out elementType, out projection);

      // 2. Create constructor
      var keyType = groupingExpression.KeyExpression.Type;
      var keyMaterializer = Visit(groupingExpression.KeyExpression);
      var groupingCtor = typeof (Grouping<,>)
        .MakeGenericType(keyType, elementType)
        .GetConstructor(new[] {typeof (ProjectionExpression), typeof (TranslatedQuery), typeof (Parameter<Tuple>), typeof (Tuple), keyType, typeof (ItemMaterializationContext)});

      // 3. Create result expression.
      var resultExpression = Expression.New(
        groupingCtor,
        Expression.Constant(projection),
        Expression.Constant(translatedQuery),
        Expression.Constant(parameterOfTuple),
        tupleParameter,
        keyMaterializer,
        itemMaterializationContextParameter);

      // 4. Result must be IGrouping<,> instead of Grouping<,>. Convert result expression.
      return Expression.Convert(resultExpression, groupingExpression.Type);
    }

    protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
    {
      // 1. Prepare subquery parameters.
      Parameter<Tuple> parameterOfTuple;
      Type elementType;
      ProjectionExpression projection;
      TranslatedQuery translatedQuery = PrepareSubqueryParameters(subQueryExpression, out parameterOfTuple, out elementType, out projection);

      // 2. Create constructor
      var subQueryCtor = typeof (SubQuery<>)
        .MakeGenericType(elementType)
        .GetConstructor(new[] {typeof (ProjectionExpression), typeof (TranslatedQuery), typeof (Parameter<Tuple>), typeof (Tuple), typeof (ItemMaterializationContext)});

      // 3. Create result expression.
      var resultExpression = Expression.New(
        subQueryCtor,
        Expression.Constant(projection),
        Expression.Constant(translatedQuery),
        Expression.Constant(parameterOfTuple),
        tupleParameter,
        itemMaterializationContextParameter);

      return Expression.Convert(resultExpression, subQueryExpression.Type);
    }

    private TranslatedQuery PrepareSubqueryParameters(SubQueryExpression subQueryExpression, out Parameter<Tuple> parameterOfTuple, out Type elementType, out ProjectionExpression projection)
    {
      // 1. Rewrite recordset and ItemProjector to parameter<tuple>
      var subqueryTupleParameter = context.GetTupleParameter(subQueryExpression.OuterParameter);
      var newDataSource = ApplyParameterToTupleParameterRewriter.Rewrite(
        subQueryExpression.ProjectionExpression.ItemProjector.DataSource,
        subqueryTupleParameter,
        subQueryExpression.ApplyParameter);

      var newItemProjectorBody = ApplyParameterToTupleParameterRewriter.Rewrite(
        subQueryExpression.ProjectionExpression.ItemProjector.Item,
        subqueryTupleParameter,
        subQueryExpression.ApplyParameter);

      var itemProjector = new ItemProjectorExpression(newItemProjectorBody, newDataSource, subQueryExpression.ProjectionExpression.ItemProjector.Context);
      parameterOfTuple = context.GetTupleParameter(subQueryExpression.OuterParameter);

      // 2. Add only parameter<tuple>. Tuple value will be assigned 
      // at the moment of materialization in SubQuery constructor
      projection = new ProjectionExpression(
        subQueryExpression.ProjectionExpression.Type,
        itemProjector,
        subQueryExpression.ProjectionExpression.TupleParameterBindings,
        subQueryExpression.ProjectionExpression.ResultType);

      // 3. Make translation 
      elementType = subQueryExpression.ProjectionExpression.ItemProjector.Item.Type;
      var resultType = SequenceHelper.GetSequenceType(elementType);
      var translateMethod = Translator.TranslateMethod.MakeGenericMethod(new[] {resultType});
      return ((TranslatedQuery) translateMethod.Invoke(context.Translator, new object[] {projection, tupleParameters.AddOne(parameterOfTuple)}));
    }

    protected override Expression VisitFieldExpression(FieldExpression expression)
    {
      var tupleExpression = GetTupleExpression(expression);

      // Materialize non-owned field.
      if (expression.Owner==null || expression.UnderlyingProperty == null) {
        if (expression.Field.IsEnum) {
          var underlyingType = Enum.GetUnderlyingType(expression.Type.StripNullable());
          if (expression.Field.IsNullable)
            underlyingType = underlyingType.ToNullable();
          var result = Expression.Convert(
            tupleExpression.MakeTupleAccess(underlyingType, expression.Mapping.Offset),
            expression.Type);
          return result;
        }
        if (typeof(Key).IsAssignableFrom(expression.Field.ValueType)) {
          Expression<Func<Domain, string, Key>> keyParseLambda = (d, s) => Key.Parse(d, s);
          Expression<Func<ItemMaterializationContext, Domain>> domainExtractorLambda = imc => imc.Session.Domain;
          var result = keyParseLambda.BindParameters(
            domainExtractorLambda.BindParameters(itemMaterializationContextParameter),
            tupleExpression.MakeTupleAccess(typeof (string), expression.Mapping.Offset));
          return result;
        }

        var tupleAccess = tupleExpression.MakeTupleAccess(expression.Type, expression.Mapping.Offset);
        return tupleAccess;
      }

      return MaterializeThroughOwner(expression, tupleExpression);
    }

    protected override Expression VisitLocalCollectionExpression(LocalCollectionExpression expression)
    {
      throw new NotSupportedException(String.Format(Strings.ExUnableToMaterializeBackLocalCollectionItem, expression.SourceExpression));
    }

    protected override Expression VisitStructureFieldExpression(StructureFieldExpression expression)
    {
      var tupleExpression = GetTupleExpression(expression);

      // Materialize non-owned structure.
      if (expression.Owner==null) {
        var typeInfo = expression.PersistentType;
        var tuplePrototype = typeInfo.TuplePrototype;
        var mappingInfo = expression.Fields
          .OfType<FieldExpression>()
          .Where(f => f.ExtendedType==ExtendedExpressionType.Field)
          .OrderBy(f => f.Field.MappingInfo.Offset)
          .Select(f => new Pair<int>(f.Field.MappingInfo.Offset, f.Mapping.Offset))
          .Distinct()
          .ToArray();

        int[] columnMap = MaterializationHelper.CreateSingleSourceMap(tuplePrototype.Count, mappingInfo);

        var persistentTupleExpression = (Expression) Expression.Call(
          BuildPersistentTupleMethodInfo,
          tupleExpression,
          Expression.Constant(tuplePrototype),
          Expression.Constant(columnMap));
        return Expression.Convert(
          Expression.Call(
            WellKnownMembers.CreateStructure,
            Expression.Field(itemMaterializationContextParameter, ItemMaterializationContext.SessionFieldInfo),
            Expression.Constant(expression.Type),
            persistentTupleExpression),
          expression.Type);
      }

      return MaterializeThroughOwner(expression, tupleExpression);
    }

    protected override Expression VisitConstructorExpression(ConstructorExpression expression)
    {
      var newExpression = expression.Constructor==null
        ? Expression.New(expression.Type) // Value type with default ctor (expression.Constructor is null in that case)
        : Expression.New(expression.Constructor, expression.ConstructorArguments.Select(Visit));

      return expression.Bindings.Count == 0 
        ? newExpression 
        : (Expression) Expression.MemberInit(newExpression, expression
        .Bindings
        .Where(item => Translator.FilterBindings(item.Key, item.Key.Name, item.Value.Type))
        .Select(item => Expression.Bind(item.Key, Visit(item.Value))).Cast<MemberBinding>());
    }

    protected override Expression VisitStructureExpression(StructureExpression expression)
    {
      var tupleExpression = GetTupleExpression(expression);

      var typeInfo = expression.PersistentType;
      var tuplePrototype = typeInfo.TuplePrototype;
      var mappingInfo = expression.Fields
        .OfType<FieldExpression>()
        .Where(f => f.ExtendedType==ExtendedExpressionType.Field)
        .OrderBy(f => f.Field.MappingInfo.Offset)
        .Select(f => new Pair<int>(f.Field.MappingInfo.Offset, f.Mapping.Offset))
        .Distinct()
        .ToArray();

      int[] columnMap = MaterializationHelper.CreateSingleSourceMap(tuplePrototype.Count, mappingInfo);

      var persistentTupleExpression = (Expression) Expression.Call(
        BuildPersistentTupleMethodInfo,
        tupleExpression,
        Expression.Constant(tuplePrototype),
        Expression.Constant(columnMap));
      return Expression.Convert(
        Expression.Call(
          WellKnownMembers.CreateStructure,
          Expression.Field(itemMaterializationContextParameter, ItemMaterializationContext.SessionFieldInfo),
          Expression.Constant(expression.Type),
          persistentTupleExpression),
        expression.Type);
    }

    protected override Expression VisitKeyExpression(KeyExpression expression)
    {
      // TODO: http://code.google.com/p/dataobjectsdotnet/issues/detail?id=336
      Expression tupleExpression = Expression.Call(
        GetTupleSegmentMethodInfo,
        GetTupleExpression(expression),
        Expression.Constant(expression.Mapping));
      return Expression.Call(
        WellKnownMembers.Key.Create,
        Expression.Constant(context.Domain),
        Expression.Constant(expression.EntityType),
        Expression.Constant(TypeReferenceAccuracy.BaseType),
        tupleExpression);
    }

    protected override Expression VisitEntityExpression(EntityExpression expression)
    {
      var tupleExpression = GetTupleExpression(expression);
      return CreateEntity(expression, tupleExpression);
    }

    /// <exception cref="InvalidOperationException">Unable to materialize Entity.</exception>
    private Expression CreateEntity(IEntityExpression expression, Expression tupleExpression)
    {
      int index;
      if (!entityRegistry.TryGetValue(expression, out index)) {
        index = entityRegistry.Count;
        entityRegistry.Add(expression, index);
      }

      if (itemMaterializationContextParameter==null)
        throw new InvalidOperationException(String.Format(Strings.ExUnableToTranslateLambdaExpressionXBecauseItRequiresToMaterializeEntityOfTypeX, context.Translator.state.CurrentLambda, expression.PersistentType.UnderlyingType.FullName));

      var typeIdField = expression.Fields.SingleOrDefault(f => f.Name==WellKnown.TypeIdFieldName);
      int typeIdIndex = typeIdField==null ? -1 : typeIdField.Mapping.Offset;

      var mappingInfo = expression.Fields
        .OfType<FieldExpression>()
        .Where(f => f.ExtendedType==ExtendedExpressionType.Field)
        .OrderBy(f => f.Field.MappingInfo.Offset)
        .Select(f => new Pair<int>(f.Field.MappingInfo.Offset, f.Mapping.Offset))
        .Distinct()
        .ToArray();

      var isMaterializedExpression = Expression.Call(
        itemMaterializationContextParameter,
        ItemMaterializationContext.IsMaterializedMethodInfo,
        Expression.Constant(index));
      var getEntityExpression = Expression.Call(
        itemMaterializationContextParameter,
        ItemMaterializationContext.GetEntityMethodInfo,
        Expression.Constant(index));
      var materializeEntityExpression = Expression.Call(
        itemMaterializationContextParameter,
        ItemMaterializationContext.MaterializeMethodInfo,
        Expression.Constant(index),
        Expression.Constant(typeIdIndex),
        Expression.Constant(expression.PersistentType),
        Expression.Constant(mappingInfo),
        tupleExpression);
      return Expression.TypeAs(
        Expression.Condition(
          isMaterializedExpression,
          getEntityExpression,
          materializeEntityExpression),
        expression.Type);
    }

    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected override Expression VisitEntityFieldExpression(EntityFieldExpression expression)
    {
      if (expression.Entity!=null)
        return Visit(expression.Entity);

      var tupleExpression = GetTupleExpression(expression);
      if (itemMaterializationContextParameter==null)
        return tupleExpression.MakeTupleAccess(expression.Type, expression.Mapping.Offset);
      return CreateEntity(expression, tupleExpression);
    }

    protected override Expression VisitEntitySetExpression(EntitySetExpression expression)
    {
      var tupleExpression = GetTupleExpression(expression);
      var materializedEntitySetExpression = MaterializeThroughOwner(expression, tupleExpression);
      var prefetechMethodInfo = MaterializationHelper.PrefetchEntitySetMethodInfo
        .MakeGenericMethod(expression.Type);
      var prefetchEntitySetExpression = Expression.Call(
        prefetechMethodInfo,
        materializedEntitySetExpression,
        itemMaterializationContextParameter);
      return prefetchEntitySetExpression;
    }

    protected override Expression VisitColumnExpression(ColumnExpression expression)
    {
      var tupleExpression = GetTupleExpression(expression);
      return tupleExpression.MakeTupleAccess(expression.Type, expression.Mapping.Offset);
    }

    protected override Expression VisitUnary(UnaryExpression u)
    {
      if (u.NodeType==ExpressionType.Convert && u.Type.IsNullable()) {
        var fieldExpression = u.Operand as FieldExpression;
        if (fieldExpression!=null) {
          var tupleExpression = GetTupleExpression(fieldExpression);
          var tupleAccess = tupleExpression.MakeTupleAccess(u.Type, fieldExpression.Mapping.Offset);
          return tupleAccess;
        }
      }
      return base.VisitUnary(u);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (m.Expression!=null
        && (ExtendedExpressionType) m.Expression.NodeType==ExtendedExpressionType.LocalCollection)
        return Visit((Expression) ((LocalCollectionExpression) m.Expression).Fields[m.Member]);

      Expression expression = Visit(m.Expression);
      if (expression==m.Expression)
        return m;

      return Expression.MakeMemberAccess(expression, m.Member);
    }

    #endregion

    #region Private Methods

    private Expression MaterializeThroughOwner(Expression target, Expression tuple)
    {
      return MaterializeThroughOwner(target, tuple, false);
    }


    private Expression MaterializeThroughOwner(Expression target, Expression tuple, bool defaultIfEmpty)
    {
      var field = target as FieldExpression;
      if (field!=null) {
        defaultIfEmpty |= field.DefaultIfEmpty;
        var owner = field.Owner;
        var materializedOwner = MaterializeThroughOwner((Expression) owner, tuple, defaultIfEmpty);
        if (defaultIfEmpty) {
          return Expression.Condition(
            Expression.Equal(materializedOwner, Expression.Constant(null, materializedOwner.Type)),
            Expression.Call(MaterializationHelper.GetDefaultMethodInfo.MakeGenericMethod(field.Type)),
            Expression.MakeMemberAccess(materializedOwner, field.Field.UnderlyingProperty));
        }
        return Expression.MakeMemberAccess(materializedOwner, field.Field.UnderlyingProperty);
      }
      return CreateEntity((EntityExpression) target, tuple);
    }

    private Expression GetTupleExpression(ParameterizedExpression expression)
    {
      if (expression.OuterParameter==null)
        return tupleParameter;

      var parameterOfTuple = context.GetTupleParameter(expression.OuterParameter);
      if (tupleParameters.Contains(parameterOfTuple)) {
        // Make access on Parameter<Tuple>
        return Expression.MakeMemberAccess(Expression.Constant(parameterOfTuple), WellKnownMembers.ParameterOfTupleValue);
      }

      // Use ApplyParameter for RecordSet predicates
      if (itemMaterializationContextParameter==null) {
        var projectionExpression = context.Bindings[expression.OuterParameter];
        var applyParameter = context.GetApplyParameter(projectionExpression);
        var applyParameterExpression = Expression.Constant(applyParameter);
        return Expression.Property(applyParameterExpression, WellKnownMembers.ApplyParameterValue);
      }

      return tupleParameter;
    }

    // ReSharper disable UnusedMember.Local
    private static Tuple BuildPersistentTuple(Tuple tuple, Tuple tuplePrototype, int[] mapping)
    {
      var result = tuplePrototype.CreateNew();
      tuple.CopyTo(result, mapping);
      return result;
    }

    private static Tuple GetTupleSegment(Tuple tuple, Segment<int> segment)
    {
      return tuple.GetSegment(segment).ToRegular();
    }

    // ReSharper restore UnusedMember.Local

    #endregion

    // Constructors

    private ExpressionMaterializer(ParameterExpression
      tupleParameter,
      TranslatorContext context,
      ParameterExpression itemMaterializationContextParameter,
      IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      this.itemMaterializationContextParameter = itemMaterializationContextParameter;
      this.tupleParameter = tupleParameter;
      this.context = context;
      this.tupleParameters = new HashSet<Parameter<Tuple>>(tupleParameters);
    }

    static ExpressionMaterializer()
    {
      BuildPersistentTupleMethodInfo = typeof (ExpressionMaterializer).GetMethod("BuildPersistentTuple", BindingFlags.NonPublic | BindingFlags.Static);
      GetTupleSegmentMethodInfo = typeof (ExpressionMaterializer).GetMethod("GetTupleSegment", BindingFlags.NonPublic | BindingFlags.Static);
    }
  }
}