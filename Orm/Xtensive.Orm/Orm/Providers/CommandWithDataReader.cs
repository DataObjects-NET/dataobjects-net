// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.01

using System;
using System.Data.Common;
using System.Threading.Tasks;
using Xtensive.Core;

namespace Xtensive.Orm.Providers
{
  public sealed class CommandWithDataReader : IDisposable, IAsyncDisposable
  {
    public DbCommand Command { get; private set; }
    public DbDataReader Reader { get; private set; }

    public void Dispose()
    {
      // Dispose the reader first, at least firebird provider requires it
      Reader.Dispose();
      Command.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
      // Dispose the reader first, at least firebird provider requires it
      await Reader.DisposeAsync();
      await Command.DisposeAsync();
    }

    // Constructors

    internal CommandWithDataReader(DbCommand command, DbDataReader reader)
    {
      ArgumentValidator.EnsureArgumentNotNull(command, nameof(command));
      ArgumentValidator.EnsureArgumentNotNull(reader, nameof(reader));

      Command = command;
      Reader = reader;

    }
  }
}