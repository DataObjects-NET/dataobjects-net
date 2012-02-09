// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.04.01

using System;
using System.Diagnostics;
using Xtensive.Collections;
using Xtensive.Internals.DocTemplates;


namespace Xtensive.Orm.Rse.Providers.Compilable
{
  /// <summary>
  /// Produces except operation between <see cref="BinaryProvider.Left"/> and 
  /// <see cref="BinaryProvider.Right"/> sources.
  /// </summary>
  [Serializable]
  public class ExceptProvider : BinaryProvider
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
      if (left.CompareTo(right) != 0)
        throw new InvalidOperationException(String.Format(Strings.ExXCantBeExecuted, "Except operation"));
    }


    // Constructors

    /// <summary>
    ///  <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="left">The left provider to execute except operation.</param>
    /// <param name="right">The right provider to to execute except operation.</param>
    public ExceptProvider(CompilableProvider left, CompilableProvider right)
      : base(ProviderType.Except, left, right)
    {
    }
  }
}