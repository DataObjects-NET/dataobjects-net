// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Orm.Rse.Providers.Compilable
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

    
    protected override RecordSetHeader BuildHeader()
    {
      return Source.Header;
    }

    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
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