// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.05.06

using System.Collections.Generic;
using System.Linq.Expressions;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Linq.Expressions.Visitors;
using Xtensive.Storage.Linq.Materialization;
using Xtensive.Storage.Linq.Rewriters;
using Xtensive.Storage.Rse;
using Xtensive.Core.Linq;

namespace Xtensive.Storage.Linq.Expressions
{
  internal class ItemProjectorExpression : ExtendedExpression
  {
    public RecordSet DataSource { get; set; }

    public TranslatorContext Context { get; private set; }

    public Expression Item { get; private set; }

    public bool IsPrimitive
    {
      get
      {
        var expression = Item.StripCasts();
        var extendedExpression = expression as ExtendedExpression;
        if (extendedExpression==null)
          return false;
        return extendedExpression.ExtendedType==ExtendedExpressionType.Column ||
          extendedExpression.ExtendedType==ExtendedExpressionType.Field;
      }
    }

    public List<int> GetColumns(ColumnExtractionModes columnExtractionModes)
    {
      return ColumnGatherer.GetColumns(Item, columnExtractionModes);
    }

    public ItemProjectorExpression Remap(RecordSet dataSource, int offset)
    {
      if (offset==0)
        return new ItemProjectorExpression(Item, dataSource, Context);
      var item = GenericExpressionVisitor<IMappedExpression>.Process(Item, mapped => mapped.Remap(offset, new Dictionary<Expression, Expression>()));
      return new ItemProjectorExpression(item, dataSource, Context);
    }

    public ItemProjectorExpression Remap(RecordSet dataSource, int[] columnMap)
    {
      var item = GenericExpressionVisitor<IMappedExpression>.Process(Item, mapped => mapped.Remap(columnMap, new Dictionary<Expression, Expression>()));
      return new ItemProjectorExpression(item, dataSource, Context);
    }

    public LambdaExpression ToLambda(TranslatorContext context, IEnumerable<Parameter<Tuple>> tupleParameters)
    {
      return ExpressionMaterializer.MakeLambda(Item, context, tupleParameters);
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
      var newDataSource = ApplyParameterRewriter.Rewrite(
        DataSource.Provider,
        oldParameter,
        newParameter)
        .Result;
      return new ItemProjectorExpression(Item, newDataSource, Context);
    }

    public override string ToString()
    {
      return string.Format("ItemProjectorExpression: IsPrimitive = {0} Item = {1}, DataSource = {2}", IsPrimitive, Item, DataSource);
    }


    // Constructors

    public ItemProjectorExpression(Expression expression, RecordSet dataSource, TranslatorContext context)
      : base(ExtendedExpressionType.ItemProjector, expression.Type)
    {
      Item = expression;
      DataSource = dataSource;
      Context = context;
    }
  }
}