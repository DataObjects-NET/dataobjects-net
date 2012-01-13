// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Base class for binary operation provider over 
  /// the <see cref="Left"/> and <see cref="Right"/> providers.
  /// </summary>
  [Serializable]
  public abstract class BinaryProvider : CompilableProvider
  {
    /// <summary>
    /// Left source.
    /// </summary>
    public CompilableProvider Left { get; private set; }

    /// <summary>
    /// Right source.
    /// </summary>
    public CompilableProvider Right { get; private set; }

    /// <inheritdoc/>
    protected override RecordSetHeader BuildHeader()
    {
      return Left.Header.Join(Right.Header);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="type">The type of provider.</param>
    /// <param name="left">The <see cref="Left"/> provider.</param>
    /// <param name="right">The <see cref="Left"/> provider.</param>
    protected BinaryProvider(ProviderType type, CompilableProvider left, CompilableProvider right)
      : base(type, left, right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      Left = left;
      Right = right;
    }
  }
}