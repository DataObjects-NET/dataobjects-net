// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Loads the data previously saved with <see cref="SaveProvider"/>.
  /// </summary>
  [Serializable]
  public class LoadProvider : CompilableProvider
  {
    private readonly RecordSetHeader header;

    /// <summary>
    /// Gets the scope of saved data.
    /// </summary>
    public TemporaryDataScope Scope { get; private set; }

    /// <summary>
    /// Gets the name of saved data.
    /// </summary>
    public string Name { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="header">The <see cref="Provider.Header"/> property value.</param>
    /// <param name="scope">The <see cref="Scope"/> property value.</param>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    public LoadProvider(RecordSetHeader header, TemporaryDataScope scope, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(header, "header");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      this.header = header;
      Scope = scope;
      Name = name;
    }
  }
}