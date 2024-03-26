// Copyright (C) 2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Collections.Generic;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade.Models.OneToMany
{
  namespace RenameField
  {
    namespace Before
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        [Association(PairTo = "Person")]
        public EntitySet<Address> Addresses { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }
    }

    namespace After
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        [Association(PairTo = "Person")]
        public EntitySet<Address> AllAddresses { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }
    }
  }

  namespace RemoveField
  {
    namespace Before
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        [Association(PairTo = "Person")]
        public EntitySet<Address> Addresses { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }
    }

    namespace After
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }
    }
  }

  namespace RemovePairedField
  {
    namespace Before
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }
    }

    namespace After
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public EntitySet<Address> Addresses { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(
            new RemoveFieldHint(
              "Xtensive.Orm.Tests.Upgrade.EntitySetUpgrade.Models.OneToMany.RemovePairedField.Before.Address",
              "Person")
          );
        }
      }
    }
  }

  namespace ChangeEntitySetType
  {
    namespace Before
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        [Association(PairTo = "Person")]
        public EntitySet<Address> Addresses { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Location : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }
    }

    namespace After
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        [Association(PairTo = "Person")]
        public EntitySet<Location> Locations { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Location : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }
    }
  }

  namespace RenameKeyField
  {
    namespace Before
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        [Association(PairTo = "Person")]
        public EntitySet<Address> Addresses { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }
    }

    namespace After
    {
      [Serializable]
      [HierarchyRoot]
      public class Person : Entity
      {
        [Key, Field]
        public long IdId { get; private set; }

        [Field]
        [Association(PairTo = "Person")]
        public EntitySet<Address> Addresses { get; private set; }
      }

      [Serializable]
      [HierarchyRoot]
      public class Address : Entity
      {
        [Key, Field]
        public long Id { get; private set; }

        [Field]
        public Person Person { get; set; }
      }

      public class Upgrader : UpgradeHandler
      {
        public override bool CanUpgradeFrom(string oldVersion) => true;

        protected override void AddUpgradeHints(ISet<UpgradeHint> hints)
        {
          base.AddUpgradeHints(hints);
          _ = hints.Add(
            new RenameFieldHint(typeof(Person), "Id", "IdId")
          );
        }
      }
    }
  }
}
