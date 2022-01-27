// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Provides access to some previously stored named <see cref="Provider"/> 
  /// or stores the specified <see cref="Source"/> with the specified <see cref="Name"/>.
  /// </summary>
  [Serializable]
  public sealed class StoreProvider : CompilableProvider
  {
    /// <summary>
    /// Gets the name of saved data.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Source provider.
    /// </summary>
    public Provider Source { get; }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return Name;
    }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    public StoreProvider(Provider source, string name)
      : base(ProviderType.Store, source.Header, source)
    {
      Name = name;
      Source = source;
    }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public StoreProvider(Provider source)
      : this(source, Guid.NewGuid().ToString())
    {
    }
  }
}