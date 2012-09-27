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

namespace Xtensive.Orm.Internals
{
  [Serializable]
  internal sealed class DelayedSequence<T> : DelayedQueryResult<IEnumerable<T>>,
    IEnumerable<T>
  {
    public IEnumerator<T> GetEnumerator()
    {
      return Materialize(Transaction.Session).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    public DelayedSequence(Session session, TranslatedQuery<IEnumerable<T>> translatedQuery, ParameterContext parameterContext) 
      : base(session, translatedQuery, parameterContext)
    {
    }
  }
}