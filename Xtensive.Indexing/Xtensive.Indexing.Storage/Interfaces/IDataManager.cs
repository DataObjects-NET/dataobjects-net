// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System.Collections.Generic;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Storage
{
  /// <summary>
  /// Data management API (DML API).
  /// Provides read-write access to any index 
  /// in the <see cref="IStorage"/>, allows to 
  /// execute queries on them.
  /// </summary>
  public interface IDataManager
  {
    /// <summary>
    /// Executes the specified command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <returns>Execution result.</returns>
    object Execute(Command command);

    /// <summary>
    /// Executes the specified sequence of commands.
    /// </summary>
    /// <param name="commands">The sequence of commands to execute.</param>
    /// <returns>Execution result (one per each command, if any).</returns>
    Dictionary<int, object> Execute(List<Command> commands);

    // Obsolete?
    /// <summary>
    /// Provides direct access to stored index.
    /// </summary>
    /// <param name="indexName">Name of the index to get.</param>
    /// <returns>An object allowing to manipulate the index.</returns>
    IIndex<Tuple, Tuple> GetIndex(string indexName);
  }
}