// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using NUnit.Framework;
using Xtensive.Core;

namespace Xtensive.Sql.Tests
{
    [TestFixture]
    public class DriverFactoryTest
    {
        [Test]
        public void ConnectionUrlTest()
        {
            var url = UrlInfo.Parse(TestUrl.SqlServer2005Aw);
            Assert.AreEqual(url.Protocol, "sqlserver");
            Assert.AreEqual(url.Host, "localhost");
            Assert.AreEqual(url.Resource, "AdventureWorks");

            url = UrlInfo.Parse("sqlserver://localhost/database");
            Assert.AreEqual("database", url.GetDatabase());
            Assert.AreEqual("default schema", url.GetSchema("default schema"));

            url = UrlInfo.Parse("sqlserver://localhost/database/");
            Assert.AreEqual("database", url.GetDatabase());
            Assert.AreEqual("default schema", url.GetSchema("default schema"));

            url = UrlInfo.Parse("sqlserver://localhost/database/schema");
            Assert.AreEqual("database", url.GetDatabase());
            Assert.AreEqual("schema", url.GetSchema(string.Empty));

            url = UrlInfo.Parse("sqlserver://localhost/database/schema/");
            Assert.AreEqual("database", url.GetDatabase());
            Assert.AreEqual("schema", url.GetSchema(string.Empty));
        }

        [Test]
        public void ServerInfoTest()
        {
            var driver = TestSqlDriver.Create(TestUrl.SqlServer2005);
            Assert.Greater(driver.CoreServerInfo.ServerVersion.Major, 8);
        }

        [Test]
        public void ProviderTest()
        {
            TestProvider("sqlserver", TestConnectionString.SqlServer2005, TestUrl.SqlServer2005);
            TestProvider("postgresql", TestConnectionString.PostgreSql84, TestUrl.PostgreSql84);
            TestProvider("mysql", TestConnectionString.MySQL50, TestUrl.MySQL50);
            // TestProvider("oracle", TestConnectionString.Oracle11, TestUrl.Oracle11);
        }

        private static void TestProvider(string providerName, string connectionString, string connectionUrl)
        {
            Assert.IsNotNull(TestSqlDriver.Create(connectionUrl));
            Assert.IsNotNull(TestSqlDriver.Create(providerName, connectionString));
        }
    }
}