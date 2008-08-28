// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.25

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider for executing saved context data.
  /// </summary>
  [Serializable]
  public class LoadProvider : CompilableProvider
  {
    private readonly RecordSetHeader header;

    /// <summary>
    /// Gets or sets the name of saved context data.
    /// </summary>
    public string ResultName { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return header;
    }


    // Constructor.

     /// <summary>
     /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
     /// </summary>
     /// <param name="resultName">The <see cref="ResultName"/> property value.</param>
     /// <param name="header">The <see cref="Provider.Header"/> property value.</param>
    public LoadProvider(RecordSetHeader header, string resultName)
    {
      this.header = header;
      ResultName = resultName;
    }
  }
}