// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.27

using System;
using System.Linq.Expressions;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse.Compilation.Expressions.Visitors;
using Xtensive.Storage;

namespace Xtensive.Storage.Rse.Compilation.Linq
{
  public class RseQueryProvider : QueryProvider
  {
    protected override object Execute(Expression expression)
    {
      expression = QueryPreprocessor.Translate(expression, Model);
      var rewriter = new RseQueryRewriter(this);
      expression  = rewriter.Rewrite(expression);
//      RecordSet result = RseQueryTranslator.Translate(expression, this);
      throw new NotImplementedException();
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RseQueryProvider(DomainModel model)
      : base(model)
    {
    }
  }
}