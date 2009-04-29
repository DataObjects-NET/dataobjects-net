// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.04.27

using System;
using Xtensive.Core;
using Xtensive.Storage.Linq.Expressions;

namespace Xtensive.Storage.Internals
{
  internal class QueryCachingScope : SimpleScope<QueryCachingScope.Variator>
  {
    internal class Variator {}

    private ResultExpression compilationResult;

    public static new QueryCachingScope Current {
      get { return (QueryCachingScope) SimpleScope<Variator>.Current; }
    }

    /// <exception cref="NotSupportedException">Second attempt to set this property.</exception>
    public ResultExpression CompilationResult {
      get { return compilationResult; }
      set {
        if (compilationResult != null)
          throw Exceptions.AlreadyInitialized("CompilationResult");
        compilationResult = value;
      }
    }
  }
}