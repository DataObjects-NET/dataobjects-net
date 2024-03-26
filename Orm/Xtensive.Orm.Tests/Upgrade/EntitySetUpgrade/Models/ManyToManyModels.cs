// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade.Models.ManyToMany
{
  namespace SharedBefore
  {
    [HierarchyRoot]
    public class Author : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field, Association(PairTo = nameof(Book.Authors))]
      public EntitySet<Book> Books { get; private set; }
    }

    [HierarchyRoot]
    public class Book : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public EntitySet<Author> Authors { get; private set; }
    }
  }

  namespace RenameEntitySetItemTypeOnMaster
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author1 : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author1> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameTypeHint(typeof(SharedBefore.Author).FullName, typeof(Author1)));
        }
      }
    }
  }

  namespace RenameEntitySetItemTypeOnSlave
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book1.Authors))]
        public EntitySet<Book1> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book1 : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameTypeHint(typeof(SharedBefore.Book).FullName, typeof(Book1)));
        }
      }
    }
  }

  namespace RenameEntitySetOnMaster
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors1))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors1 { get; private set; }
      }
    }
  }

  namespace RenameEntitySetOnSlave
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books1 { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }
    }
  }

  namespace RenameEntitySetFieldAndTypeOnMaster
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book1.Authors1))]
        public EntitySet<Book1> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book1 : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors1 { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameTypeHint(typeof(SharedBefore.Book).FullName, typeof(Book1)));
        }
      }
    }
  }

  namespace RenameEntitySetFieldAndTypeOnSlave
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author1 : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books1 { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author1> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameTypeHint(typeof(SharedBefore.Author).FullName, typeof(Author1)));
        }
      }
    }
  }

  namespace RenameKeyFieldOnMaster
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id1 { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameFieldHint(typeof(Book), "Id", "Id1"));
        }
      }
    }
  }

  namespace RenameKeyFieldOnSlave
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id1 { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameFieldHint(typeof(Author), "Id", "Id1"));
        }
      }
    }
  }

  namespace ChangeTypeOfKeyFieldOnMasterConvertible
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }
    }
  }

  namespace ChangeTypeOfKeyFieldOnSlaveConvertible
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }
    }
  }

  namespace ChangeTypeOfKeyFieldOnMasterUnconvertible
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }

        public Book(string id)
          : base(id)
        {
        }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new ChangeFieldTypeHint(typeof(Book), "Id"));
        }
      }
    }
  }

  namespace ChangeTypeOfKeyFieldOnSlaveUnconvertible
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }

        public Author(string id)
          : base(id)
        {
        }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new ChangeFieldTypeHint(typeof(Author), "Id"));
        }
      }
    }
  }

  namespace ChangeEntitySetItemTypeOnMaster
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Creator : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Creator : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Creators))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Creator> Creators { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameFieldHint(typeof(Book), "Authors", "Creators"));

          ChangeEntitySetType(typeof(Book), "Authors", hints, UpgradeContext);
        }

        private static void ChangeEntitySetType(Type type, string entitySetFieldName, ISet<UpgradeHint> hints, UpgradeContext context)
        {
          var containingTypeFullName = type.FullName;
          var containingTypeName = type.Name;
          var oldContainingType =
            context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType == containingTypeFullName)
            ?? context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType.EndsWith(containingTypeName, StringComparison.Ordinal));
          if (oldContainingType is null)
            throw new InvalidOperationException();

          var esField = oldContainingType.Fields.Where(f => f.Name == entitySetFieldName).FirstOrDefault();

          var affectedAssociations = context.ExtractedDomainModel.Associations.Where(a => a.ConnectorType != null
            && (a.ReferencingField == esField))
            .ToList();

          var connectorType = (affectedAssociations.FirstOrDefault(a => a.IsMaster)
            ?? affectedAssociations.Select(a => a.Reversed).FirstOrDefault(a => a.IsMaster)).ConnectorType.UnderlyingType;
          _ = hints.Add(new RemoveTypeHint(connectorType));
        }

        private static void CreateRemovePKFieldHints(string typeName, string fieldName, ISet<UpgradeHint> hints, UpgradeContext context)
        {
          var removingType = context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType == typeName);
          if (removingType is null)
            throw new InvalidOperationException();
          var removingField = removingType.Fields.FirstOrDefault(f => f.Name == fieldName);
          if (removingField is null || !removingField.IsPrimaryKey)
            throw new InvalidOperationException();

          var removeFieldHint = new RemoveFieldHint(typeName, fieldName);
          _ = hints.Add(removeFieldHint);

          var affectedAssociations = context.ExtractedDomainModel.Associations
            .Where(a => a.ConnectorType != null
              && (a.ReferencedType.UnderlyingType == removeFieldHint.Type || a.ReferencingField.DeclaringType.UnderlyingType == removeFieldHint.Type))
            .ToList();
          foreach (var affectedAssociation in affectedAssociations) {
            var connectorType = affectedAssociation.ConnectorType;

            var auxSourceTypeField = (affectedAssociation.ReferencedType.UnderlyingType == removeFieldHint.Type)
              ? connectorType.Fields.FirstOrDefault(a => a.Name == WellKnown.SlaveFieldName)
              : (affectedAssociation.ReferencingField.DeclaringType.UnderlyingType == removeFieldHint.Type)
                 ? connectorType.Fields.FirstOrDefault(a => a.Name == WellKnown.MasterFieldName)
                 : throw new InvalidOperationException();

            var removingAuxField = auxSourceTypeField.Fields.First(a => a.Name.Contains(removeFieldHint.Field, StringComparison.Ordinal));
            _ = hints.Add(new RemoveFieldHint(connectorType.UnderlyingType, removingAuxField.Name));
          }
        }
      }
    }
  }

  namespace ChangeEntitySetItemTypeOnSlave
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }

      [HierarchyRoot]
      public class Brochure : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Brochure.Authors))]
        public EntitySet<Brochure> Brochures { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Brochure : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameFieldHint(typeof(Author), "Books", "Brochures"));

          ChangeEntitySetType(typeof(Author), "Books", hints, UpgradeContext);
        }

        private static void ChangeEntitySetType(Type type, string entitySetFieldName, ISet<UpgradeHint> hints, UpgradeContext context)
        {
          var containingTypeFullName = type.FullName;
          var containingTypeName = type.Name;
          var oldContainingType =
            context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType == containingTypeFullName)
            ?? context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType.EndsWith(containingTypeName, StringComparison.Ordinal));
          if (oldContainingType is null)
            throw new InvalidOperationException();

          var esField = oldContainingType.Fields.Where(f => f.Name == entitySetFieldName).FirstOrDefault();

          var affectedAssociations = context.ExtractedDomainModel.Associations.Where(a => a.ConnectorType != null
            && (a.ReferencingField == esField))
            .ToList();

          var connectorType = (affectedAssociations.FirstOrDefault(a => a.IsMaster)
            ?? affectedAssociations.Select(a => a.Reversed).FirstOrDefault(a => a.IsMaster)).ConnectorType.UnderlyingType;
          _ = hints.Add(new RemoveTypeHint(connectorType));
        }
      }
    }
  }

  namespace ChangeESTypeFromRemovedTypeOnMaster
  {
    namespace After
    {
      [HierarchyRoot]
      public class Creator : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Creators))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Creator> Creators { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);

          _ = hints.Add(new RenameFieldHint(typeof(Book), "Authors", "Creators"));
          _ = hints.Add(new RemoveTypeHint(typeof(SharedBefore.Author).FullName));
          ChangeEntitySetType(typeof(Book), "Authors", hints, UpgradeContext);
        }
        private static void ChangeEntitySetType(Type type, string entitySetFieldName, ISet<UpgradeHint> hints, UpgradeContext context)
        {
          var containingTypeFullName = type.FullName;
          var containingTypeName = type.Name;
          var oldContainingType =
            context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType == containingTypeFullName)
            ?? context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType.EndsWith(containingTypeName, StringComparison.Ordinal));
          if (oldContainingType is null)
            throw new InvalidOperationException();

          var esField = oldContainingType.Fields.Where(f => f.Name == entitySetFieldName).FirstOrDefault();

          var affectedAssociations = context.ExtractedDomainModel.Associations.Where(a => a.ConnectorType != null
            && (a.ReferencingField == esField))
            .ToList();

          var connectorType = (affectedAssociations.FirstOrDefault(a => a.IsMaster)
            ?? affectedAssociations.Select(a => a.Reversed).FirstOrDefault(a => a.IsMaster)).ConnectorType.UnderlyingType;
          _ = hints.Add(new RemoveTypeHint(connectorType));
        }
      }
    }
  }

  namespace ChangeESTypeFromRemovedTypeOnSlave
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Brochure.Authors))]
        public EntitySet<Brochure> Brochures { get; private set; }
      }

      [HierarchyRoot]
      public class Brochure : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);

          _ = hints.Add(new RenameFieldHint(typeof(Author), "Books", "Brochures"));
          _ = hints.Add(new RemoveTypeHint(typeof(SharedBefore.Book).FullName));
          ChangeEntitySetType(typeof(Author), "Books", hints, UpgradeContext);
        }

        private static void ChangeEntitySetType(Type type, string entitySetFieldName, ISet<UpgradeHint> hints, UpgradeContext context)
        {
          var containingTypeFullName = type.FullName;
          var containingTypeName = type.Name;
          var oldContainingType =
            context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType == containingTypeFullName)
            ?? context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType.EndsWith(containingTypeName, StringComparison.Ordinal));
          if (oldContainingType is null)
            throw new InvalidOperationException();

          var esField = oldContainingType.Fields.Where(f => f.Name == entitySetFieldName).FirstOrDefault();

          var affectedAssociations = context.ExtractedDomainModel.Associations.Where(a => a.ConnectorType != null
            && (a.ReferencingField == esField))
            .ToList();

          var connectorType = (affectedAssociations.FirstOrDefault(a => a.IsMaster)
            ?? affectedAssociations.Select(a => a.Reversed).FirstOrDefault(a => a.IsMaster)).ConnectorType.UnderlyingType;
          _ = hints.Add(new RemoveTypeHint(connectorType));
        }
      }
    }
  }

  namespace RemoveEntitySetFieldOnMaster
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }
    }
  }

  namespace RemoveEntitySetFieldOnSlave
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }
    }
  }

  namespace RemoveEntitySetFieldAndItemTypeOnMaster
  {
    namespace After
    {
      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);

          _ = hints.Add(new RemoveTypeHint(typeof(SharedBefore.Author).FullName));
          AddRemoveEntitySetHints(typeof(SharedBefore.Book).FullName, "Authors", hints, UpgradeContext);
        }

        private static void AddRemoveEntitySetHints(string typeName, string entitySetFieldName, ISet<UpgradeHint> hints, UpgradeContext context)
        {
          var removeFieldHint = new RemoveFieldHint(typeName, entitySetFieldName);
          _ = hints.Add(removeFieldHint);

          var type = context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType.Equals(removeFieldHint.Type));
          if (type == null)
            throw new Exception();
          var field = type.Fields.FirstOrDefault(f => f.Name.Equals(removeFieldHint.Field, StringComparison.Ordinal));
          if (field == null || !field.IsEntitySet)
            throw new Exception();

          var association = type.Associations
            .FirstOrDefault(a => a.ReferencingField == field);
          if (association is not null)
            _ = hints.Add(new RemoveTypeHint(association.ConnectorType.UnderlyingType));
        }
      }
    }
  }

  namespace RemoveEntitySetFieldAndItemTypeOnSlave
  {
    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);

          _ = hints.Add(new RemoveTypeHint(typeof(SharedBefore.Book).FullName));
          AddRemoveEntitySetHints(typeof(SharedBefore.Author).FullName, "Books", hints, UpgradeContext);
        }

        private static void AddRemoveEntitySetHints(string typeName, string entitySetFieldName, ISet<UpgradeHint> hints, UpgradeContext context)
        {
          var removeFieldHint = new RemoveFieldHint(typeName, entitySetFieldName);
          _ = hints.Add(removeFieldHint);

          var type = context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType.Equals(removeFieldHint.Type));
          if (type == null)
            throw new Exception();
          var field = type.Fields.FirstOrDefault(f => f.Name.Equals(removeFieldHint.Field, StringComparison.Ordinal));
          if (field == null || !field.IsEntitySet)
            throw new Exception();

          var association = type.Associations
            .FirstOrDefault(a => a.ReferencingField == field);
          if (association is not null)
            _ = hints.Add(new RemoveTypeHint(association.ConnectorType.UnderlyingType));
        }
      }
    }
  }

  namespace RemoveMasterKeyFieldType
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key(0)]
        public int Id1 { get; private set; }

        [Field, Key(1)]
        public int Id2 { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }

        public Author(int id1, int id2)
          : base(id1, id2)
        {
        }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key(0)]
        public int Id1 { get; private set; }

        [Field, Key(1)]
        public int Id2 { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field/*, Association(PairTo = nameof(Author.Books))*/]
        public EntitySet<Author> Authors { get; private set; }

        public Book(int id1, int id2)
          : base(id1, id2)
        {
        }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key(0)]
        public int Id1 { get; private set; }

        [Field, Key(1)]
        public int Id2 { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }

        public Author(int id1, int id2)
          : base(id1, id2)
        {
        }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          // basically we need to check whether removing field is PK and find any
          // associations it involved and add additional remove field hints

          base.AddUpgradeHints(hints);
          CreateRemovePKFieldHints(
            typeof(Book).FullName.Replace("After", "Before"),
            "Id2",
            hints,
            UpgradeContext);

          _ = hints.Add(new RenameFieldHint(typeof(Book), "Id1", "Id"));
        }

        private static void CreateRemovePKFieldHints(string typeName, string fieldName, ISet<UpgradeHint> hints, UpgradeContext context)
        {
          var removingType = context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType == typeName);
          if (removingType is null)
            throw new InvalidOperationException();
          var removingField = removingType.Fields.FirstOrDefault(f => f.Name == fieldName);
          if (removingField is null || !removingField.IsPrimaryKey)
            throw new InvalidOperationException();

          var removeFieldHint = new RemoveFieldHint(typeName, fieldName);
          _ = hints.Add(removeFieldHint);

          var affectedAssociations = context.ExtractedDomainModel.Associations
            .Where(a => a.ConnectorType != null
              && (a.ReferencedType.UnderlyingType == removeFieldHint.Type || a.ReferencingField.DeclaringType.UnderlyingType == removeFieldHint.Type))
            .Where(a => a.IsMaster)
            .ToList();
          foreach (var affectedAssociation in affectedAssociations) {
            var connectorType = affectedAssociation.ConnectorType;

            var auxSourceTypeField = (affectedAssociation.ReferencedType.UnderlyingType == removeFieldHint.Type)
              ? connectorType.Fields.FirstOrDefault(a => a.Name == WellKnown.SlaveFieldName)
              : (affectedAssociation.ReferencingField.DeclaringType.UnderlyingType == removeFieldHint.Type)
                 ? connectorType.Fields.FirstOrDefault(a => a.Name == WellKnown.MasterFieldName)
                 : throw new InvalidOperationException();

            var removingAuxField = auxSourceTypeField.Fields.First(a => a.Name.Contains(removeFieldHint.Field, StringComparison.Ordinal));
            _ = hints.Add(new RemoveFieldHint(connectorType.UnderlyingType, removingAuxField.Name));
          }
        }
      }
    }
  }

  namespace RemoveSlaveKeyFieldType
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key(0)]
        public int Id1 { get; private set; }

        [Field, Key(1)]
        public int Id2 { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }

        public Author(int id1, int id2)
          : base(id1, id2)
        {
        }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key(0)]
        public int Id1 { get; private set; }

        [Field, Key(1)]
        public int Id2 { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }

        public Book(int id1, int id2)
          : base(id1, id2)
        {
        }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Author : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field, Association(PairTo = nameof(Book.Authors))]
        public EntitySet<Book> Books { get; private set; }
      }

      [HierarchyRoot]
      public class Book : Entity
      {
        [Field, Key(0)]
        public int Id1 { get; private set; }

        [Field, Key(1)]
        public int Id2 { get; private set; }

        [Field]
        public string Name { get; set; }

        [Field]
        public EntitySet<Author> Authors { get; private set; }

        public Book(int id1, int id2)
          : base(id1, id2)
        {
        }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          // basically we need to check whether removing field is PK and find any
          // associations it involved and add additional remove field hints

          base.AddUpgradeHints(hints);
          CreateRemovePKFieldHints(
            typeof(Author).FullName.Replace("After", "Before"),
            "Id2",
            hints,
            UpgradeContext);

          _ = hints.Add(new RenameFieldHint(typeof(Author), "Id1", "Id"));
        }

        private static void CreateRemovePKFieldHints(string typeName, string fieldName, ISet<UpgradeHint> hints, UpgradeContext context)
        {
          var removingType = context.ExtractedDomainModel.Types.FirstOrDefault(t => t.UnderlyingType == typeName);
          if (removingType is null)
            throw new InvalidOperationException();
          var removingField = removingType.Fields.FirstOrDefault(f => f.Name == fieldName);
          if (removingField is null || !removingField.IsPrimaryKey)
            throw new InvalidOperationException();

          var removeFieldHint = new RemoveFieldHint(typeName, fieldName);
          _ = hints.Add(removeFieldHint);

          var affectedAssociations = context.ExtractedDomainModel.Associations
            .Where(a => a.ConnectorType != null
              && (a.ReferencedType.UnderlyingType == removeFieldHint.Type || a.ReferencingField.DeclaringType.UnderlyingType == removeFieldHint.Type))
            .ToList();
          foreach (var affectedAssociation in affectedAssociations) {
            var connectorType = affectedAssociation.ConnectorType;

            var auxSourceTypeField = (affectedAssociation.ReferencedType.UnderlyingType == removeFieldHint.Type)
              ? connectorType.Fields.FirstOrDefault(a => a.Name == WellKnown.SlaveFieldName)
              : (affectedAssociation.ReferencingField.DeclaringType.UnderlyingType == removeFieldHint.Type)
                 ? connectorType.Fields.FirstOrDefault(a => a.Name == WellKnown.MasterFieldName)
                 : throw new InvalidOperationException();

            var removingAuxField = auxSourceTypeField.Fields.First(a => a.Name.Contains(removeFieldHint.Field, StringComparison.Ordinal));
            _ = hints.Add(new RemoveFieldHint(connectorType.UnderlyingType, removingAuxField.Name));
          }
        }
      }
    }
  }
}