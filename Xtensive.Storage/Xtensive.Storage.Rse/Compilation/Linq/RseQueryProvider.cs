// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System;
using System.Collections;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;
using Xtensive.Storage;
using Xtensive.Storage.Rse.Compilation.Expressions;

namespace Xtensive.Storage.Rse.Compilation.Linq
{
  public class RseQueryProvider : QueryProvider
  {
    protected override object Execute(Expression expression)
    {
      expression = QueryPreprocessor.Translate(expression, Model);
      var rewriter = new RseQueryRewriter(this);
      expression  = rewriter.Rewrite(expression);
      var translator = new RseQueryTranslator(this);
      translator.Translate(expression);
      var shaper = translator.Shaper;
      var result = translator.Result;
      if (shaper != null)
        return shaper(result);
      var arguments = expression.Type.GetGenericArguments();
      var r = (IEnumerable)EntityMaterializer(result, arguments[0]);
      return r;
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RseQueryProvider(DomainModel model, Func<RecordSet, Type, IEnumerable> entityMaterializer)
      : base(model, entityMaterializer)
    {
    }
  }
}