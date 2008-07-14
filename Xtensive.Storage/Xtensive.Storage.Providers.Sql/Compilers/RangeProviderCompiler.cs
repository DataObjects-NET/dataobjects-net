// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.14

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Rse.Compilation;
using Xtensive.Storage.Rse.Providers;
using Xtensive.Storage.Rse.Providers.Declaration;

namespace Xtensive.Storage.Providers.Sql.Compilers
{
  public class RangeProviderCompiler : ProviderCompiler<RangeProvider>
  {
    private readonly ExecutionContext executionContext;
    private DomainHandler domainHandler;

    protected override Provider Compile(RangeProvider provider)
    {
      var source = provider.Source.Compile() as SqlProvider;
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

      SqlExpression rangeExpression = Xtensive.Sql.Dom.Sql.Constant("1") == Xtensive.Sql.Dom.Sql.Constant("1");

      for (int i = 0; i < from.Count; i++) {
        var valueType = from.GetValueType(i);
        switch (valueType) {
        case EntireValueType.PositiveInfinitesimal:
          break;
        case EntireValueType.NegativeInfinitesimal:
//          from.GetValue(i);
//          rangeExpression &= 
          break;
        case EntireValueType.PositiveInfinity:
          rangeExpression &= Xtensive.Sql.Dom.Sql.Constant("1") != Xtensive.Sql.Dom.Sql.Constant("1");
          continue;
        case EntireValueType.NegativeInfinity:
          continue;
        default:
          break;
        }
      }


      throw new System.NotImplementedException();
    }

    // Constructors

    public RangeProviderCompiler(Rse.Compilation.CompilerResolver resolver)
      : base(resolver)
    {
      executionContext = ((CompilerResolver)resolver).ExecutionContext;
      domainHandler = (DomainHandler)executionContext.DomainHandler;
    }
  }
}