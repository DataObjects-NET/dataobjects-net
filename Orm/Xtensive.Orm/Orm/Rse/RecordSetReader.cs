// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Providers;
using EnumerationContext = Xtensive.Orm.Rse.Providers.EnumerationContext;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse
{
  /// <summary>
  /// <see cref="RecordSetReader"/> instance properly runs query execution and
  /// serves as both synchronous or asynchronous enumerator over resulting record set.
  /// </summary>
  public class RecordSetReader: IEnumerator<Tuple>, IAsyncEnumerator<Tuple>
  {
    private const string enumerationMarker = "Enumerated";
    private enum State
    {
      New,
      Prepared,
      InProgress,
      Finished
    }

    private readonly EnumerationContext context;
    private readonly ExecutableProvider provider;
    private readonly CancellationToken token;
    private readonly bool isGreedy;

    private State state = State.New;
    private bool enumerated;
    private DataReader dataReader;
    private ICompletableScope enumerationScope;

    /// <inheritdoc cref="IEnumerator{T}.Current"/>
    public Tuple Current => dataReader.Current;

    /// <inheritdoc/>
    object IEnumerator.Current => Current;

    /// <summary>
    /// Gets <see cref="EnumerationContext"/> associated with the current operation.
    /// </summary>
    public EnumerationContext Context => context;

    /// <summary>
    /// Gets <see cref="RecordSetHeader"/> describing content of each individual <see cref="Tuple"/>
    /// in the underlying record set.
    /// </summary>
    public RecordSetHeader Header => provider?.Header;

    /// <inheritdoc/>
    public void Reset()
    {
      if (state == State.InProgress || state == State.Finished) {
        dataReader.Reset();
      }
    }

    /// <inheritdoc/>
    public bool MoveNext()
    {
      switch (state) {
        case State.New:
          throw new InvalidOperationException("RecordSet is not prepared.");
        case State.Prepared:
          state = State.InProgress;
          goto case State.InProgress;
        case State.InProgress:
          try {
            if (dataReader.MoveNext()) {
              return true;
            }
          }
          catch {
            FinishEnumeration(true);

            throw;
          }

          FinishEnumeration(false);

          state = State.Finished;
          goto case State.Finished;
        case State.Finished:
        default:
          return false;
      }
    }

    /// <inheritdoc/>
    public async ValueTask<bool> MoveNextAsync()
    {
      switch (state) {
        case State.New:
          throw new InvalidOperationException("RecordSet is not prepared.");
        case State.Prepared:
          state = State.InProgress;
          goto case State.InProgress;
        case State.InProgress:
          try {
            if (await dataReader.MoveNextAsync().ConfigureAwaitFalse()) {
              return true;
            }
          }
          catch {
            FinishEnumeration(true);

            throw;
          }

          FinishEnumeration(false);
          state = State.Finished;
          goto case State.Finished;
        case State.Finished:
        default:
          return false;
      }
    }

    private async ValueTask Prepare(bool executeAsync)
    {
      enumerationScope = context.BeginEnumeration();
      enumerated = context.GetValue<bool>(provider, enumerationMarker);
      if (!enumerated) {
        if (executeAsync) {
          await provider.OnBeforeEnumerateAsync(context, token);
        }
        else {
          provider.OnBeforeEnumerate(context);
        }
        context.SetValue(provider, enumerationMarker, true);
      }

      try {
        dataReader = executeAsync
          ? await provider.OnEnumerateAsync(context, token).ConfigureAwaitFalse()
          : provider.OnEnumerate(context);

        if (isGreedy && !dataReader.IsInMemory) {
          var tuples = new List<Tuple>();
          if (executeAsync) {
            await using (dataReader.ConfigureAwaitFalse()) {
              while (await dataReader.MoveNextAsync().ConfigureAwaitFalse()) {
                tuples.Add(dataReader.Current);
              }
            }
          }
          else {
            using (dataReader) {
              while (dataReader.MoveNext()) {
                tuples.Add(dataReader.Current);
              }
            }
          }
          dataReader = new DataReader(tuples);
        }
      }
      catch {
        FinishEnumeration(true);
        throw;
      }
      state = State.Prepared;
    }

    private void FinishEnumeration(bool isError)
    {
      if (!enumerated) {
        provider?.OnAfterEnumerate(context);
      }

      if (!isError) {
        enumerationScope?.Complete();
      }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (state != State.New) {
        dataReader.Dispose();
      }
      enumerationScope?.Dispose();
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
      if (state != State.New) {
        await dataReader.DisposeAsync().ConfigureAwaitFalse();
      }
      enumerationScope?.Dispose();
    }

    private RecordSetReader(EnumerationContext context, ExecutableProvider provider, CancellationToken token = default)
    {
      this.context = context;
      this.provider = provider;
      this.token = token;
      isGreedy = context.CheckOptions(EnumerationContextOptions.GreedyEnumerator);
    }

    private RecordSetReader(DataReader dataReader)
    {
      this.dataReader = dataReader;
      state = State.Prepared;
    }

    /// <summary>
    /// Creates a <see cref="RecordSetReader"/> instance capable to read <paramref name="provider"/>
    /// execution results and bound to the specified <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The <see cref="EnumerationContext"/> instance associated with the query execution.</param>
    /// <param name="provider">The <see cref="ExecutableProvider"/> to be processed.</param>
    /// <returns><see cref="RecordSetReader"/> instance ready for enumeration.
    /// This means query is already executed but no records have been read yet.</returns>
    public static RecordSetReader Create(EnumerationContext context, ExecutableProvider provider)
    {
      var recordSet = new RecordSetReader(context, provider);
      var task = recordSet.Prepare(false);
      task.GetAwaiter().GetResult(); // Ensure exception, if any, is being thrown
      return recordSet;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="RecordSetReader"/> instance capable to read <paramref name="provider"/>
    /// execution results and bound to the specified <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The <see cref="EnumerationContext"/> instance associated with the query execution.</param>
    /// <param name="provider">The <see cref="ExecutableProvider"/> to be processed.</param>
    /// <param name="token">The <see cref="CancellationToken"/> allowing to cancel query execution if necessary.</param>
    /// <returns><see cref="RecordSetReader"/> instance ready for enumeration.
    /// This means query is already executed but no records have been read yet.</returns>
    public static async ValueTask<RecordSetReader> CreateAsync(
      EnumerationContext context, ExecutableProvider provider, CancellationToken token)
    {
      var recordSet = new RecordSetReader(context, provider, token);
      await recordSet.Prepare(true).ConfigureAwaitFalse();
      return recordSet;
    }

    /// <summary>
    /// Creates <see cref="RecordSetReader"/> instance wrapping the specified sequence of <paramref name="tuples"/>.
    /// </summary>
    /// <param name="tuples">A tuple sequence to be wrapped.</param>
    /// <returns><see cref="RecordSetReader"/> instance ready for enumeration.</returns>
    public static RecordSetReader Create(IEnumerable<Tuple> tuples) => new RecordSetReader(new DataReader(tuples));
  }
}