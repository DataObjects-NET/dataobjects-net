// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.27

using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Compilable;

namespace Xtensive.Storage.Providers.Sql.Servers.Firebird
{
  internal class SqlCompiler : Sql.SqlCompiler
  {
    /// <inheritdoc/>
    protected override SqlProvider VisitTake(TakeProvider provider)
    {
      var compiledSource = Compile(provider.Source);

      var query = ExtractSqlSelect(provider, compiledSource);
      var binding = CreateLimitOffsetParameterBinding(provider.Count);
      query.Limit = binding.ParameterReference;
      return CreateProvider(query, binding, provider, compiledSource);
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitSkip(SkipProvider provider)
    {
      var compiledSource = Compile(provider.Source);
      var binding = CreateLimitOffsetParameterBinding(provider.Count);

      SqlSelect query;
      if (compiledSource.Request.SelectStatement.Offset.IsNullReference() && compiledSource.Request.SelectStatement.Limit.IsNullReference())
        query = compiledSource.Request.SelectStatement.ShallowClone();
      else {
        var queryRef = compiledSource.PermanentReference;
        query = SqlDml.Select(queryRef);
        query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      }
      query.Offset = binding.ParameterReference;
      return CreateProvider(query, binding, provider, compiledSource);
    }



    // Constructors
    
    public SqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}