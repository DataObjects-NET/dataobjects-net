// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.15

namespace Xtensive.Storage.Providers.PgSql
{
  public class SessionHandler : Sql.SessionHandler
  {
    public override void Initialize()
    {
      base.Initialize();
      // TODO: Think what should be done here
    }

    internal protected new DomainHandler DomainHandler
    {
      get { return base.DomainHandler as DomainHandler; }
    }
  }
}