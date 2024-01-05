// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using Xtensive.Core;
using Xtensive.Orm.Rse;

namespace Xtensive.Orm.Linq.Materialization
{
  internal readonly struct Materializer
  {
    private readonly Func<RecordSetReader, Session, ParameterContext, object> materializeMethod;

    public QueryResult<T> Invoke<T>(RecordSetReader recordSetReader, Session session, ParameterContext parameterContext)
    {
      var reader = (IMaterializingReader<T>)
        materializeMethod.Invoke(recordSetReader, session, parameterContext);
      return new QueryResult<T>(reader, session.GetLifetimeToken());
    }

    public Materializer(Func<RecordSetReader,Session,ParameterContext,object> materializeMethod)
    {
      this.materializeMethod = materializeMethod;
    }
  }
}