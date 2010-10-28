// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// An abstract base class for executable provider having a single data <see cref="Source"/>.
  /// </summary>
  [Serializable]
  public abstract class UnaryExecutableProvider<TOrigin> : ExecutableProvider<TOrigin>
    where TOrigin: CompilableProvider
  {
    /// <summary>
    /// Gets the only data source of this provider.
    /// </summary>
    public ExecutableProvider Source { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider{TOrigin}.Origin"/> property value.</param>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    protected UnaryExecutableProvider(TOrigin origin, ExecutableProvider source)
      : base(origin, source)
    {
      Source = source;
    }
  }
}