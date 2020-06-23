using System;
using Xtensive.Core;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Linq.Materialization
{
  public readonly struct Materializer
  {
    private readonly Func<RecordSetReader, Session, ParameterContext, object> materializeMethod;

    public QueryResult<T> Invoke<T>(RecordSetReader recordSetReader, Session session, ParameterContext parameterContext)
    {
      var reader = (IMaterializingReader<T>)
        materializeMethod.Invoke(recordSetReader, session, parameterContext);
      return new QueryResult<T>(reader);
    }

    public Materializer(Func<RecordSetReader,Session,ParameterContext,object> materializeMethod)
    {
      this.materializeMethod = materializeMethod;
    }
  }
}