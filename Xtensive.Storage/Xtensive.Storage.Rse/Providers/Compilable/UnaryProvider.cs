// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Base class for unary operation provider over the <see cref="Source"/>.
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

    protected UnaryProvider(CompilableProvider provider)
      : base(provider)
    {
      Source = provider;
    }
  }
}