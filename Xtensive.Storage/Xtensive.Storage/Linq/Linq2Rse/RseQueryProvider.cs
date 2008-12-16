// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System;
using System.Collections;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Model;
using Xtensive.Storage;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Compilation.Expressions;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  public class RseQueryProvider : QueryProvider
  {
    protected override object Execute(Expression expression)
    {
      expression = QueryPreprocessor.Translate(expression, Model);
//      var rewriter = new RseQueryRewriter(this);
//      expression  = rewriter.Rewrite(expression);
      var compiler = new RseQueryTranslator(this);
      var result = compiler.Translate(expression);
      var shaper = result.Shaper;
      var rs = result.RecordSet;
      if (shaper != null)
        return shaper(rs);

      var arguments = expression.Type.GetGenericArguments();
      return EntityMaterializer(rs, arguments[0]);
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