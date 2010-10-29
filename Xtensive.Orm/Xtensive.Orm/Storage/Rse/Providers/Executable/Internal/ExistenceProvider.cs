// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.20

using System;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class ExistenceProvider : UnaryExecutableProvider<Compilable.ExistenceProvider>
  {
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      var result = new List<Tuple>(1);
      var source = Source.Enumerate(context);
      result.Add(Tuple.Create(source.Any()));
      return result;
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ExistenceProvider(Compilable.ExistenceProvider origin, ExecutableProvider source)
      : base(origin, source)
    {}
  }
}