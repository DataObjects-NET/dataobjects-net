// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class RangeProviderCompiler : TypeCompiler<RangeProvider>
  {
    private struct ExpressionData
    {
      public SqlExpression Expression;
      public readonly IEntire<Tuple> RangeBound;
      public readonly bool IsLowerBound;
      public readonly IList<SqlColumn> KeyColumns;

      public ExpressionData(SqlExpression expression, IEntire<Tuple> rangeBound, IList<SqlColumn> keyColumns, bool isLowerBound)
      {
        Expression = expression;
        RangeBound = rangeBound;
        KeyColumns = keyColumns;
        IsLowerBound = isLowerBound;
      }
    }

    private struct ExpressionHandler : ITupleActionHandler<ExpressionData>
    {
      public bool Execute<TFieldType>(ref ExpressionData actionData, int fieldIndex)
      {
        if (actionData.IsLowerBound)
        {
          var entireValueType = actionData.RangeBound.GetValueType(fieldIndex);
          switch (entireValueType) {
            case EntireValueType.PositiveInfinitesimal:
              actionData.Expression &= actionData.KeyColumns[fieldIndex] > Xtensive.Sql.Dom.Sql.Literal(actionData.RangeBound.GetValue<TFieldType>(fieldIndex));
              return false;
            case EntireValueType.NegativeInfinitesimal:
              actionData.Expression &= actionData.KeyColumns[fieldIndex] >= Xtensive.Sql.Dom.Sql.Literal(actionData.RangeBound.GetValue<TFieldType>(fieldIndex));
              return false;
            case EntireValueType.PositiveInfinity:
              actionData.Expression &= Xtensive.Sql.Dom.Sql.Constant("1") != Xtensive.Sql.Dom.Sql.Constant("1");
              return true;
            case EntireValueType.NegativeInfinity:
              return false;
            default:
              actionData.Expression &= actionData.KeyColumns[fieldIndex] >= Xtensive.Sql.Dom.Sql.Literal(actionData.RangeBound.GetValue<TFieldType>(fieldIndex));
              return false;
          }
        }
        else {
          var entireValueType = actionData.RangeBound.GetValueType(fieldIndex);
          switch (entireValueType) {
            case EntireValueType.PositiveInfinitesimal:
              actionData.Expression &= actionData.KeyColumns[fieldIndex] <= Xtensive.Sql.Dom.Sql.Literal(actionData.RangeBound.GetValue<TFieldType>(fieldIndex));
              return false;
            case EntireValueType.NegativeInfinitesimal:
              actionData.Expression &= actionData.KeyColumns[fieldIndex] < Xtensive.Sql.Dom.Sql.Literal(actionData.RangeBound.GetValue<TFieldType>(fieldIndex));
              return false;
            case EntireValueType.PositiveInfinity:
              return false;
            case EntireValueType.NegativeInfinity:
              actionData.Expression &= Xtensive.Sql.Dom.Sql.Constant("1")!=Xtensive.Sql.Dom.Sql.Constant("1");
              return true;
            default:
              actionData.Expression &= actionData.KeyColumns[fieldIndex] <= Xtensive.Sql.Dom.Sql.Literal(actionData.RangeBound.GetValue<TFieldType>(fieldIndex));
              return false;
            }
        }
      }
    }

    protected override ExecutableProvider Compile(RangeProvider provider)
    {
      var source = (SqlProvider)Compiler.Compile(provider.Source, true);
      if (source == null)
        return null;

      var queryRef = Xtensive.Sql.Dom.Sql.QueryRef(source.Query);
      SqlSelect query = Xtensive.Sql.Dom.Sql.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());

      var direction = provider.Range.GetDirection(AdvancedComparer<IEntire<Tuple>>.Default);
      var from = direction == Direction.Positive ? 
        provider.Range.EndPoints.First : 
        provider.Range.EndPoints.Second;
      var to = direction == Direction.Positive ?
        provider.Range.EndPoints.Second :
        provider.Range.EndPoints.First;

      var keyColumns = provider.Header.OrderInfo.OrderedBy.Select(pair => query.Columns[pair.Key]).ToList();
      var expressionData = new ExpressionData(null, from, keyColumns, true);
      var expressionHandler = new ExpressionHandler();
      from.Descriptor.Execute(expressionHandler, ref expressionData, Direction.Positive);
      to.Descriptor.Execute(expressionHandler, ref expressionData, Direction.Positive);

      query.Where = expressionData.Expression;

      return new SqlProvider(provider, query);
    }

    // Constructors

    public RangeProviderCompiler(Rse.Compilation.Compiler provider)
      : base(provider)
    {
    }
  }
}