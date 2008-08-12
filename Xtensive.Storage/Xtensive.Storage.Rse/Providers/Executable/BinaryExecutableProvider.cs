// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.15

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Rse.Providers.Executable
{
  /// <summary>
  /// An abstract base class for executable provider having <see cref="Left"/> and <see cref="Right"/> data sources.
  /// </summary>
  [Serializable]
  public abstract class BinaryExecutableProvider: ExecutableProvider
  {
    /// <summary>
    /// Gets the "left" data source of this provider.
    /// </summary>
    public ExecutableProvider Left { get; private set; }

    /// <summary>
    /// Gets the "right" data source of this provider.
    /// </summary>
    public ExecutableProvider Right { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return Left.Header.Join(Right.Header);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="origin">The <see cref="ExecutableProvider.Origin"/> property value.</param>
    /// <param name="left">The <see cref="Left"/> property value.</param>
    /// <param name="right">The <see cref="Right"/> property value.</param>
    protected BinaryExecutableProvider(CompilableProvider origin, ExecutableProvider left, ExecutableProvider right)
      : base(origin, left, right)
    {
      Left = left;
      Right = right;
    }
  }
}