using System;
using System.Collections.Generic;
using System.Text;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.PostgreSql.Rse
{
  internal sealed class PgSqlAggregateColumnDesrciptor : AggregateColumnDescriptor
  {
    public (int Precision, int Scale)? DecimalParametersHint { get; private set; }

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    /// <param name="name"><see cref="Name"/> property value.</param>
    /// <param name="index"><see cref="SourceIndex"/> property value.</param>
    /// <param name="aggregateType">The <see cref="AggregateType"/> property value.</param>
    /// <param name="decimalParametersHint">Additional informantion about decimal column type.</param>
    public PgSqlAggregateColumnDesrciptor(string name, int index, AggregateType aggregateType, (int Precision, int Scale) decimalParametersHint)
      : base(name, index, aggregateType)
    {
      DecimalParametersHint = decimalParametersHint;
    }
  }
}
