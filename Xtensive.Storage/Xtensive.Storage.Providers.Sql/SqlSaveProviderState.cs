// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.03

namespace Xtensive.Storage.Providers.Sql
{
  public sealed class SqlSaveProviderState
  {
    public bool BeforeEnumerationExecuted { get; set; }

    public bool AfterEnumerationExecuted { get; set; }
  }
}