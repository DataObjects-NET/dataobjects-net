using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;

namespace Xtensive.Orm.Tests.Model.InterfaceAssociation
{
  namespace Model1
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
  }

  namespace Model2
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
  }

  namespace Model3
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
  }

  namespace Model4
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
  }

  namespace Model5
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
  }

  namespace Model6
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
  }

  namespace Model7
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
  }

  namespace Model8
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
  }

  namespace Model9
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
  }

  namespace Model10
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
  }

  namespace Model11
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
  }

  namespace Model12
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
  }

  namespace Model13
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
  }

  namespace Model14
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
  }

  namespace Model15
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
  }

  namespace Model16
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
  }

  namespace Model17
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
  }

  namespace Model18
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
  }

  namespace Model19
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
  }

  namespace Model20
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
  }

  namespace Model21
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
  }

  namespace Model22
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
  }

  namespace Model23
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
  }

  namespace Model24
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
  }

  namespace Model25
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
  }

  namespace Model26
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
  }

  namespace Model27
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
  }

  namespace Model28
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
  }

  namespace Model29
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
  }

  namespace Model30
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
  }

  namespace Model31
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
  }

  namespace Model32
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
  }

  namespace Model33
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
  }

  namespace Model34
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
  }

  namespace Model35
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
  }

  namespace Model36
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
  }

  [TestFixture]
  public class InterfaceAssociationTest
  {
    [Test]
    public void CombinedTest01()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model1.Item).Assembly, typeof(Model1.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model1.Document();
        key = document.Key;
        new Model1.Item() { Document = document };
        new Model1.Item() { Document = document };
        new Model1.Item() { Document = document };
        new Model1.Item() { Document = document };
        new Model1.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model1.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model1.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model1.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest02()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model2.Item).Assembly, typeof(Model2.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model2.Document();
        key = document.Key;
        new Model2.Item() { Document = document };
        new Model2.Item() { Document = document };
        new Model2.Item() { Document = document };
        new Model2.Item() { Document = document };
        new Model2.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model2.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model2.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model2.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest03()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model3.Item).Assembly, typeof(Model3.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model3.Document();
        key = document.Key;
        new Model3.Item() { Document = document };
        new Model3.Item() { Document = document };
        new Model3.Item() { Document = document };
        new Model3.Item() { Document = document };
        new Model3.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model3.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model3.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model3.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest04()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model4.Item).Assembly, typeof(Model4.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model4.Document();
        key = document.Key;
        new Model4.Item() { Document = document };
        new Model4.Item() { Document = document };
        new Model4.Item() { Document = document };
        new Model4.Item() { Document = document };
        new Model4.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model4.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model4.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model4.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest05()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model5.Item).Assembly, typeof(Model5.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model5.Document();
        key = document.Key;
        new Model5.Item() { Document = document };
        new Model5.Item() { Document = document };
        new Model5.Item() { Document = document };
        new Model5.Item() { Document = document };
        new Model5.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model5.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model5.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model5.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest06()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model6.Item).Assembly, typeof(Model6.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model6.Document();
        key = document.Key;
        new Model6.Item() { Document = document };
        new Model6.Item() { Document = document };
        new Model6.Item() { Document = document };
        new Model6.Item() { Document = document };
        new Model6.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model6.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model6.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model6.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest07()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model7.Item).Assembly, typeof(Model7.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest08()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model8.Item).Assembly, typeof(Model8.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest09()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model9.Item).Assembly, typeof(Model9.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model9.Document();
        key = document.Key;
        new Model9.Item() { Document = document };
        new Model9.Item() { Document = document };
        new Model9.Item() { Document = document };
        new Model9.Item() { Document = document };
        new Model9.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model9.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model9.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model9.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest10()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model10.Item).Assembly, typeof(Model10.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model10.Document();
        key = document.Key;
        new Model10.Item() { Document = document };
        new Model10.Item() { Document = document };
        new Model10.Item() { Document = document };
        new Model10.Item() { Document = document };
        new Model10.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model10.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model10.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model10.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest11()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model11.Item).Assembly, typeof(Model11.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest12()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model12.Item).Assembly, typeof(Model12.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest13()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model13.Item).Assembly, typeof(Model13.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model13.Document();
        key = document.Key;
        new Model13.Item() { Document = document };
        new Model13.Item() { Document = document };
        new Model13.Item() { Document = document };
        new Model13.Item() { Document = document };
        new Model13.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model13.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model13.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model13.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest14()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model14.Item).Assembly, typeof(Model14.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest15()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model15.Item).Assembly, typeof(Model15.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model15.Document();
        key = document.Key;
        new Model15.Item() { Document = document };
        new Model15.Item() { Document = document };
        new Model15.Item() { Document = document };
        new Model15.Item() { Document = document };
        new Model15.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model15.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model15.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model15.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest16()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model16.Item).Assembly, typeof(Model16.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model16.Document();
        key = document.Key;
        new Model16.Item() { Document = document };
        new Model16.Item() { Document = document };
        new Model16.Item() { Document = document };
        new Model16.Item() { Document = document };
        new Model16.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model16.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model16.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model16.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest17()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model17.Item).Assembly, typeof(Model17.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model17.Document();
        key = document.Key;
        new Model17.Item() { Document = document };
        new Model17.Item() { Document = document };
        new Model17.Item() { Document = document };
        new Model17.Item() { Document = document };
        new Model17.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model17.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model17.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model17.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest18()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model18.Item).Assembly, typeof(Model18.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest19()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model19.Item).Assembly, typeof(Model19.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest20()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model20.Item).Assembly, typeof(Model20.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest21()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model21.Item).Assembly, typeof(Model21.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model21.Document();
        key = document.Key;
        new Model21.Item() { Document = document };
        new Model21.Item() { Document = document };
        new Model21.Item() { Document = document };
        new Model21.Item() { Document = document };
        new Model21.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model21.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model21.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model21.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest22()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model22.Item).Assembly, typeof(Model22.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model22.Document();
        key = document.Key;
        new Model22.Item() { Document = document };
        new Model22.Item() { Document = document };
        new Model22.Item() { Document = document };
        new Model22.Item() { Document = document };
        new Model22.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model22.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model22.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model22.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest23()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model23.Item).Assembly, typeof(Model23.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest24()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model24.Item).Assembly, typeof(Model24.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest25()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model25.Item).Assembly, typeof(Model25.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model25.Document();
        key = document.Key;
        new Model25.Item() { Document = document };
        new Model25.Item() { Document = document };
        new Model25.Item() { Document = document };
        new Model25.Item() { Document = document };
        new Model25.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model25.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model25.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model25.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest26()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model26.Item).Assembly, typeof(Model26.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest27()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model27.Item).Assembly, typeof(Model27.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model27.Document();
        key = document.Key;
        new Model27.Item() { Document = document };
        new Model27.Item() { Document = document };
        new Model27.Item() { Document = document };
        new Model27.Item() { Document = document };
        new Model27.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model27.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model27.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model27.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest28()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model28.Item).Assembly, typeof(Model28.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model28.Document();
        key = document.Key;
        new Model28.Item() { Document = document };
        new Model28.Item() { Document = document };
        new Model28.Item() { Document = document };
        new Model28.Item() { Document = document };
        new Model28.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model28.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model28.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model28.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest29()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model29.Item).Assembly, typeof(Model29.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model29.Document();
        key = document.Key;
        new Model29.Item() { Document = document };
        new Model29.Item() { Document = document };
        new Model29.Item() { Document = document };
        new Model29.Item() { Document = document };
        new Model29.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model29.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model29.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model29.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest30()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model30.Item).Assembly, typeof(Model30.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest31()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model31.Item).Assembly, typeof(Model31.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model31.Document();
        key = document.Key;
        new Model31.Item() { Document = document };
        new Model31.Item() { Document = document };
        new Model31.Item() { Document = document };
        new Model31.Item() { Document = document };
        new Model31.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model31.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model31.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model31.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest32()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model32.Item).Assembly, typeof(Model32.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
    [Test]
    public void CombinedTest33()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model33.Item).Assembly, typeof(Model33.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model33.Document();
        key = document.Key;
        new Model33.Item() { Document = document };
        new Model33.Item() { Document = document };
        new Model33.Item() { Document = document };
        new Model33.Item() { Document = document };
        new Model33.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model33.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model33.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model33.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest34()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model34.Item).Assembly, typeof(Model34.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model34.Document();
        key = document.Key;
        new Model34.Item() { Document = document };
        new Model34.Item() { Document = document };
        new Model34.Item() { Document = document };
        new Model34.Item() { Document = document };
        new Model34.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model34.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model34.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model34.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest35()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model35.Item).Assembly, typeof(Model35.Item).Namespace);
      var domain = Domain.Build(config);
      var key = (Key)null;
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = new Model35.Document();
        key = document.Key;
        new Model35.Item() { Document = document };
        new Model35.Item() { Document = document };
        new Model35.Item() { Document = document };
        new Model35.Item() { Document = document };
        new Model35.Item() { Document = document };
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model35.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(5, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(5, itemCount);

        var some = document.Items.First();
        some.Document = null;
        itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);
        t.Complete();
      }

      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var document = session.Query.Single<Model35.Document>(key);
        var itemCount = 0;
        Assert.AreEqual(4, document.Items.Count);
        foreach (var item in document.Items) {
          Assert.IsNotNull(item);
          itemCount++;
        }
        Assert.AreEqual(4, itemCount);

        document.Remove();
        var items = session.Query.All<Model35.Item>().ToList();
        Assert.AreEqual(1, items.Count);
        
        t.Complete();
      }
    }
    [Test]
    public void CombinedTest36()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Model36.Item).Assembly, typeof(Model36.Item).Namespace);
      Assert.Throws<DomainBuilderException>(()=>Domain.Build(config));
    }
  }
}