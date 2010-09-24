using System;
using NUnit.Framework;

namespace Xtensive.Storage.Tests.Model.InterfaceAssociation
{
  namespace Model0_0_0_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_0_0_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_0_1_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_0_1_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_0_2_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_0_2_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_1_0_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_1_0_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_1_1_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_1_1_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_1_2_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model0_1_2_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<IItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<IItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_0_0_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_0_0_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_0_1_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_0_1_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_0_2_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_0_2_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      IDocument Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public IDocument Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_1_0_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_1_0_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_1_1_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_1_1_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_1_2_0
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model1_1_2_1
  {
    public interface IDocument : IEntity
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<Item> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      Document Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public Document Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_0_0_0
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_0_0_1
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_0_1_0
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_0_1_1
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_0_2_0
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_0_2_1
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_1_0_0
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_1_0_1
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_1_1_0
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_1_1_1
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_1_2_0
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
  namespace Model2_1_2_1
  {
    public interface IDocument<TItem> : IEntity
      where TItem : IItem
    {
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      EntitySet<TItem> Items { get; }
    }

    public interface IItem : IEntity
    {
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      IDocument<Item> Document { get; set; }
    }

    [HierarchyRoot]
    public class Document : Entity, IDocument<Item>
    {
      [Field,Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Document", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Item> Items { get; private set; }
    }

    [HierarchyRoot]
    public class Item : Entity, IItem
    {
      [Field, Key]
      public int Id { get; private set; }
      [Field]
      [Association(PairTo = "Items", OnOwnerRemove = OnRemoveAction.Clear, OnTargetRemove = OnRemoveAction.Cascade)]
      public IDocument<Item> Document { get; set; }
    }

    [TestFixture]
    public class InterfaceAssociationTest
    {
      [Test]
      public void CombinedTest()
      {
        var config = DomainConfigurationFactory.Create();
        config.Types.Register(typeof(Item).Assembly, typeof(Item).Namespace);
        var domain = Domain.Build(config);

        using (var session = Session.Open(domain))
        using (var t = Transaction.Open())
        {
          var document = new Document();
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };
          new Item() { Document = document };

          var itemCount = 0;
          Assert.AreEqual(5, document.Items.Count);
          foreach (var item in document.Items)
          {
            Assert.IsNotNull(item);
            itemCount++;
          }
          Assert.AreEqual(5, itemCount);
        }
      }
    }
  }
}