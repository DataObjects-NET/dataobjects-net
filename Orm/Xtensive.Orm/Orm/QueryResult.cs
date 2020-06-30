using System.Collections;
using System.Collections.Generic;
using Xtensive.Orm.Linq.Materialization;

namespace Xtensive.Orm
{
  public readonly struct QueryResult<TItem> : IEnumerable<TItem>
  {
    private readonly IMaterializingReader<TItem> reader;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TItem> GetEnumerator() => reader.AsEnumerator();

    public async IAsyncEnumerable<TItem> AsAsyncEnumerable()
    {
      var enumerator = reader.AsAsyncEnumerator();
      while (await enumerator.MoveNextAsync().ConfigureAwait(false)) {
        yield return enumerator.Current;
      }
    }

    internal QueryResult(IMaterializingReader<TItem> reader)
    {
      this.reader = reader;
    }

    private class EnumerableReader: IMaterializingReader<TItem>
    {
      private readonly IEnumerable<TItem> items;

      public IEnumerator<TItem> AsEnumerator() => items.GetEnumerator();

      public IAsyncEnumerator<TItem> AsAsyncEnumerator() => throw new System.NotSupportedException();

      public EnumerableReader(IEnumerable<TItem> items)
      {
        this.items = items;
      }
    }

    internal QueryResult(IEnumerable<TItem> items)
    {
      reader = new EnumerableReader(items);
    }
  }
}