using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Linq;

namespace Xtensive.Orm
{
  public readonly struct QueryAsyncResult<TElement>: IAsyncEnumerable<TElement>
  {
    private readonly ParameterizedQuery parameterizedQuery;
    private readonly Session session;
    private readonly ParameterContext parameterContext;
    private readonly CancellationToken token;

    public ValueTaskAwaiter<IEnumerable<TElement>> GetAwaiter() =>
      new ValueTask<IEnumerable<TElement>>(
        parameterizedQuery.ExecuteAsync<IEnumerable<TElement>>(session, parameterContext, false, token)).GetAwaiter();

    public async IAsyncEnumerator<TElement> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
      using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken);
      var result = await parameterizedQuery.ExecuteAsync<IAsyncEnumerable<TElement>>(
        session, parameterContext, true, linkedTokenSource.Token);
      await foreach (var element in result.WithCancellation(linkedTokenSource.Token)) {
        yield return element;
      }
    }

    internal QueryAsyncResult(
      ParameterizedQuery parameterizedQuery,
      Session session,
      ParameterContext parameterContext,
      in CancellationToken token)
    {
      this.parameterizedQuery = parameterizedQuery;
      this.session = session;
      this.parameterContext = parameterContext;
      this.token = token;
    }
  }
}