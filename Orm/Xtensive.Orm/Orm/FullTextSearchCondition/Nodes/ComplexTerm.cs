
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class ComplexTerm : Operand, IComplexTerm
  {
    public IOperand RootOperand { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal ComplexTerm(IOperator source, IOperand operandsSequenceRoot)
      : base(SearchConditionNodeType.ComplexTerm, source)
    {
      ArgumentValidator.EnsureArgumentNotNull(operandsSequenceRoot, "operandsSequenceRoot");
      RootOperand = operandsSequenceRoot;
    }
  }
}
