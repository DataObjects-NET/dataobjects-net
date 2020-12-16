// Copyright (C) 2011-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2011.01.24

using NUnit.Framework;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  [Ignore("Seems to be no support for encrypted connection strings")]
  public class EncryptedConnectionStringTest : HasConfigurationAccessTest
  {
    [Test]
    public void MainTest()
    {
      var config = LoadDomainConfiguration("encrypted");
      Assert.That(config.ConnectionInfo.ConnectionString, Is.Not.Null.And.Not.Empty);
      Assert.That(config.ConnectionInfo.Provider, Is.Not.Null.And.Not.Empty);
    }
  }
}