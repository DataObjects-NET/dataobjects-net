// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.01.24

using NUnit.Framework;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class EncryptedConnectionStringTest
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfiguration.Load("encrypted");
      Assert.IsNotNullOrEmpty(config.ConnectionInfo.ConnectionString);
      Assert.IsNotNullOrEmpty(config.ConnectionInfo.Provider);
    }
  }
}