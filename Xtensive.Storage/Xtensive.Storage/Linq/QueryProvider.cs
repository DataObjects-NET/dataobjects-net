// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System.Linq.Expressions;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq
{
  internal sealed class QueryProvider : QueryProviderBase
  {
    /// <inheritdoc/>
    protected override object Execute(Expression expression)
    {
      var result = Compile(expression);
      var projector = result.Projector;
      var recordSet = result.RecordSet;

      var compiledProjector = projector.Compile();
      return compiledProjector(recordSet);
    }

    internal ResultExpression Compile(Expression expression)
    {
      var context = new TranslatorContext(expression);
      return context.Translator.Translate();
    }


    // Constructors

    public QueryProvider()
    {
    }
  }
}