// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.22

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Base class for unary operation provider over the <see cref="Left"/> and <see cref="Right"/>.
  /// </summary>
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


    // Constructor

    protected BinaryProvider(CompilableProvider left, CompilableProvider right)
      : base(left, right)
    {
      Left = left;
      Right = right;
    }
  }
}