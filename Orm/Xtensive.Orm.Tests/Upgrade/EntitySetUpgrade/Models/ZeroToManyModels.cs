// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade.Models.ZeroToMany
{
  namespace RenameEntitySetItemType
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff1 : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff1> Guys { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(
            new RenameTypeHint(typeof(Staff1).Namespace.Replace("After", "Before") + ".Staff", typeof(Staff1)));
        }
      }
    }
  }

  namespace RenameEntitySetField
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys1 { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameFieldHint(typeof(Brigade), "Guys", "Guys1"));
          //_ = hints.Add(
          //  new RenameTypeHint(typeof(Staff1).FullName.Replace("After", "Before").Replace("Staff1", "Staff"), typeof(Staff1)));

        }
      }
    }
  }

  namespace RenameEntitySetFieldAndType
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff1 : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff1> Guys1 { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RenameFieldHint(typeof(Brigade), "Guys", "Guys1"));
          _ = hints.Add(
            new RenameTypeHint(typeof(Staff1).FullName.Replace("After", "Before").Replace("Staff1", "Staff"), typeof(Staff1)));

        }
      }
    }
  }

  namespace RenameKeyField
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long IdId { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(
            new RenameFieldHint(typeof(Staff), "Id", "IdId"));
        }
      }
    }
  }

  namespace ChangeEntitySetItemType
  {
    namespace Before
    {
      [HierarchyRoot]
      public class User : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After1
    {
      [HierarchyRoot]
      public class User : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<User> Guys { get; private set; }
      }
    }

    namespace After2
    {
      [HierarchyRoot]
      public class User : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<User> Guys { get; private set; }
      }
    }
  }

  namespace ChangeTypeOfKeyFieldConvertible
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public int Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(
            new ChangeFieldTypeHint(typeof(Brigade), "Id"));
        }
      }
    }
  }

  namespace ChangeTypeOfKeyFieldUnconvertible
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        public Staff(string key1)
          : base(key1)
        {

        }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(
            new ChangeFieldTypeHint(typeof(Brigade), "Id"));
        }
      }
    }
  }

  namespace ChangeTypeOfKeyFieldsUnconvertible
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key(0)]
        public string Id1 { get; private set; }

        [Field, Key(1)]
        public string Id2 { get; private set; }

        [Field]
        public int Test { get; set; }

        public Staff(string key1, string key2)
          : base(key1, key2)
        {
        }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key(0)]
        public long Id1 { get; private set; }

        [Field, Key(1)]
        public long Id2 { get; private set; }

        [Field]
        public int Test { get; set; }

        public Staff(string key1, string key2)
          : base(key1, key2)
        {
        }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new ChangeFieldTypeHint(typeof(Staff), "Id1"));
          _ = hints.Add(new ChangeFieldTypeHint(typeof(Staff), "Id2"));
        }
      }
    }
  }

  namespace RemoveEntitySetField
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public string Id { get; private set; }


        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public string Id { get; private set; }


        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          AddRemoveEntitySetHints(
            typeof(Staff).Namespace.Replace("After", "Before") + ".Brigade",
            "Guys",
            hints,
            UpgradeContext);
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
          if (association.Multiplicity == Multiplicity.ZeroToMany)
            _ = hints.Add(new RemoveTypeHint(association.ConnectorType.UnderlyingType));
        }
      }
    }
  }

  namespace RemoveEntitySetFieldAndItemType
  {
    namespace Before
    {
      [HierarchyRoot]
      public class User : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class User : Entity
      {
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(new RemoveTypeHint(typeof(User).Namespace.Replace("After", "Before") + ".Staff"));
          AddRemoveEntitySetHints(
            typeof(User).Namespace.Replace("After", "Before") + ".Brigade",
            "Guys",
            hints,
            UpgradeContext);
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
          if (association.Multiplicity == Multiplicity.ZeroToMany)
            _ = hints.Add(new RemoveTypeHint(association.ConnectorType.UnderlyingType));
        }
      }
    }
  }

  namespace RemoveSlaveKeyField
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key(0)]
        public string Id1 { get; private set; }

        [Field, Key(1)]
        public string Id2 { get; private set; }

        [Field]
        public int Test { get; set; }

        public Staff(string key1, string key2)
          : base(key1, key2)
        {
        }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }

        public Brigade(string key1)
          : base(key1)
        {
        }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        public Staff(string key1)
          : base(key1)
        {
        }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }

        public Brigade(string key1)
          : base(key1)
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
            typeof(Staff).Namespace.Replace("After", "Before") + ".Staff",
            "Id2",
            hints,
            UpgradeContext);

          _ = hints.Add(new RenameFieldHint(typeof(Staff), "Id1", "Id"));
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

  namespace RemoveMasterKeyField
  {
    namespace Before
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        public Staff(string key1)
          : base(key1)
        {
        }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key(0)]
        public string Id1 { get; private set; }

        [Field, Key(1)]
        public string Id2 { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }

        public Brigade(string key1, string key2)
          : base(key1, key2)
        {
        }
      }
    }

    namespace After
    {
      [HierarchyRoot]
      public class Staff : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        public Staff(string key1)
          : base(key1)
        {
        }
      }

      [HierarchyRoot]
      public class Brigade : Entity
      {
        [Field, Key]
        public string Id { get; private set; }

        [Field]
        public int Test { get; set; }

        [Field]
        public EntitySet<Staff> Guys { get; private set; }

        public Brigade(string key1)
          : base(key1)
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
            typeof(Brigade).Namespace.Replace("After", "Before") + ".Brigade",
            "Id2",
            hints,
            UpgradeContext);

          _ = hints.Add(new RenameFieldHint(typeof(Brigade), "Id1", "Id"));
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
