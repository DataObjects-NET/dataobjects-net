// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.29

using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.Snakes
{
  [Explicit("Requires Sql connection.")]
  public class SqlStorageTest : TestBase
  {
    public override void DomainSetup()
    {
      config = new DomainConfiguration("mssql2005://localhost/Snakes");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.SnakeModel");
      domain = Domain.Build(config);
      domain.Model.Dump();
    }
  }
}