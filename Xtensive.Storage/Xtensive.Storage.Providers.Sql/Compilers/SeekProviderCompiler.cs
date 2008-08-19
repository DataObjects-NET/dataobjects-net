// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.08.18

using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers.Compilable;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  internal sealed class SeekProviderCompiler : TypeCompiler<SeekProvider>
  {
    private struct ExpressionData
    {
      public SqlExpression Expression;
      public readonly Tuple Value;
      public readonly IList<SqlColumn> KeyColumns;

      public ExpressionData(SqlExpression expression, Tuple value, IList<SqlColumn> keyColumns)
      {
        Expression = expression;
        Value = value;
        KeyColumns = keyColumns;
      }
    }

    private struct ExpressionHandler : ITupleActionHandler<ExpressionData>
    {
      public bool Execute<TFieldType>(ref ExpressionData actionData, int fieldIndex)
      {
        actionData.Expression &= actionData.KeyColumns[fieldIndex] >= SqlFactory.Literal(actionData.Value.GetValue<TFieldType>(fieldIndex));
        return false;
      }
    }

    protected override ExecutableProvider Compile(SeekProvider provider)
    {
      var source = Compiler.Compile(provider.Source, true) as SqlProvider;
      if (source == null)
        return null;

      var query = source.Query.Clone() as SqlSelect;
      var tuple = provider.CompiledKey.Invoke();
      var keyColumns = provider.Header.Order.Select(pair => query.Columns[pair.Key]).ToList();
      var expressionData = new ExpressionData(null, tuple, keyColumns);
      var expressionHandler = new ExpressionHandler();
      tuple.Descriptor.Execute(expressionHandler, ref expressionData, Direction.Positive);

      query.Where = SqlExpression.IsNull(query.Where) ?
              expressionData.Expression :
              query.Where & expressionData.Expression;

      return new SqlProvider(provider, query, Handlers);
    }

    public SeekProviderCompiler(Rse.Compilation.Compiler compiler)
      : base(compiler)
    {
    }
  }
}