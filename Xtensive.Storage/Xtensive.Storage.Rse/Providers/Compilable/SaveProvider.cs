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
  /// Saves its <see cref="UnaryProvider.Source"/> under specified
  /// <see cref="Name"/>.
  /// </summary>
  public class SaveProvider : CompilableProvider
  {
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
      return Source.Header;
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    /// <param name="scope">The <see cref="Scope{TContext}"/> property value.</param>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    public SaveProvider(Provider source, TemporaryDataScope scope, string name)
      : base(source)
    {
      Scope = scope;
      Name = name;
      Source = source;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="Source"/> property value.</param>
    public SaveProvider(Provider source)
      : this(source, TemporaryDataScope.Enumeration, Guid.NewGuid().ToString())
    {
    }
  }
}