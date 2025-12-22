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

      Assert.That(c1.DefaultSchema, Is.EqualTo(s1));
      Assert.That(c2.DefaultSchema, Is.EqualTo(s2));

      s2.Catalog = c1;
      Assert.That(c1.Schemas.Count, Is.EqualTo(2));
      Assert.That(s2.Catalog, Is.EqualTo(s1.Catalog));

      s2.Catalog = c2;
      Assert.That(s2.Catalog, Is.Not.EqualTo(s1.Catalog));
      Assert.That(c1.Schemas.Count, Is.EqualTo(1));
      Assert.That(c2.Schemas.Count, Is.EqualTo(1));

      s2.Catalog = null;
      Assert.That(s2.Catalog, Is.Null);
      Assert.That(c2.Schemas.Count, Is.EqualTo(0));
    }
  }
}
