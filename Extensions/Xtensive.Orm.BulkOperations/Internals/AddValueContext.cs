using System.Linq.Expressions;
using Xtensive.Orm.Model;
using Xtensive.Sql.Dml;

namespace Xtensive.Orm.BulkOperations
{
  internal class AddValueContext
  {
    public bool EntityParamExists { get; set; }

    public SetDescriptor Descriptor { get; set; }

    public LambdaExpression Lambda { get; set; }
    public SetStatement Statement { get; set; }

    public FieldInfo Field { get; set; }

    public bool SubqueryExists { get; set; }
  }
}