// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System.Linq.Expressions;

namespace Xtensive.Storage.Linq
{
  internal sealed class QueryProvider : QueryProviderBase
  {
    /// <inheritdoc/>
    protected override object Execute(Expression expression)
    {
      var compiler = new QueryTranslator(this, expression);
      var result = compiler.Translate();
      var shaper = result.Projector;
      var recordSet = result.RecordSet;
      
      // TODO: Always use Projector
      if (shaper != null) {
        var compiled = shaper.Compile();
        return compiled(recordSet);
      }

      var arguments = expression.Type.GetGenericArguments();
      return recordSet.ToEntities(arguments[0]);
    }


    // Constructors

    public QueryProvider()
    {
    }
  }
}