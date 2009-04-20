// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Base class for unary operation provider over the <see cref="Source"/> provider.
  /// </summary>
  public abstract class UnaryProvider : CompilableProvider
  {
    /// <summary>
    /// Source provider.
    /// </summary>
    public CompilableProvider Source { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return Source.Header;
    }


    // Constructor

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type of the provider.</param>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    protected UnaryProvider(ProviderType type, CompilableProvider source)
      : base(type, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      Source = source;
    }
  }
}