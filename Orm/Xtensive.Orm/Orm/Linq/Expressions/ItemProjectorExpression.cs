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
using Xtensive.Parameters;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Linq.Expressions.Visitors;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Linq.Rewriters;
using Xtensive.Orm.Rse;
using Xtensive.Linq;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Linq.Expressions
{
  internal class ItemProjectorExpression : ExtendedExpression
  {
    public CompilableProvider DataSource { get; set; }
    public TranslatorContext Context { get; private set; }
    public Expression Item { get; private set; }

    public bool IsPrimitive { get { return CheckItemIsPrimitive(Item); } }

    private bool CheckItemIsPrimitive(Expression item)
    {
      var extendedItem = item.StripCasts() as ExtendedExpression;
      if (extendedItem==null)
        return false;
      switch (extendedItem.ExtendedType) {
        case ExtendedExpressionType.Column:
        case ExtendedExpressionType.Field:
          return true;
        case ExtendedExpressionType.Marker:
          var marker = (MarkerExpression) extendedItem;
          return CheckItemIsPrimitive(marker.Target);
        default:
          return false;
      }
    }

    public List<int> GetColumns(ColumnExtractionModes columnExtractionModes)
    {
      return ColumnGatherer.GetColumns(Item, columnExtractionModes);
    }

    public ItemProjectorExpression Remap(CompilableProvider dataSource, int offset)
    {
      if (offset==0)
        return new ItemProjectorExpression(Item, dataSource, Context);
      var item = GenericExpressionVisitor<IMappedExpression>.Process(Item, mapped => mapped.Remap(offset, new Dictionary<Expression, Expression>()));
      return new ItemProjectorExpression(item, dataSource, Context);
    }

    public ItemProjectorExpression Remap(CompilableProvider dataSource, int[] columnMap)
    {
      var item = GenericExpressionVisitor<IMappedExpression>.Process(Item, mapped => mapped.Remap(columnMap, new Dictionary<Expression, Expression>()));
      return new ItemProjectorExpression(item, dataSource, Context);
    }

    public LambdaExpression ToLambda(TranslatorContext context)
    {
      return ExpressionMaterializer.MakeLambda(Item, context);
    }

    public MaterializationInfo Materialize(TranslatorContext context, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      return ExpressionMaterializer.MakeMaterialization(this, context, tupleParameters);
    }

    public ItemProjectorExpression BindOuterParameter(ParameterExpression parameter)
    {
      var item = GenericExpressionVisitor<IMappedExpression>.Process(Item, mapped => mapped.BindParameter(parameter, new Dictionary<Expression, Expression>()));
      return new ItemProjectorExpression(item, DataSource, Context);
    }

    public ItemProjectorExpression RemoveOuterParameter()
    {
      var item = GenericExpressionVisitor<IMappedExpression>.Process(Item, mapped => mapped.RemoveOuterParameter(new Dictionary<Expression, Expression>()));
      return new ItemProjectorExpression(item, DataSource, Context);
    }

    public ItemProjectorExpression RemoveOwner()
    {
      var item = OwnerRemover.RemoveOwner(Item);
      return new ItemProjectorExpression(item, DataSource, Context);
    }

    public ItemProjectorExpression SetDefaultIfEmpty()
    {
      var item = GenericExpressionVisitor<ParameterizedExpression>.Process(Item, mapped => {
        mapped.DefaultIfEmpty = true;
        return mapped;
      });
      return new ItemProjectorExpression(item, DataSource, Context);
    }

    public ItemProjectorExpression RewriteApplyParameter(ApplyParameter oldParameter, ApplyParameter newParameter)
    {
      var newDataSource = ApplyParameterRewriter.Rewrite(DataSource, oldParameter, newParameter);
      var newItemProjectorBody = ApplyParameterRewriter.Rewrite(Item, oldParameter, newParameter);
      return new ItemProjectorExpression(newItemProjectorBody, newDataSource, Context);
    }

    public ItemProjectorExpression EnsureEntityIsJoined()
    {
      var dataSource = DataSource;
      var newItem = new ExtendedExpressionReplacer(e => {
        if (e is EntityExpression) {
          var entityExpression = (EntityExpression) e;
          var typeInfo = entityExpression.PersistentType;
          if (typeInfo.Fields.All(fieldInfo => entityExpression.Fields.Any(entityField => entityField.Name==fieldInfo.Name)))
            return entityExpression;
          var joinedIndex = typeInfo.Indexes.PrimaryIndex;
          var joinedRs = IndexProvider.Get(joinedIndex).Alias(Context.GetNextAlias());
          var keySegment = entityExpression.Key.Mapping;
          var keyPairs = keySegment.GetItems()
            .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
            .ToArray();
          var offset = dataSource.Header.Length;
          dataSource = entityExpression.IsNullable
            ? dataSource.LeftJoin(joinedRs, JoinAlgorithm.Default, keyPairs)
            : dataSource.Join(joinedRs, JoinAlgorithm.Default, keyPairs);
          EntityExpression.Fill(entityExpression, offset);
          return entityExpression;
        }
        if (e is EntityFieldExpression) {
          var entityFieldExpression = (EntityFieldExpression)e;
          if (entityFieldExpression.Entity != null)
            return entityFieldExpression.Entity;
          var typeInfo = entityFieldExpression.PersistentType;
          var joinedIndex = typeInfo.Indexes.PrimaryIndex;
          var joinedRs = IndexProvider.Get(joinedIndex).Alias(Context.GetNextAlias());
          var keySegment = entityFieldExpression.Mapping;
          var keyPairs = keySegment.GetItems()
            .Select((leftIndex, rightIndex) => new Pair<int>(leftIndex, rightIndex))
            .ToArray();
          var offset = dataSource.Header.Length;
          dataSource = entityFieldExpression.IsNullable 
            ? dataSource.LeftJoin(joinedRs, JoinAlgorithm.Default, keyPairs)
            : dataSource.Join(joinedRs, JoinAlgorithm.Default, keyPairs);
          entityFieldExpression.RegisterEntityExpression(offset);
          return entityFieldExpression.Entity;
        }
        if (e is FieldExpression) {
          var fe = (FieldExpression) e;
          if (fe.ExtendedType==ExtendedExpressionType.Field)
            return fe.RemoveOwner();
        }
        return null;
      })
        .Replace(Item);
      return new ItemProjectorExpression(newItem, dataSource, Context);
    }

    public override string ToString()
    {
      return string.Format("ItemProjectorExpression: IsPrimitive = {0} Item = {1}, DataSource = {2}", IsPrimitive, Item, DataSource);
    }


    // Constructors

    public ItemProjectorExpression(Expression expression, CompilableProvider dataSource, TranslatorContext context)
      : base(ExtendedExpressionType.ItemProjector, expression.Type)
    {
      DataSource = dataSource;
      Context = context;
      var newApplyParameter = Context.GetApplyParameter(dataSource);
      var applyParameterReplacer = new ExtendedExpressionReplacer(ex =>
        ex is SubQueryExpression
          ? ((SubQueryExpression) ex).ReplaceApplyParameter(newApplyParameter)
          : null);
      Item = applyParameterReplacer.Replace(expression);
    }
  }
}