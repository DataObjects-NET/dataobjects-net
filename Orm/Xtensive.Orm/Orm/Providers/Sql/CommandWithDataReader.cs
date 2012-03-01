// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.01

using System;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Orm.Providers.Sql
{
  public sealed class CommandWithDataReader : IDisposable
  {
    public DbCommand Command { get; private set; }
    public DbDataReader Reader { get; private set; }

    public void Dispose()
    {
      // Dispose the reader first, at least firebird provider requires it
      Reader.Dispose();
      Command.Dispose();
    }

    // Constructors

    internal CommandWithDataReader(DbCommand command, DbDataReader reader)
    {
      ArgumentValidator.EnsureArgumentNotNull(command, "command");
      ArgumentValidator.EnsureArgumentNotNull(reader, "reader");

      Command = command;
      Reader = reader;

    }
  }
}