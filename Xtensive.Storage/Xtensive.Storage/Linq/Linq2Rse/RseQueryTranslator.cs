// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.11

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Linq.Expressions;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Linq.Linq2Rse.Internal;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers.Compilable;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  internal class RseQueryTranslator : ExpressionVisitor
  {
    private const string aliasPrefix = "alias";
    private int aliasSuffix = 0;
    private readonly QueryProvider provider;
    private readonly Expression query;
    private readonly DomainModel model;
    private readonly FieldAccessTranslator fieldAccessTranslator;
    private readonly FieldAccessFlattener fieldAccessFlattener;
    private readonly ProjectionBuilder projectionBuilder;
    private readonly ExpressionEvaluator evaluator;
    private readonly ParameterExtractor parameterExtractor;
    private readonly Dictionary<ParameterExpression, ProjectionExpression> map;

    public Expression Query
    {
      get { return query; }
    }

    public DomainModel Model
    {
      get { return model; }
    }

    public ExpressionEvaluator Evaluator
    {
      get { return evaluator; }
    }

    public ParameterExtractor ParameterExtractor
    {
      get { return parameterExtractor; }
    }

    public FieldAccessTranslator FieldAccessTranslator
    {
      get { return fieldAccessTranslator; }
    }

    public FieldAccessFlattener FieldAccessFlattener
    {
      get { return fieldAccessFlattener; }
    }

    public ProjectionExpression GetProjection(ParameterExpression pe)
    {
      return map[pe];
    }

    public ProjectionExpression Translate()
    {
      return (ProjectionExpression) Visit(query);
    }

    public Dictionary<string, Segment<int>> BuildFieldMapping(TypeInfo type, int offset)
    {
      var fieldMapping = new Dictionary<string, Segment<int>>();
      foreach (var field in type.Fields) {
        fieldMapping.Add(field.Name, new Segment<int>(offset + field.MappingInfo.Offset, field.MappingInfo.Length)); 
        if (field.IsEntity)
          fieldMapping.Add(field.Name + ".Key", new Segment<int>(offset + field.MappingInfo.Offset, field.MappingInfo.Length));
      }
      var keySegment = new Segment<int>(offset, type.Hierarchy.KeyFields.Sum(pair => pair.Key.MappingInfo.Length));
      fieldMapping.Add("Key", keySegment);

      return fieldMapping;
    }

    protected bool IsRoot(Expression expression)
    {
      return query==expression;
    }

    public string GetNextAlias()
    {
      return aliasPrefix + aliasSuffix++;
    }


    protected override Expression VisitConstant(ConstantExpression c)
    {
      if (c.Value == null)
        return c;
      var rootPoint = c.Value as IQueryable;
      if (rootPoint != null) {
        var type = provider.Model.Types[rootPoint.ElementType];
        var index = type.Indexes.PrimaryIndex;

        var fieldMapping = BuildFieldMapping(type, 0);
        var mapping = new TypeMapping(fieldMapping, new Dictionary<string, TypeMapping>());
        var recordSet = IndexProvider.Get(index).Result;

        return new ProjectionExpression(
          c.Type,
          recordSet,
          mapping,
          null);
      }
      return base.VisitConstant(c);
    }

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (mc.Method.DeclaringType==typeof (Queryable) || mc.Method.DeclaringType==typeof (Enumerable)) {
        switch (mc.Method.Name) {
        case WellKnown.Queryable.Where:
          return VisitWhere(mc.Arguments[0], mc.Arguments[1].StripQuotes());
        case WellKnown.Queryable.Select:
          return VisitSelect(mc.Type, mc.Arguments[0], mc.Arguments[1].StripQuotes());
        case WellKnown.Queryable.SelectMany:
          if (mc.Arguments.Count==2) {
            return VisitSelectMany(
              mc.Type, mc.Arguments[0],
              mc.Arguments[1].StripQuotes(),
              null);
          }
          if (mc.Arguments.Count==3) {
            return VisitSelectMany(
              mc.Type, mc.Arguments[0],
              mc.Arguments[1].StripQuotes(),
              mc.Arguments[2].StripQuotes());
          }
          break;
        case WellKnown.Queryable.Join:
          return VisitJoin(
            mc.Type, mc.Arguments[0], mc.Arguments[1],
            mc.Arguments[2].StripQuotes(),
            mc.Arguments[3].StripQuotes(),
            mc.Arguments[4].StripQuotes());
        case WellKnown.Queryable.OrderBy:
          return VisitOrderBy(mc.Type, mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Positive);
        case WellKnown.Queryable.OrderByDescending:
          return VisitOrderBy(mc.Type, mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Negative);
        case WellKnown.Queryable.ThenBy:
          return VisitThenBy(mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Positive);
        case WellKnown.Queryable.ThenByDescending:
          return VisitThenBy(mc.Arguments[0], (mc.Arguments[1].StripQuotes()), Direction.Negative);
        case WellKnown.Queryable.GroupBy:
          if (mc.Arguments.Count==2) {
            return VisitGroupBy(
              mc.Arguments[0],
              (mc.Arguments[1].StripQuotes()),
              null,
              null
              );
          }
          if (mc.Arguments.Count==3) {
            LambdaExpression lambda1 = (mc.Arguments[1].StripQuotes());
            LambdaExpression lambda2 = (mc.Arguments[2].StripQuotes());
            if (lambda2.Parameters.Count==1) {
              // second lambda is element selector
              return VisitGroupBy(mc.Arguments[0], lambda1, lambda2, null);
            }
            if (lambda2.Parameters.Count==2) {
              // second lambda is result selector
              return VisitGroupBy(mc.Arguments[0], lambda1, null, lambda2);
            }
          }
          else if (mc.Arguments.Count==4) {
            return VisitGroupBy(
              mc.Arguments[0],
              (mc.Arguments[1].StripQuotes()),
              (mc.Arguments[2].StripQuotes()),
              (mc.Arguments[3].StripQuotes())
              );
          }
          break;
        case WellKnown.Queryable.Count:
        case WellKnown.Queryable.Min:
        case WellKnown.Queryable.Max:
        case WellKnown.Queryable.Sum:
        case WellKnown.Queryable.Average:
          if (mc.Arguments.Count==1) {
            return VisitAggregate(mc.Arguments[0], mc.Method, null, IsRoot(mc));
          }
          if (mc.Arguments.Count==2) {
            LambdaExpression selector = (mc.Arguments[1].StripQuotes());
            return VisitAggregate(mc.Arguments[0], mc.Method, selector, IsRoot(mc));
          }
          break;
        case WellKnown.Queryable.Distinct:
          if (mc.Arguments.Count==1) {
            return VisitDistinct(mc.Arguments[0]);
          }
          break;
        case WellKnown.Queryable.Skip:
          if (mc.Arguments.Count==2) {
            return VisitSkip(mc.Arguments[0], mc.Arguments[1]);
          }
          break;
        case WellKnown.Queryable.Take:
          if (mc.Arguments.Count==2) {
            return VisitTake(mc.Arguments[0], mc.Arguments[1]);
          }
          break;
        case WellKnown.Queryable.First:
        case WellKnown.Queryable.FirstOrDefault:
        case WellKnown.Queryable.Single:
        case WellKnown.Queryable.SingleOrDefault:
          if (mc.Arguments.Count==1) {
            return VisitFirst(mc.Arguments[0], null, mc.Method, IsRoot(mc));
          }
          if (mc.Arguments.Count==2) {
            LambdaExpression predicate = (mc.Arguments[1].StripQuotes());
            return VisitFirst(mc.Arguments[0], predicate, mc.Method, IsRoot(mc));
          }
          break;
        case WellKnown.Queryable.Any:
          if (mc.Arguments.Count==1) {
            return VisitAnyAll(mc.Arguments[0], mc.Method, null, IsRoot(mc));
          }
          if (mc.Arguments.Count==2) {
            LambdaExpression predicate = (mc.Arguments[1].StripQuotes());
            return VisitAnyAll(mc.Arguments[0], mc.Method, predicate, IsRoot(mc));
          }
          break;
        case WellKnown.Queryable.All:
          if (mc.Arguments.Count==2) {
            var predicate = (LambdaExpression) (mc.Arguments[1]);
            return VisitAnyAll(mc.Arguments[0], mc.Method, predicate, IsRoot(mc));
          }
          break;
        case WellKnown.Queryable.Contains:
          if (mc.Arguments.Count==2) {
            return VisitContains(mc.Arguments[0], mc.Arguments[1], IsRoot(mc));
          }
          break;
        }
      }
      return base.VisitMethodCall(mc);
    }

    private Expression VisitContains(Expression source, Expression match, bool isRoot)
    {
      throw new NotImplementedException();
    }

    private Expression VisitAnyAll(Expression source, MethodInfo method, LambdaExpression predicate, bool isRoot)
    {
      throw new NotImplementedException();
    }

    private Expression VisitFirst(Expression source, LambdaExpression predicate, MethodInfo method, bool isRoot)
    {
      if (!isRoot)
        throw new NotImplementedException();
      ProjectionExpression projection = predicate!=null ? 
        (ProjectionExpression) VisitWhere(source, predicate) : 
        (ProjectionExpression) Visit(source);
      RecordSet recordSet = null;
      switch (method.Name) {
      case WellKnown.Queryable.First:
      case WellKnown.Queryable.FirstOrDefault:
        recordSet = projection.RecordSet.Take(1);
        break;
      case WellKnown.Queryable.Single:
      case WellKnown.Queryable.SingleOrDefault:
        recordSet = projection.RecordSet.Take(2);
        break;
      }
      var enumerableType = typeof(Enumerable);
      MethodInfo enumerableMethod = enumerableType
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .First(m => m.Name == method.Name && m.GetParameters().Length == 1)
        .MakeGenericMethod(method.ReturnType);
      MethodInfo castMethod = enumerableType.GetMethod("Cast").MakeGenericMethod(method.ReturnType);
      Expression<Func<RecordSet,object>> materializer = set => set.ToEntities(method.ReturnType);
      var rs = materializer.Parameters[0];
      var body = Expression.Convert(Expression.Call(null, enumerableMethod, Expression.Call(null, castMethod, materializer.Body)), typeof(object));
      var le = Expression.Lambda(body, rs);
      return new ProjectionExpression(method.ReturnType, recordSet, projection.Mapping, (Expression<Func<RecordSet, object>>) le);
    }

    private Expression VisitTake(Expression source, Expression take)
    {
      var projection = (ProjectionExpression)Visit(source);
      var rs = projection.RecordSet.Take((Expression<Func<int>>) take, true);
      return new ProjectionExpression(projection.Type, rs, projection.Mapping, projection.Projector);
    }

    private Expression VisitSkip(Expression source, Expression skip)
    {
      var projection = (ProjectionExpression)Visit(source);
      var rs = projection.RecordSet.Skip((Expression<Func<int>>)skip, true);
      return new ProjectionExpression(projection.Type, rs, projection.Mapping, projection.Projector);
    }

    private Expression VisitDistinct(Expression expression)
    {
      throw new NotImplementedException();
    }

    private Expression VisitAggregate(Expression source, MethodInfo method, LambdaExpression argument, bool isRoot)
    {
      if (!isRoot)
        throw new NotImplementedException();
      string name = "$Count";
      AggregateType type = AggregateType.Count;
      Expression<Func<RecordSet, object>> shaper;
      ProjectionExpression projection;
      int aggregateColumn = 0;
      if (method.Name == WellKnown.Queryable.Count) {
        shaper = set => (int)(set.First().GetValue<long>(0));
        if (argument != null)
          projection = (ProjectionExpression) VisitWhere(source, argument);
        else
          projection = (ProjectionExpression) Visit(source);
      }
      else {
        projection = (ProjectionExpression)Visit(source);
        if (argument==null) 
          throw new NotSupportedException();

        map[argument.Parameters[0]] = projection;
//        var column = argument.Body as FieldAccessExpression;
//        if (column==null)
//          throw new NotSupportedException();
//        aggregateColumn = column.Field.MappingInfo.Offset;
        shaper = set => set.First().GetValueOrDefault(0);
        switch (method.Name) {
        case WellKnown.Queryable.Min:
          name = "$Min";
          type = AggregateType.Min;
          break;
        case WellKnown.Queryable.Max:
          name = "$Max";
          type = AggregateType.Max;
          break;
        case WellKnown.Queryable.Sum:
          name = "$Sum";
          type = AggregateType.Sum;
          break;
        case WellKnown.Queryable.Average:
          name = "$Avg";
          type = AggregateType.Avg;
          break;
        }
      }

      var recordSet = projection.RecordSet.Aggregate(null, new AggregateColumnDescriptor(name, aggregateColumn, type));
      return new ProjectionExpression(projection.Type, recordSet, null, shaper);
    }

    private Expression VisitGroupBy(Expression source, LambdaExpression keySelector, LambdaExpression elementSelector, LambdaExpression resultSelector)
    {
      throw new NotImplementedException();
    }

    private Expression VisitThenBy(Expression expression, LambdaExpression lambdaExpression, Direction direaction)
    {
      throw new NotImplementedException();
    }

    private Expression VisitOrderBy(Type type, Expression expression, LambdaExpression lambdaExpression, Direction direction)
    {
      throw new NotImplementedException();
    }

    private Expression VisitJoin(Type resultType, Expression outerSource, Expression innerSource, LambdaExpression outerKey, LambdaExpression innerKey, LambdaExpression resultSelector)
    {
      var outer = (ProjectionExpression)Visit(outerSource);
      var inner = (ProjectionExpression)Visit(innerSource);
      outer = fieldAccessFlattener.FlattenFieldAccess(outer, outerKey);
      inner = fieldAccessFlattener.FlattenFieldAccess(inner, innerKey);
      var outerKeyPath = fieldAccessTranslator.Translate(outerKey.Body);
      var innerKeyPath = fieldAccessTranslator.Translate(innerKey.Body);
      if (outerKeyPath == null || innerKeyPath == null) {
        throw new InvalidOperationException();
      }

      var keyPairs = new List<Pair<int>>();
      bool isKey = typeof(Key).IsAssignableFrom(outerKey.Body.Type);
      bool isEntity = typeof(IEntity).IsAssignableFrom(outerKey.Body.Type);
      bool isStructure = typeof(Structure).IsAssignableFrom(outerKey.Body.Type);

      if (isKey || isEntity || isStructure) {
        if (isStructure)
          throw new NotImplementedException();

        TypeInfo type;
        if (isKey) {
          var keyAccess = (MemberExpression) outerKey.Body;
          type = Model.Types[keyAccess.Expression.Type];
        }
        else
          type = Model.Types[outerKey.Body.Type];
        MappingPathItem pathItem = null;
        if (outerKeyPath.Count != 0)
          pathItem = outerKeyPath.ExtractTail();
        if (innerKeyPath.Count != 0)
          pathItem = innerKeyPath.ExtractTail();
        foreach (var field in type.Hierarchy.KeyFields.Keys) {
          var fieldName = pathItem == null ? field.Name : pathItem.JoinedFieldName + "." + field.Name;
          var keyItem = new MappingPathItem(fieldName, null);
          outerKeyPath.AddTail(keyItem);
          innerKeyPath.AddTail(keyItem);
          keyPairs.Add(new Pair<int>(
            outer.GetFieldSegment(outerKeyPath).Offset, 
            outer.GetFieldSegment(innerKeyPath).Offset));
          outerKeyPath.ExtractTail();
          innerKeyPath.ExtractTail();
        }
      }
      else {
        keyPairs.Add(new Pair<int>(outer.GetFieldSegment(outerKeyPath).Offset, inner.GetFieldSegment(innerKeyPath).Offset));
      }

      var innerRecordSet = inner.RecordSet.Alias(GetNextAlias());
      var recordSet = outer.RecordSet.Join(innerRecordSet, keyPairs.ToArray());
//      Dictionary<TypeInfo, TypeMapping> typeMappings = null;
//      Func<RecordSet, object> shaper = null;
//      return new ProjectionExpression(resultType, recordSet, typeMappings, shaper, true);

      throw new NotImplementedException(ExpressionWriter.WriteToString(query));
    }

    private Expression VisitSelectMany(Type resultType, Expression source, LambdaExpression collectionSelector, LambdaExpression resultSelector)
    {
      throw new NotImplementedException();
    }

    private Expression VisitSelect(Type resultType, Expression expression, LambdaExpression le)
    {
      var source = (ProjectionExpression)Visit(expression);
      map[le.Parameters[0]] = source;
      var result = projectionBuilder.Build(source, le.Body);
      return result;
    }

    private Expression VisitWhere(Expression expression, LambdaExpression le)
    {
      var source = (ProjectionExpression)Visit(expression);
      map[le.Parameters[0]] = source;
      source = fieldAccessFlattener.FlattenFieldAccess(source, le);
      var predicate = fieldAccessTranslator.Translate(source, le);
      var recordSet = source.RecordSet.Filter((Expression<Func<Tuple, bool>>)predicate);
      return new ProjectionExpression(expression.Type, recordSet, source.Mapping, null);
    }


    // Constructor

    public RseQueryTranslator(QueryProvider provider, Expression query)
    {
      model = provider.Model;
      this.provider = provider;
      this.query = query;
      map = new Dictionary<ParameterExpression, ProjectionExpression>();
      evaluator = new ExpressionEvaluator(query);
      parameterExtractor = new ParameterExtractor(evaluator);
      fieldAccessTranslator = new FieldAccessTranslator(this);
      fieldAccessFlattener = new FieldAccessFlattener(this);
      projectionBuilder = new ProjectionBuilder(this);
    }
  }
}