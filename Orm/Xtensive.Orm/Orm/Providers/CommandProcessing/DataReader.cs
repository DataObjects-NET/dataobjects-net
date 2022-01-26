// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// <see cref="DataReader"/> behaves as synchronous or asynchronous enumerator
  /// over either regular <see cref="IEnumerable{T}"/> of <see cref="Tuple"/>s
  /// or over the running <see cref="Command"/> instance.
  /// </summary>
  public readonly struct DataReader: IEnumerator<Tuple>, IAsyncEnumerator<Tuple>
  {
    private readonly object source;
    private readonly CancellationToken token;
    private readonly DbDataReaderAccessor accessor;

    /// <summary>
    /// Indicates current <see cref="DataReader"/> is built
    /// over <see cref="IEnumerable{T}"/> of <see cref="Tuple"/>s data source.
    /// </summary>
    public bool IsInMemory => !(source is Command);

    /// <inheritdoc cref="IEnumerator{T}.Current"/>
    public Tuple Current => source is Command command
      ? command.ReadTupleWith(accessor)
      : ((IEnumerator<Tuple>)source).Current;

    /// <inheritdoc/>
    object IEnumerator.Current => Current;

    /// <inheritdoc/>
    public bool MoveNext()
    {
      if (!(source is Command command)) {
        return ((IEnumerator<Tuple>) source).MoveNext();
      }

      if (command.NextRow()) {
        return true;
      }

      // We don't need the command anymore because all records are processed to the moment.
      command.Dispose();
      return false;
    }

    /// <inheritdoc/>
    public async ValueTask<bool> MoveNextAsync()
    {
      if (!(source is Command command)) {
        return ((IEnumerator<Tuple>) source).MoveNext();
      }

      if (await command.NextRowAsync(token).ConfigureAwait(false)) {
        return true;
      }

      // We don't need the command anymore because all records are processed to the moment.
      await command.DisposeAsync().ConfigureAwait(false);
      return false;
    }

    /// <inheritdoc/>
    public void Reset()
    {
      if (source is Command) {
        throw new NotSupportedException("Multiple enumeration is not supported.");
      }
      ((IEnumerator)source).Reset();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      if (source is Command command) {
        command.Dispose();
      }
      else {
        ((IEnumerator<Tuple>) source).Dispose();
      }
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
      if (source is Command command) {
        await command.DisposeAsync().ConfigureAwait(false);
      }
      else {
        await ((IAsyncEnumerator<Tuple>) source).DisposeAsync().ConfigureAwait(false);
      }
    }

    /// <summary>
    /// Creates <see cref="DataReader"/> wrapping enumerable collection of <see cref="Tuple"/>s.
    /// </summary>
    /// <param name="tuples">Collection of <see cref="Tuple"/>s to read from.</param>
    public DataReader(IEnumerable<Tuple> tuples)
    {
      source = tuples.GetEnumerator();
      accessor = null;
      token = CancellationToken.None;
    }

    /// <summary>
    /// Creates <see cref="DataReader"/> wrapping active <see cref="Command"/> instance.
    /// </summary>
    /// <param name="command"><see cref="Command"/> instance to read data from.</param>
    /// <param name="accessor"><see cref="DbDataReaderAccessor"/> instance
    /// transforming raw database records to <see cref="Tuple"/>s.</param>
    /// <param name="token"><see cref="CancellationToken"/> to terminate operation if necessary.</param>
    public DataReader(Command command, DbDataReaderAccessor accessor, CancellationToken token)
    {
      source = command;
      this.accessor = accessor;
      this.token = token;
    }
  }
}
