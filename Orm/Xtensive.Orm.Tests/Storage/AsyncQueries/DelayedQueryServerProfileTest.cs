// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.09.12

using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.AsyncQueries
{
  public class DelayedQueryServerProfileTest : DelayedQueryTestBase
  {
    protected override SessionConfiguration SessionConfiguration
    {
      get { return new SessionConfiguration(SessionOptions.ServerProfile); }
    }
  }
}
