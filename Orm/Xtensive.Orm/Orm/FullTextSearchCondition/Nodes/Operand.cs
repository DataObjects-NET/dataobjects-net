
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Internals;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public abstract class Operand : IOperand
  {
    public IOperator Source { get; private set; }

    public SearchConditionNodeType NodeType { get; private set; }

    public IOperator And()
    {
      return SearchConditionNodeFactory.CreateAnd(this);
    }

    public IOperator Or()
    {
      return SearchConditionNodeFactory.CreateOr(this);
    }

    public IOperator AndNot()
    {
      return SearchConditionNodeFactory.CreateAndNot(this);
    }

    public void AcceptVisitor(ISearchConditionNodeVisitor visitor)
    {
      AcceptVisitorInternal(visitor);
    }

    protected abstract void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor);

    internal Operand(SearchConditionNodeType nodeType, IOperator sourceOperator)
    {
      NodeType = nodeType;
      Source = sourceOperator;
    }
  }
}
