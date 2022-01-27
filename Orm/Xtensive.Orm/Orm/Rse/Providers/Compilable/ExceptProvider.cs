// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.01

using System;
using Xtensive.Core;

namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Produces except operation between <see cref="BinaryProvider.Left"/> and 
  /// <see cref="BinaryProvider.Right"/> sources.
  /// </summary>
  [Serializable]
  public sealed class ExceptProvider : BinaryProvider
  {


    // Constructors

    private static RecordSetHeader BuildHeader(CompilableProvider left, CompilableProvider right)
    {
      var leftHeader = left.Header;
      var leftDescriptor = leftHeader.TupleDescriptor;
      var rightDescriptor = right.Header.TupleDescriptor;
      if (leftDescriptor != rightDescriptor) {
        throw new InvalidOperationException(String.Format(Strings.ExXCantBeExecuted, "Except operation"));
      }
      return leftHeader;
    }

    /// <summary>
    ///  Initializes a new instance of this class.
    /// </summary>
    /// <param name="left">The left provider to execute except operation.</param>
    /// <param name="right">The right provider to to execute except operation.</param>
    public ExceptProvider(CompilableProvider left, CompilableProvider right)
      : base(ProviderType.Except, BuildHeader(left, right), left, right)
    {
    }
  }
}