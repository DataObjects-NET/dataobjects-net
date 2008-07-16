// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// An abstract base class for executable provider having a single data <see cref="Source"/>.
  /// </summary>
  [Serializable]
  public abstract class UnaryExecutableProvider: ExecutableProvider
  {
    /// <summary>
    /// Gets the only data source of this provider.
    /// </summary>
    public Provider Source { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider.Origin"/> property value.</param>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public UnaryExecutableProvider(CompilableProvider origin, Provider source)
      : base(origin, source)
    {
      Source = source;
    }
  }
}