// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

using System;
using Xtensive.Core;


namespace Xtensive.Orm.Rse.Providers
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
    public CompilableProvider Left { get; }

    /// <summary>
    /// Right source.
    /// </summary>
    public CompilableProvider Right { get; }


    // Constructors

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The type of provider.</param>
    /// <param name="left">The <see cref="Left"/> provider.</param>
    /// <param name="right">The <see cref="Right"/> provider.</param>
    protected BinaryProvider(ProviderType type, CompilableProvider left, CompilableProvider right)
      : this(type, left.Header.Join(right.Header), left, right)
    {
    }

    /// <summary>
    ///   Initializes a new instance of this class.
    /// </summary>
    /// <param name="type">The type of provider.</param>
    /// <param name="header">The header of the resulting record set.</param>
    /// <param name="left">The <see cref="Left"/> provider.</param>
    /// <param name="right">The <see cref="Right"/> provider.</param>
    protected BinaryProvider(ProviderType type, RecordSetHeader header, CompilableProvider left, CompilableProvider right)
      : base(type, header, left, right)
    {
      Left = left;
      Right = right;
    }
  }
}