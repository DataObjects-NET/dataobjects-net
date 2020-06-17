using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Rse.Providers;
using EnumerationContext = Xtensive.Orm.Rse.Providers.EnumerationContext;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Rse
{
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

    public Tuple Current => dataReader.Current;

    object IEnumerator.Current => Current;
    public EnumerationContext Context => context;
    public RecordSetHeader Header => provider?.Header;

    public void Reset()
    {
      if (state == State.InProgress || state == State.Finished) {
        dataReader.Reset();
      }
    }

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
            if (await dataReader.MoveNextAsync()) {
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
        provider.OnBeforeEnumerate(context);
        context.SetValue(provider, enumerationMarker, true);
      }

      try {
        dataReader = executeAsync
          ? await provider.OnEnumerateAsync(context, token)
          : provider.OnEnumerate(context);

        if (isGreedy && !dataReader.IsInMemory) {
          var tuples = new List<Tuple>();
          if (executeAsync) {
            await using (dataReader) {
              while (await dataReader.MoveNextAsync()) {
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

    public void Dispose()
    {
      if (state != State.New) {
        dataReader.Dispose();
      }
      enumerationScope?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
      if (state != State.New) {
        await dataReader.DisposeAsync();
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

    public static RecordSetReader Create(EnumerationContext context, ExecutableProvider provider)
    {
      var recordSet = new RecordSetReader(context, provider);
      var task = recordSet.Prepare(false);
      task.GetAwaiter().GetResult(); // Ensure exception, if any, is being thrown
      return recordSet;
    }

    public static async ValueTask<RecordSetReader> CreateAsync(
      EnumerationContext context, ExecutableProvider provider, CancellationToken token)
    {
      var recordSet = new RecordSetReader(context, provider, token);
      await recordSet.Prepare(true);
      return recordSet;
    }

    public static RecordSetReader Create(IEnumerable<Tuple> tuples)
    {
      return new RecordSetReader(new DataReader(tuples));
    }

  }
}