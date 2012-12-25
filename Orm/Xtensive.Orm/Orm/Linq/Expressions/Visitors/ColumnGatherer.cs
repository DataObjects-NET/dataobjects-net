// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Linq.Expressions.Visitors
{
  internal sealed class ColumnGatherer : PersistentExpressionVisitor
  {
    private readonly ColumnExtractionModes columnExtractionModes;
    private readonly List<Pair<int, Expression>> columns = new List<Pair<int, Expression>>();
    private SubQueryExpression topSubquery;

    private bool TreatEntityAsKey
    {
      get { return (columnExtractionModes & ColumnExtractionModes.TreatEntityAsKey)!=ColumnExtractionModes.Default; }
    }

    private bool KeepTypeId
    {
      get { return (columnExtractionModes & ColumnExtractionModes.KeepTypeId)!=ColumnExtractionModes.Default; }
    }

    private bool DistinctValues
    {
      get { return (columnExtractionModes & ColumnExtractionModes.Distinct)!=ColumnExtractionModes.Default; }
    }

    private bool OrderedValues
    {
      get { return (columnExtractionModes & ColumnExtractionModes.Ordered)!=ColumnExtractionModes.Default; }
    }

    private bool OmitLazyLoad
    {
      get { return (columnExtractionModes & ColumnExtractionModes.OmitLazyLoad)!=ColumnExtractionModes.Default; }
    }

    public static List<Pair<int, Expression>> GetColumnsAndExpressions(Expression expression, ColumnExtractionModes columnExtractionModes)
    {
      var gatherer = new ColumnGatherer(columnExtractionModes);
      gatherer.Visit(expression);
      var distinct = gatherer.DistinctValues
        ? gatherer.columns.Distinct()
        : gatherer.columns;
      var ordered = gatherer.OrderedValues
        ? distinct.OrderBy(i => i)
        : distinct;
      return ordered.ToList();
    }
    
    public static List<int> GetColumns(Expression expression, ColumnExtractionModes columnExtractionModes)
    {
      var gatherer = new ColumnGatherer(columnExtractionModes);
      gatherer.Visit(expression);
      var distinct = gatherer.DistinctValues
        ? gatherer.columns.Select(p=>p.First).Distinct()
        : gatherer.columns.Select(p=>p.First);
      var ordered = gatherer.OrderedValues
        ? distinct.OrderBy(i => i)
        : distinct;
      return ordered.ToList();
    }

    protected override Expression VisitMarker(MarkerExpression expression)
    {
      Visit(expression.Target);
      return expression;
    }

    protected override Expression VisitFieldExpression(FieldExpression f)
    {
      ProcessFieldOwner(f);
      AddColumns(f, f.Mapping.GetItems());
      return f;
    }

    protected override Expression VisitStructureFieldExpression(StructureFieldExpression s)
    {
      ProcessFieldOwner(s);
      AddColumns(s,
        s.Fields
          .Where(f => f.ExtendedType==ExtendedExpressionType.Field)
          .Select(f => f.Mapping.Offset));
      return s;
    }

    protected override Expression VisitKeyExpression(KeyExpression k)
    {
      AddColumns(k, k.Mapping.GetItems());
      return k;
    }

    protected override Expression VisitEntityExpression(EntityExpression e)
    {
      if (TreatEntityAsKey) {
        var keyExpression = (KeyExpression) e.Fields.First(f => f.ExtendedType==ExtendedExpressionType.Key);
        AddColumns(e, keyExpression.Mapping.GetItems());
        if (KeepTypeId)
          AddColumns(e, e.Fields.First(f => f.Name==WellKnown.TypeIdFieldName).Mapping.GetItems());
      }
      else {
        AddColumns(e,
          e.Fields
            .OfType<FieldExpression>()
            .Where(f => f.ExtendedType==ExtendedExpressionType.Field)
            .Where(f => !(OmitLazyLoad && f.Field.IsLazyLoad))
            .Select(f => f.Mapping.Offset));
      }
      return e;
    }

    protected override Expression VisitEntityFieldExpression(EntityFieldExpression ef)
    {
      var keyExpression = (KeyExpression) ef.Fields.First(f => f.ExtendedType==ExtendedExpressionType.Key);
      AddColumns(ef, keyExpression.Mapping.GetItems());
      if (!TreatEntityAsKey)
        Visit(ef.Entity);
      return ef;
    }

    protected override Expression VisitEntitySetExpression(EntitySetExpression es)
    {
      VisitEntityExpression((EntityExpression) es.Owner);
      return es;
    }

    protected override Expression VisitColumnExpression(ColumnExpression c)
    {
      AddColumns(c, c.Mapping.GetItems());
      return c;
    }

    protected override Expression VisitStructureExpression(StructureExpression expression)
    {
      AddColumns(expression,
        expression.Fields
          .Where(fieldExpression => fieldExpression.ExtendedType==ExtendedExpressionType.Field)
          .Select(fieldExpression => fieldExpression.Mapping.Offset));
      return expression;
    }

    protected override Expression VisitGroupingExpression(GroupingExpression expression)
    {
      Visit(expression.KeyExpression);
      VisitSubQueryExpression(expression);
      return expression;
    }

    protected override Expression VisitSubQueryExpression(SubQueryExpression subQueryExpression)
    {
      bool isTopSubquery = false;

      if (topSubquery==null) {
        isTopSubquery = true;
        topSubquery = subQueryExpression;
      }

      Visit(subQueryExpression.ProjectionExpression.ItemProjector.Item);
      var visitor = new ApplyParameterAccessVisitor(topSubquery.ApplyParameter, (mc, index) => {
        columns.Add(new Pair<int, Expression>(index, mc));
        return mc;
      });
      var providerVisitor = new CompilableProviderVisitor((provider, expression) => visitor.Visit(expression));
      providerVisitor.VisitCompilable(subQueryExpression.ProjectionExpression.ItemProjector.DataSource);

      if (isTopSubquery)
        topSubquery = null;

      return subQueryExpression;
    }

    protected override Expression VisitLocalCollectionExpression(LocalCollectionExpression expression)
    {
      foreach (var field in expression.Fields)
        Visit((Expression) field.Value);
      return expression;
    }

    protected override Expression VisitConstructorExpression(ConstructorExpression expression)
    {
      foreach (var binding in expression.Bindings)
        Visit(binding.Value);
      foreach (var argument in expression.ConstructorArguments)
        Visit(argument);
      return base.VisitConstructorExpression(expression);
    }

    private void ProcessFieldOwner(FieldExpression fieldExpression)
    {
      if (TreatEntityAsKey || fieldExpression.Owner==null)
        return;
      var entity = fieldExpression.Owner as EntityExpression;
      var structure = fieldExpression.Owner as StructureFieldExpression;
      while (entity==null && structure!=null) {
        entity = structure.Owner as EntityExpression;
        structure = structure.Owner as StructureFieldExpression;
      }
      if (entity==null)
        throw new InvalidOperationException(String.Format(Strings.ExUnableToResolveOwnerOfFieldExpressionX, fieldExpression));

      AddColumns(fieldExpression,
        entity
          .Key
          .Mapping
          .GetItems()
          .AddOne(entity
            .Fields
            .Single(field => field.Name==WellKnown.TypeIdFieldName)
            .Mapping
            .Offset));
    }

    private void AddColumns(ParameterizedExpression parameterizedExpression, IEnumerable<int> expressionColumns)
    {
      var isSubqueryParameter = topSubquery!=null && parameterizedExpression.OuterParameter==topSubquery.OuterParameter;
      var isNotParametrized = topSubquery==null && parameterizedExpression.OuterParameter==null;

      if (isSubqueryParameter || isNotParametrized)
        columns.AddRange(expressionColumns.Select(i=>new Pair<int, Expression>(i, parameterizedExpression)));
    }

    protected override Expression VisitFreeTextExpression(FullTextExpression expression)
    {
      VisitEntityExpression(expression.EntityExpression);
      VisitColumnExpression(expression.RankExpression);
      return expression;
    }

    // Constructors

    private ColumnGatherer(ColumnExtractionModes columnExtractionModes)
    {
      this.columnExtractionModes = columnExtractionModes;
    }
  }
}