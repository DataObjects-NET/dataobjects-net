using NUnit.Framework;
using Xtensive.Sql.Dom.Database;

namespace Xtensive.Sql.Dom.Tests
{
  [TestFixture]
  public class CollectionTest
  {
    [Test]
    public void Test2()
    {
      Model m1 = new Model("model1");
      Model m2 = new Model("model1");

      Server s1 = m1.CreateServer("server1");
      Server s2 = m2.CreateServer("server2");

      s2.Model = m1;
      Assert.AreEqual(m1.Servers.Count, 2);
      Assert.AreEqual(s1.Model, s2.Model);

      s2.Model = m2;
      Assert.AreNotEqual(s1.Model, s2.Model);
      Assert.AreEqual(m1.Servers.Count, 1);
      Assert.AreEqual(m2.Servers.Count, 1);

      s2.Model = null;
      Assert.IsNull(s2.Model);
      Assert.AreEqual(m2.Servers.Count, 0);
    }
  }
}
