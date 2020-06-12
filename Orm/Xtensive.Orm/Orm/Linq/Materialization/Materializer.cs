using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Rse.Providers;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Linq.Materialization
{
  internal readonly struct Materializer
  {
    private readonly Func<TupleReader, Session, Dictionary<Parameter<Tuple>, Tuple>, ParameterContext, object> materializeMethod;

    public SequenceQueryResult<T> Invoke<T>(
      TupleReader tupleReader, Session session, Dictionary<Parameter<Tuple>,Tuple> tupleParameterBindings, ParameterContext parameterContext)
    {
      var reader = (MaterializingReader<T>)
        materializeMethod.Invoke(tupleReader, session, tupleParameterBindings, parameterContext);
      return new SequenceQueryResult<T>(reader);
    }

    public Materializer(
      Func<TupleReader,Session,Dictionary<Parameter<Tuple>,Tuple>,ParameterContext,object> materializeMethod)
    {
      this.materializeMethod = materializeMethod;
      throw new NotImplementedException();
    }
  }
}