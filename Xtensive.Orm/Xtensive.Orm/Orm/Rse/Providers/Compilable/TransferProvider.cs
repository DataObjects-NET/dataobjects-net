// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.11

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Transfers further computations to the specified location.
  /// </summary>
  [Serializable]
  public sealed class TransferProvider : UnaryProvider
  {
    /// <summary>
    /// Gets <see cref="TransferType"/>.
    /// </summary>
    public TransferType Options { get; private set; }

    /// <summary>
    /// Gets the execution site location.
    /// </summary>
    public UrlInfo Location { get; private set; }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return Location.ToString();
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="options">The <see cref="Options"/> property value.</param>
    public TransferProvider(CompilableProvider source, TransferType options)
      : this(source, options, null)
    {
      Options = options;
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The <see cref="UnaryProvider.Source"/> property value.</param>
    /// <param name="options">The <see cref="Options"/> property value.</param>
    /// <param name="location">The <see cref="Location"/> property value.</param>
    public TransferProvider(CompilableProvider source, TransferType options, UrlInfo location)
      : base(ProviderType.Transfer, source)
    {
      Options = options;
      if (!ReferenceEquals(location, null))
        throw new NotSupportedException();
      Location = location;
    }
  }
}