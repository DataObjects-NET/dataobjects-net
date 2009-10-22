// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.22

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  public class InProvider: UnaryExecutableProvider<Compilable.InProvider>
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
    public InProvider(Compilable.InProvider origin, ExecutableProvider source)
      : base(origin, source)
    {}
  }
}