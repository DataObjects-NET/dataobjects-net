// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using NUnit.Framework;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Sql
{
  [TestFixture]
  public class CollectionTest
  {
    [Test]
    public void CatalogSchemaTest()
    {
      var c1 = new Catalog("catalog1");
      var c2 = new Catalog("catalog2");

      var s1 = c1.CreateSchema("schema1");
      var s2 = c2.CreateSchema("schema2");

      Assert.AreEqual(s1, c1.DefaultSchema);
      Assert.AreEqual(s2, c2.DefaultSchema);

      s2.Catalog = c1;
      Assert.AreEqual(c1.Schemas.Count, 2);
      Assert.AreEqual(s1.Catalog, s2.Catalog);

      s2.Catalog = c2;
      Assert.AreNotEqual(s1.Catalog, s2.Catalog);
      Assert.AreEqual(c1.Schemas.Count, 1);
      Assert.AreEqual(c2.Schemas.Count, 1);

      s2.Catalog = null;
      Assert.IsNull(s2.Catalog);
      Assert.AreEqual(c2.Schemas.Count, 0);
    }
  }
}
