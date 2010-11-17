// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;
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
      get { return Materialize(); }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="translatedQuery">The translated query.</param>
    /// <param name="parameterContext">The parameter context.</param>
    public Delayed(TranslatedQuery<T> translatedQuery, ParameterContext parameterContext) :
      base(translatedQuery, parameterContext)
    {}
  }
}