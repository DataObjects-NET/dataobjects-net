// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.16

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Orm.Rse.Providers
{
  /// <summary>
  /// Compilable provider that iterates over <see cref="BinaryProvider.Right"/> 
  /// provider result for each item from the <see cref="BinaryProvider.Left"/> provider.
  /// </summary>
  [Serializable]
  public sealed class ApplyProvider : BinaryProvider,
    IInlinableProvider
  {
    /// <summary>
    /// Gets the apply parameter.
    /// </summary>
    public ApplyParameter ApplyParameter { get; }

    /// <summary>
    /// Gets a value indicating whether columns of this provider should be inlined.
    /// </summary>
    public bool IsInlined { get; }

    /// <summary>
    /// Gets apply type.
    /// </summary>
    public JoinType ApplyType { get; }

    /// <summary>
    /// Gets a value indicating whether applying of single or first row expected.
    /// </summary>
    public ApplySequenceType SequenceType { get; }

    /// <inheritdoc/>
    protected override string ParametersToString()
    {
      return $"{ApplyType} apply";
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ApplyProvider(ApplyParameter applyParameter, CompilableProvider left, CompilableProvider right)
      : this(applyParameter, left, right, false, ApplySequenceType.All, JoinType.Inner)
    {
    }


    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public ApplyProvider(ApplyParameter applyParameter, CompilableProvider left, CompilableProvider right, bool isInlined, ApplySequenceType applySequenceType, JoinType applyType)
      : base(ProviderType.Apply, left, right)
    {
      if (applyType is not JoinType.Inner and not JoinType.LeftOuter) {
        throw new ArgumentOutOfRangeException(nameof(applyType));
      }
      ApplyParameter = applyParameter;
      IsInlined = isInlined;
      SequenceType = applySequenceType;
      ApplyType = applyType;
    }
  }
}