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
using Xtensive.Storage.Rse;

namespace Xtensive.Storage.Linq.Linq2Rse
{
  public class RseQueryProvider : QueryProvider
  {
    protected override object Execute(Expression expression)
    {
      var compiler = new RseQueryTranslator(this, expression);
      var result = compiler.Translate();
      var shaper = result.Shaper;
      var recordSet = result.RecordSet;
      // TODO: Always use Shaper
      if (shaper != null)
        return shaper(recordSet);

      var arguments = expression.Type.GetGenericArguments();
      return recordSet.ToEntities(arguments[0]);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public RseQueryProvider(DomainModel model)
      : base(model)
    {
    }
  }
}