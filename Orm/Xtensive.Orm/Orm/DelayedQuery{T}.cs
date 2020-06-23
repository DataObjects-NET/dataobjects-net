// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Linq.Materialization;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Internals
{
  [Serializable]
  public sealed class DelayedQuery<T> : DelayedQuery, IEnumerable<T>
  {
    public IEnumerator<T> GetEnumerator() => Materialize<T>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Constructors

    internal DelayedQuery(Session session, TranslatedQuery translatedQuery, ParameterContext parameterContext)
      : base(session, translatedQuery, parameterContext)
    {}
  }
}