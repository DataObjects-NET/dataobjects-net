using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues_Issue0817_LocalCollectionWithEnum;

namespace Xtensive.Storage.Tests.Issues_Issue0817_LocalCollectionWithEnum
{
  [HierarchyRoot]
  public class Item : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
#pragma warning disable 108,114
    public ItemState State { get; set; }
#pragma warning restore 108,114
  }

  public enum ItemState
  {
    Registering = 17,
    Wrong = 44
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  [TestFixture]
  public class Issue0817_LocalCollectionWithEnum : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Item).Assembly, typeof (Item).Namespace);
      return config;
    }

    [Test]
    public void Query1Test()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var query = from item in Query.All<Item>()
                      where item.State.In(new[] {ItemState.Registering})
                      select item;
          var result = query.ToList();
        }
      }
    }

    [Test]
    public void Query2Test()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var itemStates = Query.Store(new[] {ItemState.Registering});
          var query = from item in Query.All<Item>()
                      where item.State.In(itemStates)
                      select item;
          var result = query.ToList();
        }
      }
    }

    [Test]
    public void Query3Test()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var itemStates = Query.Store(new[] {ItemState.Registering});

          var query = from item in Query.All<Item>()
                      where itemStates.Contains(item.State)
                      select item;
          var result = query.ToList();
        }
      }
    }
  }
}