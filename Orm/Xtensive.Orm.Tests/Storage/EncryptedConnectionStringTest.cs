// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.24

using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class EncryptedConnectionStringTest
  {
    [Test, Explicit]
    public void MainTest()
    {
      var config = DomainConfiguration.Load("encrypted");
      Assert.IsNotNullOrEmpty(config.ConnectionInfo.ConnectionString);
      Assert.IsNotNullOrEmpty(config.ConnectionInfo.Provider);
    }
  }
}