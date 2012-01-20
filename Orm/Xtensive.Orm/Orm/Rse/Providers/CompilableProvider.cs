// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.03

using System;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Abstract base class for any <see cref="RecordQuery"/> <see cref="RecordQuery.Provider"/>,
  /// that can be compiled.
  /// </summary>
  [Serializable]
  public abstract class CompilableProvider : Provider
  {
    /// <summary>
    /// Creates the <see cref="RecordQuery"/> wrapping this provider.
    /// </summary>
    public RecordQuery Result { get { return new RecordQuery(this); } }


    // Constructors

    /// <inheritdoc/>
    protected CompilableProvider(ProviderType type, params Provider[] sources)
      : base(type, sources)
    {
    }
  }
}