using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  internal readonly struct Materializer
  {
    private readonly Func<TupleReader, Session, ParameterContext, object> materializeMethod;

    public SequenceQueryResult<T> Invoke<T>(TupleReader tupleReader, Session session, ParameterContext parameterContext)
    {
      var reader = (IMaterializingReader<T>)
        materializeMethod.Invoke(tupleReader, session, parameterContext);
      return new SequenceQueryResult<T>(reader);
    }

    public Materializer(Func<TupleReader,Session,ParameterContext,object> materializeMethod)
    {
      this.materializeMethod = materializeMethod;
    }
  }
}