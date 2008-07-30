// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.08.22

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Provides access to a sequence of <see cref="Tuple"/>s
  /// exposed by its <see cref="Provider"/>, as well as wide
  /// range of extension methods (see <see cref="RecordSetExtensions"/>)
  /// to operate with them.
  /// </summary>
  [Serializable]
  public sealed class RecordSet : IEnumerable<Tuple>
  {
    /// <summary>
    /// Gets the header of the <see cref="RecordSet"/>.
    /// </summary>
    public RecordSetHeader Header
    {
      get { return Provider.Header; }
    }

    /// <summary>
    /// Gets the provider this <see cref="RecordSet"/> is bound to.
    /// </summary>
    public CompilableProvider Provider { get; private set; }

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator()
    {
      return Provider.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider"><see cref="Provider"/> property value.</param>
    internal RecordSet(CompilableProvider provider)
    {
      Provider = provider;
    }
  }
}