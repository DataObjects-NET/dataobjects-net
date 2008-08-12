// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using System.Linq;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  [Serializable]
  internal sealed class FilteringProvider : UnaryExecutableProvider
  {
    private readonly Func<Tuple, bool> predicate;

    protected override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context).Where(predicate);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider.Origin"/> property value.</param>
    /// <param name="source">The <see cref="UnaryExecutableProvider.Source"/> property value.</param>
    ///<param name="predicate">Filtering predicate.</param>
    public FilteringProvider(CompilableProvider origin, ExecutableProvider source, Func<Tuple, bool> predicate)
      : base(origin, source)
    {
      this.predicate = predicate;
    }
  }
}