// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.07.15

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// A sequence of upgrade actions.
  /// </summary>
  public class UpgradeActionSequence: IEnumerable<string>
  {
    /// <summary>
    /// Gets the count of all actions in sequence.
    /// </summary>
    public int Count
    {
      get
      {
        return NonTransactionalPrologCommands.Count
          + CleanupDataCommands.Count
          + PreUpgradeCommands.Count
          + UpgradeCommands.Count
          + CopyDataCommands.Count
          + PostCopyDataCommands.Count
          + CleanupCommands.Count
          + NonTransactionalEpilogCommands.Count;
      }
    }

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

    /// <inheritdoc/>
    public IEnumerator<string> GetEnumerator()
    {
      return NonTransactionalPrologCommands
        .Concat(CleanupDataCommands)
        .Concat(PreUpgradeCommands)
        .Concat(UpgradeCommands)
        .Concat(CopyDataCommands)
        .Concat(PostCopyDataCommands)
        .Concat(CleanupCommands)
        .Concat(NonTransactionalEpilogCommands).GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="UpgradeActionSequence"/> class.
    /// </summary>
    public UpgradeActionSequence()
    {
      NonTransactionalPrologCommands = new List<string>();
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