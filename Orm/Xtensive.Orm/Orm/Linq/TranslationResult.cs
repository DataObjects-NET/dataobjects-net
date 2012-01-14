// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.10

using System;
using System.Diagnostics;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Linq
{
  public abstract class TranslationResult
  {
    public TranslatedQuery UntypedQuery { get; private set; }
    public RecordQuery RecordQuery { get; private set; }


    // Constructors

    protected TranslationResult(TranslatedQuery untypedQuery, RecordQuery recordQuery)
    {
      UntypedQuery = untypedQuery;
      RecordQuery = recordQuery;
    }
  }

  public sealed class TranslationResult<TResult> : TranslationResult
  {
    public TranslatedQuery<TResult> Query { get; private set; }
    

    // Constructors

    public TranslationResult(TranslatedQuery<TResult> query, RecordQuery recordQuery)
      : base(query, recordQuery)
    {
      Query = query;
    }
  }
}