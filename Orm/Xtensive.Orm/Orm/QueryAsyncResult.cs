using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Internals;

namespace Xtensive.Orm
{
  public readonly struct QueryAsyncResult<TElement>
  {
    private readonly CompiledQueryRunner queryRunner;
    private readonly Func<QueryEndpoint, IQueryable<TElement>> query;
    private readonly CancellationToken token;

    public ValueTaskAwaiter<IEnumerable<TElement>> GetAwaiter() =>
      new ValueTask<IEnumerable<TElement>>(queryRunner.ExecuteAsync(query, token)).GetAwaiter();

    internal QueryAsyncResult(CompiledQueryRunner queryRunner, Func<QueryEndpoint, IQueryable<TElement>> query, in CancellationToken token)
    {
      this.queryRunner = queryRunner;
      this.query = query;
      this.token = token;
    }
  }
}