// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.01

using System;
using System.Diagnostics;
using Xtensive.Collections;



namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Produces except operation between <see cref="BinaryProvider.Left"/> and 
  /// <see cref="BinaryProvider.Right"/> sources.
  /// </summary>
  [Serializable]
  public sealed class ExceptProvider : BinaryProvider
  {
    protected override RecordSetHeader BuildHeader()
    {
      EnsureIntersectIsPossible();
      return Left.Header;
    }

    private void EnsureIntersectIsPossible()
    {
      var left = Left.Header.TupleDescriptor;
      var right = Right.Header.TupleDescriptor;
      if (left!=right)
        throw new InvalidOperationException(String.Format(Strings.ExXCantBeExecuted, "Except operation"));
    }


    // Constructors

    /// <summary>
    ///  Initializes a new instance of this class.
    /// </summary>
    /// <param name="left">The left provider to execute except operation.</param>
    /// <param name="right">The right provider to to execute except operation.</param>
    public ExceptProvider(CompilableProvider left, CompilableProvider right)
      : base(ProviderType.Except, left, right)
    {
      Initialize();
    }
  }
}