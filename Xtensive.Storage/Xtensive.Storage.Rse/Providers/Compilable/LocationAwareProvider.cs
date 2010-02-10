// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.03.12

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Abstract base class for <see cref="CompilableProvider"/> that's aware of its location.
  /// </summary>
  [Serializable]
  public abstract class LocationAwareProvider : CompilableProvider
  {
    /// <summary>
    /// Gets or sets execution site location.
    /// </summary>
    public Location Location { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected LocationAwareProvider(ProviderType type, Location location, params Provider[] sources)
      : base(type, sources)
    {
      Location = location;
    }
  }
}