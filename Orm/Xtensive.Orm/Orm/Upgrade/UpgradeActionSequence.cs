// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.07.15

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// A sequence of upgrade actions.
  /// </summary>
  public sealed class UpgradeActionSequence : IEnumerable<string>
  {
    /// <summary>
    /// Gets the number of all actions in sequence (including non-transactional commands).
    /// </summary>
    public int Count =>
      NonTransactionalPrologCommands.Count
      + PreCleanupDataCommands.Count
      + CleanupDataCommands.Count
      + PreUpgradeCommands.Count
      + UpgradeCommands.Count
      + CopyDataCommands.Count
      + PostCopyDataCommands.Count
      + CleanupCommands.Count
      + NonTransactionalEpilogCommands.Count;

    /// <summary>
    /// Gets or sets pre-cleanup data commands.
    /// </summary>
    /// <value>The pre-cleanup data commands.</value>
    public List<string> PreCleanupDataCommands { get; private set; }

    /// <summary>
    /// Gets or sets the cleanup data commands.
    /// </summary>
    /// <value>The cleanup data commands.</value>
    public List<string> CleanupDataCommands { get; private set; }

    /// <summary>
    /// Gets or sets the pre upgrade commands.
    /// </summary>
    /// <value>The pre upgrade commands.</value>
    public List<string> PreUpgradeCommands { get; private set; }

    /// <summary>
    /// Gets or sets the upgrade commands.
    /// </summary>
    /// <value>The upgrade commands.</value>
    public List<string> UpgradeCommands { get; private set; }

    /// <summary>
    /// Gets or sets the copy data commands.
    /// </summary>
    /// <value>The copy data commands.</value>
    public List<string> CopyDataCommands { get; private set; }

    /// <summary>
    /// Gets or sets the post copy data commands.
    /// </summary>
    /// <value>The post copy data commands.</value>
    public List<string> PostCopyDataCommands { get; private set; }

    /// <summary>
    /// Gets or sets the cleanup commands.
    /// </summary>
    /// <value>The cleanup commands.</value>
    public List<string> CleanupCommands { get; private set; }

    /// <summary>
    /// Gets or sets the non transactional epilog commands.
    /// </summary>
    /// <value>The non transactional epilog commands.</value>
    public List<string> NonTransactionalEpilogCommands { get; private set; }

    /// <summary>
    /// Gets or sets the non transactional prolog commands.
    /// </summary>
    /// <value>The non transactional prolog commands.</value>
    public List<string> NonTransactionalPrologCommands { get; private set; }

    /// <summary>
    /// Handles action sequence with specified processors.
    /// </summary>
    /// <param name="regularProcessor">Transactional processor.</param>
    /// <param name="nonTransactionalProcessor">Non-transactional processor.</param>
    public void ProcessWith(Action<IEnumerable<string>> regularProcessor, Action<IEnumerable<string>> nonTransactionalProcessor)
    {
      ArgumentValidator.EnsureArgumentNotNull(regularProcessor, nameof(regularProcessor));
      ArgumentValidator.EnsureArgumentNotNull(nonTransactionalProcessor, nameof(nonTransactionalProcessor));

      if (NonTransactionalPrologCommands.Count > 0) {
        nonTransactionalProcessor.Invoke(NonTransactionalPrologCommands);
      }

      foreach (var batch in EnumerateTransactionalCommandBatches()) {
        regularProcessor.Invoke(batch);
      }

      if (NonTransactionalEpilogCommands.Count > 0) {
        nonTransactionalProcessor.Invoke(NonTransactionalEpilogCommands);
      }
    }

    /// <summary>
    /// Asynchronously handles action sequence with specified processors.
    /// </summary>
    /// <remarks> Multiple active operations are not supported. Use <see langword="await"/>
    /// to ensure that all asynchronous operations have completed.</remarks>
    /// <param name="regularProcessor">Transactional processor.</param>
    /// <param name="nonTransactionalProcessor">Non-transactional processor.</param>
    /// <param name="token">The cancellation token to interrupt asynchronous execution if needed.</param>
    public async Task ProcessWithAsync(
      Func<IEnumerable<string>, CancellationToken, Task> regularProcessor,
      Func<IEnumerable<string>, CancellationToken, Task> nonTransactionalProcessor,
      CancellationToken token)
    {
      ArgumentValidator.EnsureArgumentNotNull(regularProcessor, nameof(regularProcessor));
      ArgumentValidator.EnsureArgumentNotNull(nonTransactionalProcessor, nameof(nonTransactionalProcessor));

      if (NonTransactionalPrologCommands.Count > 0) {
        await nonTransactionalProcessor.Invoke(NonTransactionalPrologCommands, token).ConfigureAwaitFalse();
      }

      foreach (var batch in EnumerateTransactionalCommandBatches()) {
        await regularProcessor.Invoke(batch, token).ConfigureAwaitFalse();
      }

      if (NonTransactionalEpilogCommands.Count > 0) {
        await nonTransactionalProcessor.Invoke(NonTransactionalEpilogCommands, token).ConfigureAwaitFalse();
      }
    }

    private IEnumerable<IReadOnlyCollection<string>> EnumerateTransactionalCommandBatches()
    {
      IEnumerable<IReadOnlyCollection<string>> EnumerateAllCommandBatches()
      {
        yield return PreCleanupDataCommands;
        yield return CleanupDataCommands;
        yield return PreUpgradeCommands;
        yield return UpgradeCommands;
        yield return CopyDataCommands;
        yield return PostCopyDataCommands;
        yield return CleanupCommands;
      }

      foreach (var p in EnumerateAllCommandBatches().Where(batch => batch.Count > 0)) {
        yield return p;
      }
    }

    /// <inheritdoc/>
    public IEnumerator<string> GetEnumerator()
    {
      var commands = new List<string>(Count);
      ProcessWith(commands.AddRange, commands.AddRange);
      return commands.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Initializes a new instance of the <see cref="UpgradeActionSequence"/> class.
    /// </summary>
    public UpgradeActionSequence()
    {
      NonTransactionalPrologCommands = new List<string>();
      PreCleanupDataCommands = new List<string>();
      CleanupDataCommands = new List<string>();
      PreUpgradeCommands = new List<string>();
      UpgradeCommands = new List<string>();
      CopyDataCommands = new List<string>();
      PostCopyDataCommands = new List<string>();
      CleanupCommands = new List<string>();
      NonTransactionalEpilogCommands = new List<string>();
    }
  }
}