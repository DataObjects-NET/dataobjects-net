using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  public readonly struct DataReader: IEnumerator<Tuple>, IAsyncEnumerator<Tuple>
  {
    private readonly object source;
    private readonly CancellationToken token;
    private readonly DbDataReaderAccessor accessor;

    public bool IsInMemory => !(source is Command);

    public Tuple Current => source is Command command
      ? command.ReadTupleWith(accessor)
      : ((IEnumerator<Tuple>)source).Current;

    object IEnumerator.Current => Current;

    public bool MoveNext() => source is Command command
      ? command.NextRow()
      : ((IEnumerator<Tuple>) source).MoveNext();

    public async ValueTask<bool> MoveNextAsync() => source is Command command
      ? await command.NextRowAsync(token)
      : ((IEnumerator<Tuple>) source).MoveNext();

    public void Reset()
    {
      if (source is Command) {
        throw new NotSupportedException("Multiple enumeration is not supported.");
      }
      ((IEnumerator)source).Reset();
    }

    public void Dispose()
    {
      if (source is Command command) {
        command.Dispose();
      }
      else {
        ((IEnumerator<Tuple>) source).Dispose();
      }
    }

    public async ValueTask DisposeAsync()
    {
      if (source is Command command) {
        await command.DisposeAsync();
      }
      else {
        await ((IAsyncEnumerator<Tuple>) source).DisposeAsync();
      }
    }

    public DataReader(IEnumerable<Tuple> tuples)
    {
      source = tuples.GetEnumerator();
      accessor = null;
    }

    public DataReader(Command command, DbDataReaderAccessor accessor, CancellationToken token)
    {
      source = command;
      this.accessor = accessor;
      this.token = token;
    }
  }
}