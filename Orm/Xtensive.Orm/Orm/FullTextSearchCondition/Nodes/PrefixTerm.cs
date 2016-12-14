// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// <see cref="IPrefixTerm"/> implementation.
  /// </summary>
  public sealed class PrefixTerm : Operand, IPrefixTerm, IWeighableTerm
  {
    /// <inheritdoc/>
    public string Prefix { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal PrefixTerm(string prefix)
      : this(null, prefix)
    {
    }

    internal PrefixTerm(IOperator source, string prefix)
      : base(SearchConditionNodeType.Prefix, source)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmptyOrWhiteSpace(prefix, "prefix");
      Prefix = prefix;
    }
  }
}