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

      // TODO: Always use Projector
      if (projector != null) {
        var compiledProjector = projector.Compile();
        return compiledProjector(recordSet);
      }
      else {
        var arguments = expression.Type.GetGenericArguments();
        return recordSet.ToEntities(arguments[0]);
      }
    }

    internal ResultExpression Compile(Expression expression)
    {
      var compiler = new QueryTranslator(this, expression);
      return compiler.Translate();
    }


    // Constructors

    public QueryProvider()
    {
    }
  }
}