// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordSet"/> <see cref="RecordSet.Provider"/>.
  /// </summary>
  [Serializable]
  public abstract class Provider : 
    IEnumerable<Tuple>,
    IHasServices
  {
    private RecordHeader header;

    /// <summary>
    /// Gets or sets the source providers 
    /// "consumed" by this provider to produce results of current provider.
    /// </summary>
    public Provider[] Sources { get; private set; }

    /// <summary>
    /// Gets the header of the record sequence this provide produces.
    /// </summary>
    public RecordHeader Header {
      get {
        EnsureHeaderIsBuilt();
        return header;
      }
    }

    /// <inheritdoc/>
    public abstract T GetService<T>()
      where T : class;

    /// <summary>
    /// Builds the <see cref="Header"/>.
    /// This method is invoked just once on each provider.
    /// </summary>
    /// <returns>Newly created <see cref="RecordHeader"/> to assign to <see cref="Header"/> property.</returns>
    protected abstract RecordHeader BuildHeader();

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public abstract IEnumerator<Tuple> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    /// <summary>
    /// Performs initialization of the provider.
    /// </summary>
    protected abstract void Initialize();

    #region Private \ internal methods

    private void EnsureHeaderIsBuilt()
    {
      if (header == null) lock (this) if (header == null)
        header = BuildHeader();
    }

    #endregion
    

    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sources"><see cref="Sources"/> property value.</param>
    protected Provider(params Provider[] sources)
    {
      Sources = sources;
      BuildHeader();
    }
  }
}