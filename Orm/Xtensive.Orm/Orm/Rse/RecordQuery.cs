// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.08.22

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// Provides a wrapper for <see cref="Provider"/>, as well as wide
  /// range of extension methods (see <see cref="RecordQueryExtensions"/>)
  /// to operate with them.
  /// </summary>
  [Serializable]
  public sealed class RecordQuery
  {
    /// <summary>
    /// Gets the header of the <see cref="RecordQuery"/>.
    /// </summary>
    public RecordSetHeader Header { get { return Provider.Header; } }

    /// <summary>
    /// Gets the provider this <see cref="RecordQuery"/> is bound to.
    /// </summary>
    public CompilableProvider Provider { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return Provider.ToString();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider"><see cref="Provider"/> property value.</param>
    internal RecordQuery(CompilableProvider provider)
    {
      Provider = provider;
    }
  }
}