// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.22

namespace Xtensive.Storage.Providers.Sql
{
  public enum NonTransactionalStage
  {
    None = 0,
    Prolog,
    Epilog
  }
}