using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class GenerationTerm : Operand, IGenerationTerm, IWeighableTerm
  {
    public GenerationType GenerationType { get; private set; }

    public ReadOnlyList<string> Terms { get; private set; }

    public override void AcceptVisitor(ISearchConditionNodeVisitor visitor)
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
      Terms = new ReadOnlyList<string>(terms.ToList());
    }
  }
}