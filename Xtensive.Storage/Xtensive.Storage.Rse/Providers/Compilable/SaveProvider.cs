// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that wraps <see cref="UnaryProvider.Source"/> provider.
  /// </summary>
  public class SaveProvider : UnaryProvider
  {
    /// <summary>
    /// Gets or sets the name of saved context data.
    /// </summary>
    public string ResultName { get; private set; }


    // Constructor.

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="resultName">The <see cref="ResultName"/> property value.</param>
    public SaveProvider(CompilableProvider source, string resultName)
      : base(source)
    {
      ResultName = resultName;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    public SaveProvider(CompilableProvider source)
      : base(source)
    {
    }
  }
}