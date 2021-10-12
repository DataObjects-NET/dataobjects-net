// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.05.21

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Linq.Rewriters;
using Xtensive.Orm.Rse;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  [Serializable]
  internal class ExpressionMaterializer : PersistentExpressionVisitor
  {
    private static readonly MethodInfo BuildPersistentTupleMethod;
    private static readonly MethodInfo GetTupleSegmentMethod;
    private static readonly MethodInfo GetParameterValueMethod;
    private static readonly PropertyInfo ParameterContextProperty;
    private static readonly MethodInfo GetTupleParameterValueMethod;
    private static readonly ParameterExpression tupleParameter = Expression.Parameter(WellKnownOrmTypes.Tuple, "tuple");

    private static readonly ParameterExpression materializationContextParameter = Expression.Parameter(WellKnownOrmTypes.ItemMaterializationContext, "mc");
    private static readonly ConstantExpression typeReferenceAccuracyBaseTypeConstantExpression = Expression.Constant(TypeReferenceAccuracy.BaseType);

    private readonly TranslatorContext context;
    private readonly ParameterExpression itemMaterializationContextParameter;
    private readonly Dictionary<IEntityExpression, int> entityRegistry = new Dictionary<IEntityExpression, int>();
    private readonly HashSet<Parameter<Tuple>> tupleParameters;

    private static readonly Type[] SubQueryConstructorArgumentTypes = {
      WellKnownOrmTypes.ProjectionExpression,
      WellKnownOrmTypes.TranslatedQuery,
      WellKnownOrmTypes.ParameterOfTuple,
      WellKnownOrmTypes.Tuple,
      WellKnownOrmTypes.ItemMaterializationContext
    };

    #region Public static methods

    public static LambdaExpression MakeLambda(Expression expression, TranslatorContext context)
    {
      var visitor = new ExpressionMaterializer(context, null, Enumerable.Empty<Parameter<Tuple>>());
      var processedExpression = OwnerRemover.RemoveOwner(expression);
      return FastExpression.Lambda(visitor.Visit(processedExpression), tupleParameter);
    }

    public static MaterializationInfo MakeMaterialization(ItemProjectorExpression projector, TranslatorContext context, 
      IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      var visitor = new ExpressionMaterializer(context, materializationContextParameter, tupleParameters);
      var lambda = FastExpression.Lambda(visitor.Visit(projector.Item), tupleParameter, materializationContextParameter);
      var count = visitor.entityRegistry.Count;
      return new MaterializationInfo(count, lambda);
    }

    #endregion

    #region Visitor methods overrsides

    protected override Expression VisitFullTextExpression(FullTextExpression expression)
    {
      var rankMaterializer = Visit(expression.RankExpression);
      var entityMaterializer = Visit(expression.EntityExpression);
      var constructorInfo = WellKnownOrmTypes.FullTextMatchOfT
        .MakeGenericType(expression.EntityExpression.Type)
        .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
          new object[] {WellKnownTypes.Double, expression.EntityExpression.Type});

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
      var groupingCtor = WellKnownOrmTypes.GroupingOfTKeyTElement
        .MakeGenericType(keyType, elementType)
        .GetConstructor(new[] {
          WellKnownOrmTypes.ProjectionExpression,
          WellKnownOrmTypes.TranslatedQuery,
          WellKnownOrmTypes.ParameterOfTuple,
          WellKnownOrmTypes.Tuple,
          keyType,
          WellKnownOrmTypes.ItemMaterializationContext
        });

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
      var translatedQuery = PrepareSubqueryParameters(
        subQueryExpression, out var parameterOfTuple, out var elementType, out var projection);

      // 2. Create constructor
      var subQueryCtor = WellKnownOrmTypes.SubQueryOfT
        .MakeGenericType(elementType)
        .GetConstructor(SubQueryConstructorArgumentTypes);

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
      var projectionExpression = subQueryExpression.ProjectionExpression;

      // 1. Rewrite recordset and ItemProjector to parameter<tuple>
      var subqueryTupleParameter = context.GetTupleParameter(subQueryExpression.OuterParameter);
      var newDataSource = ApplyParameterToTupleParameterRewriter.Rewrite(
        projectionExpression.ItemProjector.DataSource,
        subqueryTupleParameter,
        subQueryExpression.ApplyParameter);

      var newItemProjectorBody = ApplyParameterToTupleParameterRewriter.Rewrite(
        projectionExpression.ItemProjector.Item,
        subqueryTupleParameter,
        subQueryExpression.ApplyParameter);

      var itemProjector = new ItemProjectorExpression(newItemProjectorBody, newDataSource, projectionExpression.ItemProjector.Context);
      parameterOfTuple = context.GetTupleParameter(subQueryExpression.OuterParameter);

      // 2. Add only parameter<tuple>. Tuple value will be assigned 
      // at the moment of materialization in SubQuery constructor
      projection = projectionExpression.Apply(itemProjector);

      // 3. Make translation 
      elementType = projectionExpression.ItemProjector.Item.Type;
      return context.Translator.Translate(projection, tupleParameters.Append(parameterOfTuple));
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
        if (WellKnownOrmTypes.Key.IsAssignableFrom(expression.Field.ValueType)) {
          Expression<Func<Domain, string, Key>> keyParseLambda = (d, s) => Key.Parse(d, s);
          Expression<Func<ItemMaterializationContext, Domain>> domainExtractorLambda = imc => imc.Session.Domain;
          var result = keyParseLambda.BindParameters(
            domainExtractorLambda.BindParameters(itemMaterializationContextParameter),
            tupleExpression.MakeTupleAccess(WellKnownTypes.String, expression.Mapping.Offset));
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
          BuildPersistentTupleMethod,
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

      var realBindings = expression.NativeBindings;

      return expression.NativeBindings.Count == 0 
        ? newExpression 
        : (Expression) Expression.MemberInit(newExpression, expression
        .NativeBindings
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
        BuildPersistentTupleMethod,
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
        GetTupleSegmentMethod,
        GetTupleExpression(expression),
        Expression.Constant(expression.Mapping));
      return Expression.Call(
        WellKnownMembers.Key.Create,
        Expression.Constant(context.Domain),
        Expression.Property(
          Expression.Field(itemMaterializationContextParameter, ItemMaterializationContext.SessionFieldInfo),
          WellKnownMembers.SessionNodeId),
        Expression.Constant(expression.EntityType),
        typeReferenceAccuracyBaseTypeConstantExpression,
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
      var originalOperandType = u.Operand.Type;
      var convertedOperandType = u.Type;

      var isConvertToNullable = u.NodeType == ExpressionType.Convert
        && !originalOperandType.IsNullable()
        && convertedOperandType.IsNullable()
        && originalOperandType == convertedOperandType.StripNullable();
      // Optimize tuple access by replacing
      //   (T?) tuple.GetValueOrDefault<T>(index)
      // with
      //   tuple.GetValueOrDefault<T?>(index)
      if (isConvertToNullable) {
        var operand = Visit(u.Operand);
        var tupleAccess = operand.AsTupleAccess();
        if (tupleAccess != null) {
          var index = tupleAccess.GetTupleAccessArgument();
          return tupleAccess.Object.MakeTupleAccess(u.Type, index);
        }
        if (operand != u.Operand) {
          return Expression.Convert(operand, u.Type);
        }
        return u;
      }
      return base.VisitUnary(u);
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (m.Expression!=null) {
        if ((ExtendedExpressionType) m.Expression.NodeType==ExtendedExpressionType.LocalCollection) {
          return Visit((Expression) ((LocalCollectionExpression) m.Expression).Fields[m.Member]);
        }
        if (itemMaterializationContextParameter!=null
          && string.Equals(nameof(Parameter<object>.Value), m.Member.Name, StringComparison.Ordinal)
          && WellKnownOrmTypes.Parameter.IsAssignableFrom(m.Expression.Type)) {
          var parameterType = m.Expression.Type;
          var parameterValueType = parameterType.IsGenericType
            ? parameterType.GetGenericArguments()[0]
            : WellKnownTypes.Object;
          return Expression.Call(
            Expression.MakeMemberAccess(itemMaterializationContextParameter, ParameterContextProperty),
              GetParameterValueMethod.MakeGenericMethod(parameterValueType), m.Expression);
        }
      }

      var expression = Visit(m.Expression);
      if (expression==m.Expression) {
        return m;
      }

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
        Expression fieldExpression;
        if (field.Field.IsDynamicallyDefined) {
          var attributes = materializedOwner.Type.GetCustomAttributes(WellKnownTypes.DefaultMemberAttribute, true);
          var indexerPropertyName = ((DefaultMemberAttribute) attributes.Single()).MemberName;
          var methodInfo = materializedOwner.Type.GetProperty(indexerPropertyName).GetGetMethod();
          fieldExpression = Expression.Convert(Expression.Call(materializedOwner, methodInfo, Expression.Constant(field.Field.Name)), field.Field.ValueType);
        }
        else
          fieldExpression = Expression.MakeMemberAccess(materializedOwner, field.Field.UnderlyingProperty);
        if (defaultIfEmpty) {
          return Expression.Condition(
            Expression.Equal(materializedOwner, Expression.Constant(null, materializedOwner.Type)),
            Expression.Call(MaterializationHelper.GetDefaultMethodInfo.MakeGenericMethod(field.Type)),
            fieldExpression);
        }
        return fieldExpression;
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
        return Expression.Call(
          Expression.MakeMemberAccess(itemMaterializationContextParameter, ParameterContextProperty),
          GetTupleParameterValueMethod, Expression.Constant(parameterOfTuple));
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

    private static Tuple GetTupleSegment(Tuple tuple, in Segment<int> segment)
    {
      return tuple.GetSegment(segment).ToRegular();
    }

    // ReSharper restore UnusedMember.Local

    #endregion

    // Constructors

    private ExpressionMaterializer(
      TranslatorContext context,
      ParameterExpression itemMaterializationContextParameter,
      IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      this.itemMaterializationContextParameter = itemMaterializationContextParameter;
      this.context = context;
      this.tupleParameters = new HashSet<Parameter<Tuple>>(tupleParameters);
    }

    static ExpressionMaterializer()
    {
      ParameterContextProperty =
        WellKnownOrmTypes.ItemMaterializationContext.GetProperty(nameof(ItemMaterializationContext.ParameterContext));
      GetParameterValueMethod = WellKnownOrmTypes.ParameterContext.GetMethod(nameof(ParameterContext.GetValue));
      GetTupleParameterValueMethod = GetParameterValueMethod.MakeGenericMethod(WellKnownOrmTypes.Tuple);
      BuildPersistentTupleMethod = typeof (ExpressionMaterializer).GetMethod("BuildPersistentTuple", BindingFlags.NonPublic | BindingFlags.Static);
      GetTupleSegmentMethod = typeof (ExpressionMaterializer).GetMethod("GetTupleSegment", BindingFlags.NonPublic | BindingFlags.Static);
    }
  }
}