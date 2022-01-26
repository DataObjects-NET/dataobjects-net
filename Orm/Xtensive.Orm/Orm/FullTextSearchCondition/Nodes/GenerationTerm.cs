// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// <see cref="IGenerationTerm"/> implementation.
  /// </summary>
  public sealed class GenerationTerm : Operand, IGenerationTerm, IWeighableTerm
  {
    /// <inheritdoc/>
    public GenerationType GenerationType { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyList<string> Terms { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal GenerationTerm(IOperator source, GenerationType generationType, ICollection<string> terms)
      : base(SearchConditionNodeType.GenerationTerm, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(terms, "terms");
      if (terms.Count==0)
        throw new ArgumentException(Strings.ExCollectionIsEmpty, "terms");
      if (terms.Any(c=>c.IsNullOrEmpty() || c.Trim().IsNullOrEmpty()))
        throw new ArgumentException(Strings.ExCollectionCannotContainAnyNeitherNullOrEmptyStringValues, "terms");
      GenerationType = generationType;
      Terms = terms.ToList().AsReadOnly();
    }
  }
}