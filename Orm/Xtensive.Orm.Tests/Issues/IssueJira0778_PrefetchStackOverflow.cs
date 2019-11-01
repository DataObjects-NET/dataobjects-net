// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.10.24

using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0778_PrefetchStackOverflowModel;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm.Tests.Issues
{
  [Explicit]
  public class IssueJira0778_PrefetchStackOverflow : AutoBuildTest
  {
    private class TestDataInvalidException : Exception
    {

    }

    private const int CustomerCount = 161000;

    private bool isSchemaRecreated = false;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Recipient).Assembly, typeof (Recipient).Namespace);
      return configuration;
    }

    protected override Domain BuildDomain(DomainConfiguration configuration)
    {
      var firstTryConfig = configuration.Clone();
      firstTryConfig.UpgradeMode = DomainUpgradeMode.Validate;
      try {
        //try to avoid long population
        var domain = base.BuildDomain(firstTryConfig);
        ValidateTestData(domain);
      }
      catch (SchemaSynchronizationException exception) {
        //schemas differ
        isSchemaRecreated = true;
      }
      catch (TestDataInvalidException) {
        // schemas are the same but data in not ok
        // create so override existing schema and publish correct data
        isSchemaRecreated = true;
      }
      var secondTry = configuration.Clone();
      secondTry.UpgradeMode = DomainUpgradeMode.Recreate;
      return base.BuildDomain(secondTry);
    }

    protected override void PopulateData()
    {
      if (isSchemaRecreated)
        return;

      PopulateEmployeeHierarchy(Domain);
      PopulateCustomers(Domain);
      PopulateContactsForEmployees(Domain);
      PopulateRecipients(Domain);
      PopulateTestHierarchy(Domain);
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession(new SessionConfiguration(SessionOptions.Default| SessionOptions.AutoActivation) {DefaultCommandTimeout = 3000}))
      using (var tx = session.OpenTransaction()) {
        ExecuteAsync(session);
      }
    }

    [Test]
    public void OneLevelPrefetchTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<AAAA>()
          .Prefetch(a => a.Reference1)
          .Prefetch(a => a.Reference2)
          .Prefetch(a => a.Reference3)
          .Prefetch(a => a.SomeObject);

        var localResult = result.ToList();
        int count = 0;
        session.Events.DbCommandExecuted += (sender, args) => count++;
        foreach (var aaaa in localResult) {
          Console.WriteLine("Reference {0}, Address {1}, Passport {2}", aaaa.Reference1, aaaa.Reference1.Address, aaaa.Reference1.Passport);
          Console.WriteLine("Reference {0}, Address {1}, Passport {2}", aaaa.Reference2, aaaa.Reference2.Address, aaaa.Reference2.Passport);
          Console.WriteLine("Reference {0}, Address {1}, Passport {2}", aaaa.Reference3, aaaa.Reference3.Address, aaaa.Reference3.Passport);
          Console.WriteLine("Some object {0}", Encoding.UTF8.GetString(aaaa.SomeObject));
        }

        Assert.That(count, Is.EqualTo(0));
      }
    }

    [Test]
    public void TwoLevelPrefetchTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<AAAA>()
          .Prefetch(a => a.Reference1)
          .Prefetch(a => a.Reference2)
          .Prefetch(a => a.Reference3)
          .Prefetch(a => a.SomeObject)
          .Prefetch(a => a.Bbbbs
            .Prefetch(b => b.Reference4)
            .Prefetch(b => b.Reference5)
            .Prefetch(b => b.Reference6)
            .Prefetch(b => b.SomeObject)
          );

        var localResult = result.ToList();
        int count = 0;
        session.Events.DbCommandExecuted += (sender, args) => count++;
        foreach (var aaaa in localResult) {
          Console.WriteLine("Reference {0}, Address {1}, Passport {2}", aaaa.Reference1, aaaa.Reference1.Address, aaaa.Reference1.Passport);
          Console.WriteLine("Reference {0}, Address {1}, Passport {2}", aaaa.Reference2, aaaa.Reference2.Address, aaaa.Reference2.Passport);
          Console.WriteLine("Reference {0}, Address {1}, Passport {2}", aaaa.Reference3, aaaa.Reference3.Address, aaaa.Reference3.Passport);
          Console.WriteLine("Some object {0}", Encoding.UTF8.GetString(aaaa.SomeObject));

          foreach (var bbbb in aaaa.Bbbbs) {
            Console.WriteLine("Reference {0}, Address {1}, Passport {2}", bbbb.Reference4, bbbb.Reference4.Address, bbbb.Reference4.Passport);
            Console.WriteLine("Reference {0}, Address {1}, Passport {2}", bbbb.Reference5, bbbb.Reference5.Address, bbbb.Reference5.Passport);
            Console.WriteLine("Reference {0}, Address {1}, Passport {2}", bbbb.Reference6, bbbb.Reference6.Address, bbbb.Reference6.Passport);
            Console.WriteLine("Some object {0}", Encoding.UTF8.GetString(bbbb.SomeObject));
          }
        }

        Assert.That(count, Is.EqualTo(0));
      }
    }

    [Test]
    public void ThreeLevelPrefetchTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<AAAA>()
          .Prefetch(a => a.Reference1)
          .Prefetch(a => a.Reference2)
          .Prefetch(a => a.Reference3)
          .Prefetch(a => a.SomeObject)
          .Prefetch(a => a.Bbbbs
            .Prefetch(b => b.Reference4)
            .Prefetch(b => b.Reference5)
            .Prefetch(b => b.Reference6)
            .Prefetch(b => b.SomeObject)
            .Prefetch(b => b.CCCCs
              .Prefetch(c => c.Reference7)
              .Prefetch(c => c.Reference8)
              .Prefetch(c => c.Reference9)
              .Prefetch(c => c.SomeObject)
            )
          );

        var localResult = result.ToList();
        int count = 0;
        session.Events.DbCommandExecuted += (sender, args) => count++;
        foreach (var aaaa in localResult) {
          Console.WriteLine("Reference {0}, Address {1}, Passport {2}", aaaa.Reference1, aaaa.Reference1.Address, aaaa.Reference1.Passport);
          Console.WriteLine("Reference {0}, Address {1}, Passport {2}", aaaa.Reference2, aaaa.Reference2.Address, aaaa.Reference2.Passport);
          Console.WriteLine("Reference {0}, Address {1}, Passport {2}", aaaa.Reference3, aaaa.Reference3.Address, aaaa.Reference3.Passport);
          Console.WriteLine("Some object {0}", Encoding.UTF8.GetString(aaaa.SomeObject));

          foreach (var bbbb in aaaa.Bbbbs) {
            Console.WriteLine("Reference {0}, Address {1}, Passport {2}", bbbb.Reference4, bbbb.Reference4.Address, bbbb.Reference4.Passport);
            Console.WriteLine("Reference {0}, Address {1}, Passport {2}", bbbb.Reference5, bbbb.Reference5.Address, bbbb.Reference5.Passport);
            Console.WriteLine("Reference {0}, Address {1}, Passport {2}", bbbb.Reference6, bbbb.Reference6.Address, bbbb.Reference6.Passport);
            Console.WriteLine("Some object {0}", Encoding.UTF8.GetString(bbbb.SomeObject));

            foreach (var cccc in bbbb.CCCCs) {
              Console.WriteLine("Reference {0}, Address {1}, Passport {2}", cccc.Reference7, cccc.Reference7.Address, cccc.Reference7.Passport);
              Console.WriteLine("Reference {0}, Address {1}, Passport {2}", cccc.Reference8, cccc.Reference8.Address, cccc.Reference8.Passport);
              Console.WriteLine("Reference {0}, Address {1}, Passport {2}", cccc.Reference9, cccc.Reference9.Address, cccc.Reference9.Passport);
              Console.WriteLine("Some object {0}", Encoding.UTF8.GetString(cccc.SomeObject));
            }
          }
        }
        Assert.That(count, Is.EqualTo(0));
      }
    }

    [Test]
    public void CyclicReferenceTest()
    {
      using (var populateSession = Domain.OpenSession())
      using (var tx = populateSession.OpenTransaction()) {
        var firstOperation = new IssueJira0778_PrefetchStackOverflowModel.Operation();
        var secondOperation = new IssueJira0778_PrefetchStackOverflowModel.Operation();
        firstOperation.PreviousOperation = secondOperation;
        firstOperation.NextOperation = secondOperation;
        secondOperation.PreviousOperation = firstOperation;
        secondOperation.NextOperation = firstOperation;

        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = session.Query.All<IssueJira0778_PrefetchStackOverflowModel.Operation>()
          .Prefetch(o => o.PreviousOperation)
          .Prefetch(o => o.NextOperation)
          .ToList();

        int count = 0;
        session.Events.DbCommandExecuted += (sender, args) => count++;
        foreach (var op in result) {
          Console.WriteLine("Prevoius operation {0}", op.PreviousOperation);
        }
        Assert.That(count, Is.EqualTo(0));
      }
    }

    private void ExecuteAsync(Session session)
    {
      var recipients = session.Query.All<Recipient>()
        .Active()
        .Where(r => !r.Data.Contains(nameof (CustomerData.Email)))
        .Prefetch(r => r.Contact)
        .ToList();

      Assert.That(recipients.Count, Is.AtLeast(1000));
      int counter = 0;
      session.Events.DbCommandExecuted += (sender, args) => counter++;
      foreach (var recipient in recipients) {
        var contact = recipient.Contact;
      }
      Assert.That(counter, Is.EqualTo(0));
    }

    private static void ValidateTestData(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var contactCount = session.Query.ExecuteDelayed(q => q.All<Contact>().Count());
        var employeeCount = session.Query.ExecuteDelayed(q => q.All<Employee>().Count());
        var recipientCount = session.Query.ExecuteDelayed(q => q.All<Recipient>().Count());
        var boss = session.Query.ExecuteDelayed(q => q.All<Employee>().FirstOrDefault(emp => emp.FirstName == "The" && emp.LastName == "Boss"));
        var As = session.Query.ExecuteDelayed(q => q.All<AAAA>().Count());
        var Bs = session.Query.ExecuteDelayed(q => q.All<BBBB>().Count());
        var Cs = session.Query.ExecuteDelayed(q => q.All<CCCC>().Count());

        var isValid = recipientCount.Value==CustomerCount &&
          employeeCount.Value== 45 &&
          boss.Value != null &&
          contactCount.Value > CustomerCount * 3 &&
          As.Value == 10 &&
          Bs.Value == 100 &&
          Cs.Value == 1000;

        if (!isValid)
          throw new TestDataInvalidException();
      }
    }

    private static void PopulateEmployeeHierarchy(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var ceo = new Employee(session, "The", "Boss");
        var gm = new Employee(session, "General", "Manager");
        var ipDirector = new Employee(session, "Investment & Placement", "Director");
        var pManager = new Employee(session, "Placement", "Director");
        new Employee(session, "Placement", "Staff #1");
        new Employee(session, "Placement", "Staff #2");
        new Employee(session, "Placement", "Staff #3");

        var iManager = new Employee(session, "Investment", "Manager");
        new Employee(session, "Investment", "Staff #1");
        new Employee(session, "Investment", "Staff #2");
        new Employee(session, "Investment", "Staff #3");

        var bdsManager = new Employee(session, "Business Development", "S.Manager");
        new Employee(session, "Projects Marketing and Exhibition Team", "Member #1");
        new Employee(session, "Projects Marketing and Exhibition Team", "Member #2");
        new Employee(session, "Projects Marketing and Exhibition Team", "Member #3");

        new Employee(session, "Projects Marketing and Exhibition Team", "Member #1");
        new Employee(session, "Projects Marketing and Exhibition Team", "Member #2");
        new Employee(session, "Projects Marketing and Exhibition Team", "Member #3");

        var pdDirector = new Employee(session, "Project Development", "Director");
        new Employee(session, "Project", "Architect #1");
        new Employee(session, "Project", "Architect #2");
        new Employee(session, "Project", "Architect #3");

        new Employee(session, "Project", "Manager #1");
        new Employee(session, "Project", "Manager #2");
        new Employee(session, "Project", "Manager #3");

        var hraDirector = new Employee(session, "HR & Admin", "Director");
        new Employee(session, "HR", "Staff #1");
        new Employee(session, "HR", "Staff #2");
        new Employee(session, "HR", "Staff #3");

        new Employee(session, "Admin", "Staff #1");
        new Employee(session, "Admin", "Staff #2");
        new Employee(session, "Admin", "Staff #3");

        new Employee(session, "Support", "Serviceman #1");
        new Employee(session, "Support", "Serviceman #2");
        new Employee(session, "Support", "Serviceman #3");

        var fitDirector = new Employee(session, "Finance & IT", "Director");
        var chiefAccountant = new Employee(session, "Chief", "Accountant");

        new Employee(session, "Accountant", "Clerk #1");
        new Employee(session, "Accountant", "Clerk #2");
        new Employee(session, "Accountant", "Clerk #3");
        new Employee(session, "Accountant", "Clerk #4");
        new Employee(session, "Accountant", "Clerk #5");

        new Employee(session, "IT","Administrator #1");
        new Employee(session, "IT", "Administrator #2");
        new Employee(session, "IT", "Administrator #3");

        tx.Complete();
      }
    }

    private static void PopulateCustomers(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        for (int i = 0; i < CustomerCount; i++) {
          var customer = new Customer(session, Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
          new Contact(session, customer, ContactType.Email, ContactGenerator.GetEmail());
          new Contact(session, customer, ContactType.Fax, ContactGenerator.GetPhone());
          new Contact(session, customer, ContactType.Phone, ContactGenerator.GetPhone());
        }
        tx.Complete();
      }
    }

    private static void PopulateContactsForEmployees(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        foreach (var employee in session.Query.All<Employee>()) {
          new Contact(session, employee, ContactType.Email, ContactGenerator.GetEmail()) {Active = true};
          new Contact(session, employee, ContactType.Fax, ContactGenerator.GetPhone()) {Active = true};
          new Contact(session, employee, ContactType.Phone, ContactGenerator.GetPhone()) {Active= true};
        }
        tx.Complete();
      }
    }

    public static void PopulateRecipients(Domain domain)
    {
      Random random = new Random();
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var customers = session.Query.All<Customer>().ToArray();
        foreach (var customer in customers) {
          var contactsToChoose = customer.Contacts.ToArray();
          new Recipient(session, new Audience(session)) {
            Contact = contactsToChoose[random.Next(0, contactsToChoose.Length)],
            Active = true,
            Data = Guid.NewGuid().ToString(),
            Customer = customer
          };
        }
        tx.Complete();
      }
    }

    public static void PopulateTestHierarchy(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          var aaaa = new AAAA {
            Reference1 = new ReferenceEntity1 {
              Reference = new ReferenceEntity10(),
              Address = new Address {
                Country = "Country #1" + i +"00",
                Region = "Region #1" + i +"00",
                City = "City #1" + i +"00",
                Street = "Street #1" + i +"00",
                Index = "Index #1" + i +"00",
                Building = "Building #1" + i +"00",
              },
              Passport = new PassportData {
                Number = "N#1" + i,
                Series = "S#1" + i
              }
            },
            Reference2 = new ReferenceEntity2 {
              Reference = new ReferenceEntity10(),
              Address = new Address {
                Country = "Country #2" + i + "00",
                Region = "Region #2" + i + "00",
                City = "City #2" + i + "00",
                Street = "Street #2" + i + "00",
                Index = "Index #2" + i + "00",
                Building = "Building #2" + i + "00",
              },
              Passport = new PassportData {
                Number = "N#2" + i,
                Series = "S#2" + i
              }
            },
            Reference3 = new ReferenceEntity3 {
              Reference = new ReferenceEntity10(),
              Address = new Address {
                Country = "Country #3" + i + "00",
                Region = "Region #3" + i + "00",
                City = "City #3" + i + "00",
                Street = "Street #3" + i + "00",
                Index = "Index #3" + i + "00",
                Building = "Building #3" + i + "00",
              },
              Passport = new PassportData {
                Number = "N#2" + i,
                Series = "S#2" + i,
              }
            },
            SomeObject = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())
          };
          for (int j = 0; j < 10; j++) {
            var bbbb = new BBBB {
              Reference4 = new ReferenceEntity4() {
                Reference = new ReferenceEntity10(),
                Address = new Address {
                  Country = "Country #1" + i + j + "00",
                  Region = "Region #1" + i + j + "00",
                  City = "City #1" + i + j + "00",
                  Street = "Street #1" + i + j + "00",
                  Index = "Index #1" + i + j + "00",
                  Building = "Building #4" + i + j + "00",
                },
                Passport = new PassportData {
                  Number = "N#4" + i + j,
                  Series = "S#4" + i + j
                }
              },
              Reference5 = new ReferenceEntity5() {
                Reference = new ReferenceEntity10(),
                Address = new Address {
                  Country = "Country #5" + i + j + "00",
                  Region = "Region #5" + i + j + "00",
                  City = "City #5" + i + j + "00",
                  Street = "Street #5" + i + j + "00",
                  Index = "Index #5" + i + j + "00",
                  Building = "Building #5" + i + j + "00",
                },
                Passport = new PassportData {
                  Number = "N#5" + i + j,
                  Series = "S#5" + i + j
                }
              },
              Reference6 = new ReferenceEntity6 {
                Reference = new ReferenceEntity10(),
                Address = new Address {
                  Country = "Country #6" + i + j + "00",
                  Region = "Region #6" + i + j + "00",
                  City = "City #6" + i + j + "00",
                  Street = "Street #6" + i + j + "00",
                  Index = "Index #6" + i + j + "00",
                  Building = "Building #6" + i + j + "00",
                },
                Passport = new PassportData {
                  Number = "N#6" + i + j,
                  Series = "S#6" + i + j
                }
              },
              SomeObject = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())
            };
            for (int k = 0; k < 10; k++) {
              var cccc = new CCCC {
                Reference7 = new ReferenceEntity7 {
                  Reference = new ReferenceEntity10(),
                  Address = new Address {
                    Country = "Country #7" + i + j + k + "00",
                    Region = "Region #7" + i + j + k + "00",
                    City = "City #7" + i + j + k + "00",
                    Street = "Street #7" + i + j + k + "00",
                    Index = "Index #7" + i + j + k + "00",
                    Building = "Building #7" + i + j + k + "00",
                  },
                  Passport = new PassportData {
                    Number = "N#7" + i + j + k,
                    Series = "S#7" + i + j + k
                  }
                },
                Reference8 = new ReferenceEntity8 {
                  Reference = new ReferenceEntity10(),
                  Address = new Address {
                    Country = "Country #8" + i + j + k + "00",
                    Region = "Region #8" + i + j + k + "00",
                    City = "City #8" + i + j + k + "00",
                    Street = "Street #8" + i + j + k + "00",
                    Index = "Index #8" + i + j + k + "00",
                    Building = "Building #8" + i + j + k + "00",
                  },
                  Passport = new PassportData {
                    Number = "N#8" + i + j + k,
                    Series = "S#8" + i + j + k
                  }
                },
                Reference9 = new ReferenceEntity9 {
                  Reference = new ReferenceEntity10(),
                  Address = new Address {
                    Country = "Country #9" + i + j + k + "00",
                    Region = "Region #9" + i + j + k + "00",
                    City = "City #9" + i + j + k + "00",
                    Street = "Street #9" + i + j + k + "00",
                    Index = "Index #9" + i + j + k + "00",
                    Building = "Building #9" + i + j + k + "00",
                  },
                  Passport = new PassportData {
                    Number = "N#9" + i + j + k,
                    Series = "S#9" + i + j + k
                  }
                },
                SomeObject = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())
              };
              bbbb.CCCCs.Add(cccc);
            }
            aaaa.Bbbbs.Add(bbbb);
          }
        }
        tx.Complete();
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0778_PrefetchStackOverflowModel
{
  public class ContactGenerator
  {
    private static Random Random = new Random();

    public const string Template = "+7({0}){1}-{2}-{3}";
    public readonly static string[] Hosts = new[] {"@gimail.com", "@jahoo.com", "@inlook.com", "@tindax.ru"};

    public static string GetEmail()
    {
      return Guid.NewGuid().ToString().Replace('-', '_') + Hosts[Random.Next(0, 4)];
    }

    public static string GetFax()
    {
      return GetPhone();
    }

    public static string GetPhone()
    {
      var code = Random.Next(900, 950);
      var block1 = Random.Next(100, 1000);
      var block2 = Random.Next(10, 100);
      var block3 = Random.Next(10, 100);
      return string.Format(Template, code, block1, block2, block3);
    }
  }

  public static class Extensions
  {
    public static IQueryable<Recipient> Active(this IQueryable<Recipient> source)
    {
      return source.Where(e => e.Active);
    }
  }

  public enum ContactType
  {
    Phone,
    Email,
    Fax
  }

  public abstract class BusinessEntityBase : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public bool Active { get; set; }

    public BusinessEntityBase(Session session)
      : base(session)
    {
    }
  }

  public interface IHasContacts: IEntity
  {
    [Field]
    EntitySet<Contact> Contacts { get; }
  }

  [HierarchyRoot]
  public class Audience : BusinessEntityBase
  {

    public Audience(Session session)
      : base(session)
    {

    }
  }

  public class CustomerData
  {
    public static string Email => "email";
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class Recipient : BusinessEntityBase
  {
    public const int DataMaxLength = 4000;

    [Field(Indexed = true, Nullable = false)]
    [NotNullConstraint(ValidateOnlyIfModified = true)]
    public Audience Audience { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Contact Contact { get; set; }

    [Field(Length = DataMaxLength, LazyLoad = false)]
    public string Data { get; set; }

    public Recipient(Session session, Audience audience) : base(session)
    {
      Audience = audience;
    }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class Contact : BusinessEntityBase
  {
    public const int MemoMaxLength = 250;

    [Field]
    [NotNullConstraint(ValidateOnlyIfModified = true)]
    [Association(PairTo = nameof(IHasContacts.Contacts))]
    public IHasContacts Owner { get; private set; }

    [Field]
    public ContactType Type { get; private set; }

    [Field(Nullable = false)]
    [NotNullOrEmptyConstraint(ValidateOnlyIfModified = true)]
    public string Value { get; private set; }

    [Field]
    public string ReversePhone { get; private set; }

    [Field(Length = MemoMaxLength)]
    public string Memo { get; set; }

    [Field, Obsolete] // Temporarily resurrected
    public bool MessagesEnabled { get; set; }

    [Field]
    public bool JobRemindersEnabled { get; set; }

    [Field]
    public bool MarketingUpdatesEnabled { get; set; }

    [Field(DefaultSqlExpression = "GETUTCDATE()")]
    public DateTime ModifiedOn { get; set; }

    public Contact(Session session, IHasContacts owner, ContactType type, string value)
      : base(session)
    {
      Owner = owner;
      Type = type;
      MarketingUpdatesEnabled = true;
      Value = value;
    }
  }

  [HierarchyRoot]
  public class Employee : BusinessEntityBase, IHasContacts
  {
    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public EntitySet<Contact> Contacts { get; private set; }

    public Employee(Session session, string firstName, string lastName)
      :base(session)
    {
      FirstName = firstName;
      LastName = lastName;
    }
  }

  [HierarchyRoot]
  public class Customer : BusinessEntityBase, IHasContacts
  {
    [Field]
    public string FistName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    public EntitySet<Contact> Contacts { get; private set; }

    public Customer(Session session, string firstName, string lastName)
      :base(session)
    {
      FistName = firstName;
      LastName = lastName;
    }
  }

  [HierarchyRoot]
  public class AAAA : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public ReferenceEntity1 Reference1 { get; set; }

    [Field]
    public ReferenceEntity2 Reference2 { get; set; }

    [Field]
    public ReferenceEntity3 Reference3 { get; set; }

    [Field(LazyLoad = true)]
    public byte[] SomeObject { get; set; }

    [Field]
    [Association(PairTo = "Owner")]
    public EntitySet<BBBB> Bbbbs { get; private set; }
  }

  [HierarchyRoot]
  public class BBBB : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public AAAA Owner { get; set; }

    [Field]
    public ReferenceEntity4 Reference4 { get; set; }

    [Field]
    public ReferenceEntity5 Reference5 { get; set; }

    [Field]
    public ReferenceEntity6 Reference6 { get; set; }

    [Field(LazyLoad = true)]
    public byte[] SomeObject { get; set; }

    [Field]
    [Association(PairTo = "Owner")]
    public EntitySet<CCCC> CCCCs { get; private set; }
  }

  [HierarchyRoot]
  public class CCCC : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public BBBB Owner { get; set; }

    [Field]
    public ReferenceEntity7 Reference7 { get; set; }

    [Field]
    public ReferenceEntity8 Reference8 { get; set; }

    [Field]
    public ReferenceEntity9 Reference9 { get; set; }

    [Field(LazyLoad = true)]
    public byte[] SomeObject { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity1 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public PassportData Passport { get; set; }

    [Field]
    public ReferenceEntity10 Reference { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity2 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public PassportData Passport { get; set; }

    [Field]
    public ReferenceEntity10 Reference { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity3 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public PassportData Passport { get; set; }

    [Field]
    public ReferenceEntity10 Reference { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity4 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public PassportData Passport { get; set; }

    [Field]
    public ReferenceEntity10 Reference { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity5 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public PassportData Passport { get; set; }

    [Field]
    public ReferenceEntity10 Reference { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity6 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public PassportData Passport { get; set; }

    [Field]
    public ReferenceEntity10 Reference { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity7 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public PassportData Passport { get; set; }

    [Field]
    public ReferenceEntity10 Reference { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity8 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public PassportData Passport { get; set; }

    [Field]
    public ReferenceEntity10 Reference { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity9 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public PassportData Passport { get; set; }

    [Field]
    public ReferenceEntity10 Reference { get; set; }
  }

  [HierarchyRoot]
  public class ReferenceEntity10 : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class Operation : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public Operation PreviousOperation { get; set; }

    [Field]
    public Operation NextOperation { get; set; }
  }


  public class Address : Structure
  {
    [Field(Length = 128)]
    public string Country { get; set; }

    [Field(Length = 128)]
    public string Region { get; set; }

    [Field(Length = 128)]
    public string City { get; set; }

    [Field(Length = 128)]
    public string Street { get; set; }

    [Field(Length = 128)]
    public string Building { get; set; }

    [Field(Length = 128)]
    public string Index { get; set; }

    public override string ToString()
    {
      var builder = new StringBuilder();

      builder.Append(Building);
      builder.Append(Street).Append(", ");
      builder.Append(City).Append(", ");
      builder.Append(Region).Append(", ");
      builder.Append(Index).Append(", ");
      builder.Append(Country);

      return builder.ToString();
    }
  }

  public class PassportData : Structure
  {
    [Field(Length = 128)]
    public string Series { get; set; }

    [Field(Length = 128)]
    public string Number { get; set; }

    public override string ToString()
    {
      return Series + " " + Number;
    }
  }
}