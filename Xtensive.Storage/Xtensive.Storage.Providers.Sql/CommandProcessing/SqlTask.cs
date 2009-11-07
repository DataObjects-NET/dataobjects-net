// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.30

namespace Xtensive.Storage.Providers.Sql
{
  internal abstract class SqlTask
  {
    public abstract void Process(CommandProcessor processor);
  }
}