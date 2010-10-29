// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using Xtensive.Internals.DocTemplates;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// Fully transparent provider.
  /// </summary>
  [Serializable]
  public abstract class TransparentProvider<TOrigin> : UnaryExecutableProvider<TOrigin>
    where TOrigin: CompilableProvider
  {
    /// <inheritdoc/>
    protected internal override IEnumerable<Tuple> OnEnumerate(EnumerationContext context)
    {
      return Source.Enumerate(context);
    }

    /// <inheritdoc/>
    public override T GetService<T>()
    {
      return Source.GetService<T>();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="source">The <see cref="UnaryExecutableProvider{TOrigin}.Source"/> property value.</param>
    public TransparentProvider(TOrigin origin, ExecutableProvider source)
      : base(origin, source)
    {
    }
  }
}