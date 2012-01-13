// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.07

using System;
using System.Linq;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Orm.Rse.Providers.Compilable;
using System.Collections.Generic;

namespace Xtensive.Orm.Providers.Sql
{
  public abstract class ManualPagingSqlCompiler : SqlCompiler
  {
    protected override SqlProvider VisitSkip(SkipProvider provider)
    {
      var skipParameterBinding = CreateLimitOffsetParameterBinding(provider.Count);
      var bindings = new List<QueryParameterBinding> {skipParameterBinding};

      var compiledSource = Compile(provider.Source);
      var source = compiledSource.Request.SelectStatement;
      var queryRef = SqlDml.QueryRef(source);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = queryRef.Columns.Last() > skipParameterBinding.ParameterReference;
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    protected override SqlProvider VisitTake(TakeProvider provider)
    {
      var takeParameterBinding = CreateLimitOffsetParameterBinding(provider.Count);
      var bindings = new List<QueryParameterBinding> { takeParameterBinding };

      var compiledSource = Compile(provider.Source);
      var source = compiledSource.Request.SelectStatement;
      var queryRef = SqlDml.QueryRef(source);
      var query = SqlDml.Select(queryRef);
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = queryRef.Columns.Last() <= takeParameterBinding.ParameterReference;
      return CreateProvider(query, bindings, provider, compiledSource);
    }

    protected override SqlProvider VisitPaging(PagingProvider provider)
    {
      var fromParameterBinding = CreateLimitOffsetParameterBinding(provider.From);
      var toParameterBinding = CreateLimitOffsetParameterBinding(provider.To);
      var bindings = new List<QueryParameterBinding> { fromParameterBinding, toParameterBinding };

      var compiledSource = Compile(provider.Source);
      var source = compiledSource.Request.SelectStatement;
      var queryRef = SqlDml.QueryRef(source);
      var query = SqlDml.Select(queryRef);
      var rowNumberColumn = queryRef.Columns.Last();
      query.Columns.AddRange(queryRef.Columns.Cast<SqlColumn>());
      query.Where = SqlDml.Between(
        rowNumberColumn, 
        fromParameterBinding.ParameterReference,
        toParameterBinding.ParameterReference);
      return CreateProvider(query, bindings, provider, compiledSource);
    }


    // Constructors

    public ManualPagingSqlCompiler(HandlerAccessor handlers)
      : base(handlers)
    {
    }
  }
}