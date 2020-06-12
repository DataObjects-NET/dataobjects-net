using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Orm.Linq.Materialization;

namespace Xtensive.Orm.Linq
{
  public readonly struct SequenceQueryResult<TItem> : IEnumerable<TItem>, IAsyncEnumerable<TItem>
  {
    private readonly MaterializingReader<TItem> reader;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TItem> GetEnumerator() => reader;

    public IAsyncEnumerator<TItem> GetAsyncEnumerator(CancellationToken cancellationToken = default) => reader;

    internal SequenceQueryResult(MaterializingReader<TItem> reader)
    {
      this.reader = reader;
    }
  }
}