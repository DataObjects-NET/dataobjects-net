// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Provides access to some previously stored named <see cref="Provider"/> 
  /// or stores the specified <see cref="Source"/> with the specified <see cref="Name"/>.
  /// </summary>
  [Serializable]
  public sealed class StoreProvider : CompilableProvider
  {
    private readonly RecordSetHeader header;

    /// <summary>
    /// Gets the name of saved data.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Source provider.
    /// </summary>
    public Provider Source { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return Name;
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="header">The <see cref="Provider.Header"/> property value.</param>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    public StoreProvider(RecordSetHeader header, string name)
      : base (ProviderType.Store)
    {
      ArgumentValidator.EnsureArgumentNotNull(header, "header");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");

      Name = name;

      this.header = header;

      Initialize();
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    public StoreProvider(Provider source, string name)
      : base(ProviderType.Store, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");

      Name = name;
      Source = source;

      header = source.Header;

      Initialize();
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public StoreProvider(Provider source)
      : base(ProviderType.Store, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");

      Name = Guid.NewGuid().ToString();
      Source = source;

      header = source.Header;

      Initialize();
    }
  }
}