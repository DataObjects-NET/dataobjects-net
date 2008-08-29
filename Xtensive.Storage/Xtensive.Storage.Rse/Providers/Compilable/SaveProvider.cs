// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Saves its <see cref="UnaryProvider.Source"/> under specified
  /// <see cref="Name"/>.
  /// </summary>
  public class SaveProvider : UnaryProvider
  {
    /// <summary>
    /// Gets the scope of saved data.
    /// </summary>
    public TemporaryDataScope Scope { get; private set; }

    /// <summary>
    /// Gets the name of saved data.
    /// </summary>
    public string Name { get; private set; }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="scope">The <see cref="Scope"/> property value.</param>
    /// <param name="name">The <see cref="Name"/> property value.</param>
    public SaveProvider(CompilableProvider source, TemporaryDataScope scope, string name)
      : base(source)
    {
      Scope = scope;
      Name = name;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    public SaveProvider(CompilableProvider source)
      : base(source)
    {
    }
  }
}