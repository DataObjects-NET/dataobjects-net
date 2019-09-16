using System.Linq.Expressions;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.BulkOperations
{
  internal class SetDescriptor
  {
    public FieldInfo Field { get; private set; }
    public ParameterExpression Parameter { get; private set; }
    public Expression Expression { get; private set; }

    public SetDescriptor(FieldInfo field, ParameterExpression parameter, Expression expression)
    {
      Field = field;
      Parameter = parameter;
      Expression = expression;
    }
  }
}