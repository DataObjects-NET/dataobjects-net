// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.10

using System;
using System.Diagnostics;
using Xtensive.Orm.Rse;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Linq
{
  public abstract class TranslationResult
  {
    public TranslatedQuery UntypedQuery { get; private set; }
    public CompilableProvider QueryProvider { get; private set; }


    // Constructors

    protected TranslationResult(TranslatedQuery untypedQuery, CompilableProvider recordQuery)
    {
      UntypedQuery = untypedQuery;
      QueryProvider = recordQuery;
    }
  }

  public sealed class TranslationResult<TResult> : TranslationResult
  {
    public TranslatedQuery<TResult> Query { get; private set; }
    

    // Constructors

    public TranslationResult(TranslatedQuery<TResult> query, CompilableProvider recordQuery)
      : base(query, recordQuery)
    {
      Query = query;
    }
  }
}