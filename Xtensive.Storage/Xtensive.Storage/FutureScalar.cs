// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.19

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Linq;

namespace Xtensive.Storage
{
  /// <summary>
  /// Future returning a scalar result.
  /// </summary>
  /// <typeparam name="T">The type of the result.</typeparam>
  [Serializable]
  public sealed class FutureScalar<T> : FutureBase<T>
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
    public FutureScalar(TranslatedQuery<T> translatedQuery, ParameterContext parameterContext) :
      base(translatedQuery, parameterContext)
    {}
  }
}