// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.05

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Provides access to some previously stored named <see cref="RecordSet"/> 
  /// or stores the specified <see cref="Source"/> with the specified <see cref="Name"/>.
  /// </summary>
  [Serializable]
  public sealed class StoreProvider : LocationAwareProvider
  {
    private const string ToStringFormat = "{0}, '{1}'";

    private readonly RecordSetHeader header;

    /// <summary>
    /// Gets the scope of saved data.
    /// </summary>
    public TemporaryDataScope Scope { get; private set; }

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
    public override string ParametersToString()
    {
      return string.Format(ToStringFormat, Scope, Name);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="header">The <see cref="Provider.Header"/> property value.</param>
    /// <param name="scope">The <see cref="Scope"/> property value.</param>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    public StoreProvider(RecordSetHeader header, TemporaryDataScope scope, string name)
      : base (ProviderType.Store, RseCompiler.DefaultServerLocation)
    {
      ArgumentValidator.EnsureArgumentNotNull(header, "header");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      this.header = header;
      Scope = scope;
      Name = name;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    /// <param name="scope">The <see cref="Scope"/> property value.</param>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    public StoreProvider(Provider source, TemporaryDataScope scope, string name)
      : base(ProviderType.Store, RseCompiler.DefaultServerLocation, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      Scope = scope;
      Name = name;
      Source = source;
      header = source.Header;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public StoreProvider(Provider source)
      : base(ProviderType.Store, RseCompiler.DefaultServerLocation, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      Scope = TemporaryDataScope.Enumeration;
      Name = Guid.NewGuid().ToString();
      Source = source;
      header = source.Header;
    }
  }
}