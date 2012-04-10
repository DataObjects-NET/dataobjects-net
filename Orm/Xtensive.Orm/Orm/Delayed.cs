// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections.Generic;

using Xtensive.Parameters;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm
{
  /// <summary>
  /// Future returning a scalar result.
  /// </summary>
  /// <typeparam name="T">The type of the result.</typeparam>
  [Serializable]
  public sealed class Delayed<T> : DelayedQueryResult<T>
  {
    /// <summary>
    /// Gets the result.
    /// </summary>
    public T Value {
      get {
        var session = Session.Current ?? transaction.Session;
        return Materialize(transaction.Session);
      }
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="session">The session.</param>
    /// <param name="translatedQuery">The translated query.</param>
    /// <param name="parameterContext">The parameter context.</param>
    internal Delayed(Session session, TranslatedQuery<T> translatedQuery, ParameterContext parameterContext) :
      base(session, translatedQuery, parameterContext)
    {
    }
  }
}