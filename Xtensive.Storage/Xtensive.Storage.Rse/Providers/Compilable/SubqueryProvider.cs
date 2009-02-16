// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.16

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Rse.Providers.Compilable
{
  /// <summary>
  /// Compilable provider that iterates over <see cref="BinaryProvider.Right"/> provider result for each item from the <see cref="BinaryProvider.Left"/> provider.
  /// </summary>
  [Serializable]
  public sealed class SubqueryProvider : BinaryProvider
  {
    /// <summary>
    /// Gets or sets the left item parameter.
    /// </summary>
    public Parameter<Tuple> LeftItemParameter { get; set; }

    /// <summary>
    /// Indicates whether current join operation should be executed as left join.
    /// </summary>
    public bool LeftJoin { get; private set; }

    /// <inheritdoc/>
    public override string ParametersToString()
    {
      return string.Format(LeftJoin
        ? "Left subquery"
        : "Subquery");
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SubqueryProvider(Parameter<Tuple> leftItemParameter, CompilableProvider left, CompilableProvider right)
      : this(leftItemParameter, left, right, false)
    {}

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SubqueryProvider(Parameter<Tuple> leftItemParameter, CompilableProvider left, CompilableProvider right, bool leftJoin)
      : base(ProviderType.Subquery, left, right)
    {
      LeftItemParameter = leftItemParameter;
      LeftJoin = leftJoin;
    }
  }
}