// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Linq
{
  public class QueryProvider : Linq.QueryProviderBase
  {
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

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="model">The model to use.</param>
    public QueryProvider(DomainModel model)
      : base(model)
    {
    }
  }
}