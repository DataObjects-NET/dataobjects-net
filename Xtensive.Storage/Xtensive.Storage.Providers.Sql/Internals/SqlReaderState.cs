// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.30

namespace Xtensive.Storage.Providers.Sql
{
  internal enum SqlReaderState
  {
    NotStarted,
    Started,
    Finished,
    Inconsistent
  }
}