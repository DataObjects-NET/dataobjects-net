// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.11.11

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;
using System.Linq;

namespace Xtensive.Orm.Manual.Transactions.SessionSwitching
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string ShortName { get; set; }

    [Field]
    public FullName FullName { get; set; }

    [Field]
    public Passport Passport { get; set; }

    [Field]
    public EntitySet<Notification> Notifications { get; private set; }
  }

  [HierarchyRoot]
  public class Passport : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Length = 10)]
    public string Series { get; set; }

    [Field(Length = 20)]
    public string Number { get; set; }
  }

  [HierarchyRoot]
  public class Notification : Entity
  {
    [Field, Key]
    public int Id { get; set; }
  }

  public class FullName : Structure
  {
    [Field(Length = 200)]
    public string FirstName { get; set; }

    [Field(Length = 200)]
    public string LastName { get; set; }

    [Field(Length = 200)]
    public string MidName { get; set; }
  }


  #endregion

  [TestFixture]
  public class SessionSwitchingTest : AutoBuildTest
  {
    private readonly int[] detachedNotificationKeys = new int [3];

    [Test]
    public void DenySwitchingOnPrimitiveFieldReadTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          string name = personA.ShortName;

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              name = personA.ShortName;
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              name = personA.ShortName;
            }
          }
        }
      }
    }

    [Test]
    public void DenySwitchingOnPrimitiveFieldWriteTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) {// Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)){ // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          personA.ShortName = "abc";

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              personA.ShortName = "def";
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              personA.ShortName = "ghi";
            }
          }
        }
      }
    }

    [Test]
    public void DenySwitchingOnReferenceFieldReadTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          var passport = personA.Passport;

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              passport = personA.Passport;
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              passport = personA.Passport;
            }
          }
        }
      }
    }

    [Test]
    public void DenySwitchingOnReferenceFieldWriteTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();
        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              personA.Passport = new Passport();
            });
          }
        }
        personA.Passport = new Passport();
        Assert.That(personA.Session, Is.EqualTo(personA.Passport.Session));
      }
    }

    [Test]
    public void DenySwitchingStructureReadTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          var fullName = personA.FullName;

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              fullName = personA.FullName;
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              fullName = personA.FullName;
            }
          }
        }
      }
    }

    [Test]
    public void DenySwitchingStructureWriteTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          personA.FullName = new FullName() {FirstName = "abc", LastName = "def"};

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
                personA.FullName = new FullName {FirstName = "def", LastName = "ghi"};
              });
            // Blocking session switching check
            using (Session.Deactivate()) {
              
            }
          }
        }

        personA.FullName = new FullName {FirstName = "jkl", LastName = "mno"};
        Assert.That(personA.Session, Is.EqualTo(personA.FullName.Session));
      }
    }

    [Test]
    public void DenySwitchingOnStructureFieldReadTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          var lastName = personA.FullName.LastName;

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              lastName = personA.FullName.LastName;
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              lastName = personA.FullName.LastName;
            }
          }
        }
      }
    }

    [Test]
    public void DenySwitchingOnStructureFieldWriteTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          personA.FullName.LastName = "abc";

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              personA.FullName.LastName = "def";
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              personA.FullName.LastName = "ghi";
            }
          }
        }
      }
    }

    [Test]
    public void DenySwitchingOnEntitySetAddTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();
        var notifications = sessionA.Query.All<Notification>().Where(n => n.Id.In(detachedNotificationKeys)).ToArray();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          personA.Notifications.Add(notifications[0]);

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              personA.Notifications.Add(notifications[1]);
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              personA.Notifications.Add(notifications[2]);
            }
          }
        }
      }

      sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();
        var notification1 = new Notification();
        var notification2 = new Notification();
        var notification3 = new Notification();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          personA.Notifications.Add(notification1);

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              personA.Notifications.Add(notification2);
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              personA.Notifications.Add(notification3);
            }
          }
        }
      }
    }

    [Test]
    public void DenySwitchingOnEntitySetRemoveTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First(p=> p.Notifications.Any());
        var notifications = personA.Notifications.ToArray();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          personA.Notifications.Remove(notifications[0]);

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              personA.Notifications.Remove(notifications[1]);
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              personA.Notifications.Remove(notifications[2]);
            }
          }
        }
      }
    }

    [Test]
    public void DenySwitchingOnEntitySetContainsTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()){ // Open & activate
        var personA = sessionA.Query.All<Person>().First(p => p.Notifications.Any());
        var notifications = personA.Notifications.ToArray();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, 
          // but allowed, since there is no running transaction
          personA.Notifications.Contains(notifications[0]);

          using (var tx = Session.Demand().OpenTransaction()) {
            // Session switching (from sessionB to sessionA) will be detected & blocked here
            AssertEx.Throws<InvalidOperationException>(() => {
              personA.Notifications.Contains(notifications[1]);
            });
            // Blocking session switching check
            using (Session.Deactivate()) {
              personA.Notifications.Contains(notifications[2]);
            }
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnPrimitiveFieldReadTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            var name = personA.ShortName;
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnPrimitiveFieldWriteTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            personA.ShortName = "abc";
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnReferenceFieldReadTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            var passport = personA.Passport;
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnReferenceFieldWriteTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            // Though switching is allowed, reference value should be from the same session.
            Assert.Throws<InvalidOperationException>(() => personA.Passport = new Passport());
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnStructureReadTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            var fullName = personA.FullName;
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnStructureWriteTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            personA.FullName = new FullName {LastName = "abc", FirstName = "def"};
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnStructureFieldReadTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            var firstName = personA.FullName.FirstName;
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnStructureFieldWriteTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            personA.FullName.FirstName = "abc";
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnEntitySetAddTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();
        var notification = sessionA.Query.All<Notification>().First(n => n.Id.In(detachedNotificationKeys));

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            personA.Notifications.Add(notification);
          }
        }
      }

      sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First();
        var notification = new Notification();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            personA.Notifications.Add(notification);
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnEntitySetRemoveTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First(p => p.Notifications.Any());
        var notification = personA.Notifications.First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            personA.Notifications.Remove(notification);
          }
        }
      }
    }

    [Test]
    public void AllowSwitchingOnEntitySetContainsTest()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AllowSwitching;
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var sessionA = Domain.OpenSession(sessionCfg))
      using (var tx1 = sessionA.OpenTransaction()) { // Open & activate
        var personA = sessionA.Query.All<Person>().First(p => p.Notifications.Any());
        var notification = personA.Notifications.First();

        using (var sessionB = Domain.OpenSession(sessionCfg)) { // Open & activate
          // Session switching (from sessionB to sessionA) will be detected here, but allowed
          using (var tx = Session.Demand().OpenTransaction()) {
            personA.Notifications.Contains(notification);
          }
        }
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (Person).Assembly, typeof (Person).Namespace);
      return config;
    }

    protected override void PopulateData()
    {
      var sessionCfg = new SessionConfiguration();
      sessionCfg.Options |= SessionOptions.AutoActivation;

      using (var session = Domain.OpenSession(sessionCfg))
      using (var transactionScope = session.OpenTransaction()) {
        // Creating initial content
        new Person {
          ShortName = "Tereza",
          Notifications = {new Notification(), new Notification(), new Notification()},
          FullName = new FullName {FirstName = "Tereza", LastName = "May"},
          Passport = new Passport()
        };
        new Person {
          ShortName = "Ivan",
          FullName = new FullName {FirstName = "Ivan", LastName = "Bunin"},
          Passport = new Passport()
        };
        detachedNotificationKeys[0] = new Notification().Id;
        detachedNotificationKeys[1] = new Notification().Id;
        detachedNotificationKeys[2] = new Notification().Id;

        transactionScope.Complete();
      }
    }
  }
}