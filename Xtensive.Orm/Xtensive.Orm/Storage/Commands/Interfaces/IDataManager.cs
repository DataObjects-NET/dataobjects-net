// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.16

using System.Collections.Generic;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Indexing;

namespace Xtensive.Storage.Commands
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
    /// <returns>Command execution result.</returns>
    CommandResult Execute(Command command);

    /// <summary>
    /// Executes the specified sequence of commands.
    /// </summary>
    /// <param name="commands">The sequence of commands to execute.</param>
    /// <returns>Command execution results (one per each command).
    /// Value for the specified index exists only if its 
    /// <see cref="CommandResult.IsDefault"/> property returns <see langword="false" />.
    /// </returns>
    Dictionary<int, CommandResult> Execute(List<Command> commands);
  }
}