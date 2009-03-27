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
      var result = context.Translator.Translate();
      var rcr = new RedundantColumnRemover(result);
      result = rcr.RemoveRedundantColumn();
      var or = new OrderbyRewriter(result);
      result = or.Rewrite();
      return result;
    }


    // Constructors

    public QueryProvider()
    {
    }
  }
}