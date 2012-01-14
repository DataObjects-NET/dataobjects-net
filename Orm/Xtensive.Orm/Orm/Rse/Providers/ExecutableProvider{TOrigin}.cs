// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.13

using System;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Abstract base class for <typeparamref name="TOrigin"/>-typed executable providers.
  /// </summary>
  /// <typeparam name="TOrigin">The type of the <see cref="Origin"/>.</typeparam>
  [Serializable]
  public abstract class ExecutableProvider<TOrigin> : ExecutableProvider
    where TOrigin: CompilableProvider
  {
    /// <summary>
    /// Gets the provider this provider is compiled from.
    /// </summary>
    public new TOrigin Origin {
      get { return (TOrigin) base.Origin; }
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="Origin"/> property value.</param>
    /// <param name="sources">The <see cref="Provider.Sources"/> property value.</param>
    protected ExecutableProvider(TOrigin origin, params ExecutableProvider[] sources)
      : base(origin, sources)
    {
    }
  }
}