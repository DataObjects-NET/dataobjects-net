// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.10.30

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Validation;
using model1 = Xtensive.Orm.Tests.Storage.AnotherValidation.ValidationModel;
using model2 = Xtensive.Orm.Tests.Storage.AnotherValidation.ImmediateValidationModel;

#region Models
namespace Xtensive.Orm.Tests.Storage.AnotherValidation.ValidationModel
{
  public class Passport : Structure
  {
    [Field(Length = 6)]
    [NotNullConstraint]
    [RegexConstraint("^[0-9]{6}$")]
    public string Number { get; set; }

    [Field(Length = 4)]
    [NotNullConstraint]
    [LengthConstraint(Min = 4, Max = 4)]
    [RegexConstraint("^[0-9]+$")]
    public string Series { get; set; }

    [Field, NotNullOrEmptyConstraint]
    public string Department { get; set; }

    [Field]
    [NotNullConstraint]
    [LengthConstraint(Min = 7, Max = 7)]
    [RegexConstraint("^([0-9]{3})-([0-9]{3})$")]
    public string DepartmentNumber { get; set; }

    [Field]
    [PastConstraint]
    public DateTime DistributeDate { get; set; }
  }

  [HierarchyRoot]
  public sealed class Measure : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, NotNullOrEmptyConstraint]
    public string Name { get; set; }

    [Field(Nullable = true)]
    public string ShortName { get; set; }

    [Field]
    public ChildClass Field1 { get; set; }

    [Field]
    public SecondChildClass Field2 { get; set; }  

    [Field]
    [Association(PairTo = "Measure")]
    public EntitySet<Product> Products { get; set; }
  }

  [HierarchyRoot]
  public sealed class Customer : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Length = 50), NotEmptyConstraint]
    public string FirstName { get; set; }

    [Field(Length = 50), NotEmptyConstraint]
    public string LastName { get; set; }

    [Field, PastConstraint]
    public DateTime Birthday { get; set; }

    [Field(Length = 128), EmailConstraint]
    public string Email { get; set; }

    [Field(Length = 50), RegexConstraint(@"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$")]
    public string Phone { get; set; }

    [Field]
    public Passport Passport { get; set; }

    [Field]
    [Association(PairTo = "Customer")]
    public EntitySet<Order> Orders { get; set; }
  }

  [HierarchyRoot]
  public sealed class Product : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, NotEmptyConstraint]
    public string Name { get; set; }

    [Field, NotNullConstraint]
    public Measure Measure { get; set; }

    [Field]
    [Association(PairTo = "Product")]
    public EntitySet<Review> Reviews { get; set; }

    [Field]
    [Association(PairTo = "Product")]
    public EntitySet<Order> Orders { get; set; }
  }

  [HierarchyRoot]
  public sealed class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, PastConstraint]
    public DateTime Date { get; set; }

    [Field(DefaultValue = false)]
    public bool DeliveryIsOrdered { get; set; }

    [Field, FutureConstraint]
    public DateTime DeliveryDate { get; set; }

    [Field, NotNullConstraint]
    public Customer Customer { get; set; }

    [Field, NotNullConstraint]
    public Product Product { get; set; }
  }
  
  [HierarchyRoot]
  public sealed class Review : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, RangeConstraint(Min = 1, Max = 5)]
    public int Rating { get; set; }

    [Field, NotEmptyConstraint, LengthConstraint(Min = 3, Max = 255)]
    public string Text { get; set; }

    [Field, NotNullConstraint]
    public Product Product { get; set; }
  }
  
  public class BaseEntity : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Length = 50), NotEmptyConstraint]
    public string NotEmptyString { get; set; }

    [Field(Length = 50), NotNullOrEmptyConstraint]
    public string NotNullOrEmptyString { get; set; }

    [Field(Length = 50), NotNullConstraint]
    public string NotNullString { get; set; }

    [Field(Length = 50), LengthConstraint(Min = 2, Max = 10)]
    public string LimitedLengthString { get; set; }

    [Field, PastConstraint]
    public DateTime PastDateTime { get; set; }

    [Field, FutureConstraint]
    public DateTime FutureDateTime { get; set; }

    [Field, RangeConstraint(Min = 3, Max = 10)]
    public int IntValueInRange { get; set; }

    [Field(Length = 50), RegexConstraint(@"^(\%){3,10}$")]
    public string StringForRegexValidation { get; set; }

    [Field]
    public virtual string OverridedField { get; set; }

    [NotNullOrEmptyConstraint]
    public virtual string PropertyBecomesField { get; set; }
  }

  [HierarchyRoot]
  public class ChildClass : BaseEntity
  {
    [NotEmptyConstraint]
    public override string OverridedField { get; set; }
  }

  [HierarchyRoot]
  public class SecondChildClass: BaseEntity
  {
    [Field]
    public override string PropertyBecomesField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage.AnotherValidation.ImmediateValidationModel
{
  public class Passport : Structure
  {
    [Field(Length = 6)]
    [NotNullConstraint(IsImmediate = true)]
    [RegexConstraint("^[0-9]{6}$", IsImmediate = true)]
    public string Number { get; set; }

    [Field(Length = 4)]
    [NotNullConstraint(IsImmediate = true)]
    [LengthConstraint(Min = 4, Max = 4, IsImmediate = true)]
    [RegexConstraint("^[0-9]+$", IsImmediate = true)]
    public string Series { get; set; }

    [Field, NotNullOrEmptyConstraint(IsImmediate = true)]
    public string Department { get; set; }

    [Field]
    [NotNullConstraint(IsImmediate = true)]
    [LengthConstraint(Min = 7, Max = 7, IsImmediate = true)]
    [RegexConstraint("^([0-9]{3})-([0-9]{3})$", IsImmediate = true)]
    public string DepartmentNumber { get; set; }

    [Field]
    [PastConstraint(IsImmediate = true)]
    public DateTime DistributeDate { get; set; }
  }

  [HierarchyRoot]
  public sealed class Measure : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, NotNullOrEmptyConstraint(IsImmediate = true)]
    public string Name { get; set; }

    [Field(Nullable = true)]
    public string ShortName { get; set; }

    [Field]
    public ChildClass Field1 { get; set; }

    [Field]
    public SecondChildClass Field2 { get; set; }  

    [Field]
    [Association(PairTo = "Measure")]
    public EntitySet<Product> Products { get; set; }
  }

  [HierarchyRoot]
  public sealed class Customer : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Length = 50), NotEmptyConstraint(IsImmediate = true)]
    public string FirstName { get; set; }

    [Field(Length = 50), NotEmptyConstraint(IsImmediate = true)]
    public string LastName { get; set; }

    [Field, PastConstraint(IsImmediate = true)]
    public DateTime Birthday { get; set; }

    [Field(Length = 128), EmailConstraint(IsImmediate = true)]
    public string Email { get; set; }

    [Field(Length = 50), RegexConstraint(@"^((8|\+7)[\- ]?)?(\(?\d{3}\)?[\- ]?)?[\d\- ]{7,10}$", IsImmediate = true)]
    public string Phone { get; set; }

    [Field]
    public Passport Passport { get; set; }

    [Field]
    [Association(PairTo = "Customer")]
    public EntitySet<Order> Orders { get; set; }
  }

  [HierarchyRoot]
  public sealed class Product : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, NotEmptyConstraint(IsImmediate = true)]
    public string Name { get; set; }

    [Field, NotNullConstraint(IsImmediate = true)]
    public Measure Measure { get; set; }

    [Field]
    [Association(PairTo = "Product")]
    public EntitySet<Review> Reviews { get; set; }

    [Field]
    [Association(PairTo = "Product")]
    public EntitySet<Order> Orders { get; set; }
  }

  [HierarchyRoot]
  public sealed class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, PastConstraint(IsImmediate = true)]
    public DateTime Date { get; set; }

    [Field(DefaultValue = false)]
    public bool DeliveryIdOrdered { get; set; }

    [Field, FutureConstraint(IsImmediate = true)]
    public DateTime DeliveryDate { get; set; }

    [Field, NotNullConstraint(IsImmediate = true)]
    public Customer Customer { get; set; }

    [Field, NotNullConstraint(IsImmediate = true)]
    public Product Product { get; set; }
  }

  [HierarchyRoot]
  public sealed class Review : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, RangeConstraint(Min = 1, Max = 5, IsImmediate = true)]
    public int Rating { get; set; }

    [Field, NotEmptyConstraint(IsImmediate = true), LengthConstraint(Min = 3, Max = 255, IsImmediate = true)]
    public string Text { get; set; }

    [Field, NotNullConstraint(IsImmediate = true)]
    public Product Product { get; set; }
  }

  public class BaseEntity : Entity
  {
    [Field ,Key]
    public int Id { get; set; }

    [Field(Length = 50), NotEmptyConstraint(IsImmediate = true)]
    public string NotEmptyString { get; set; }

    [Field(Length = 50), NotNullOrEmptyConstraint(IsImmediate = true)]
    public string NotNullOrEmptyString { get; set; }

    [Field(Length = 50), NotNullConstraint(IsImmediate = true)]
    public string NotNullString { get; set; }

    [Field(Length = 50), LengthConstraint(Min = 2, Max = 10, IsImmediate = true)]
    public string LimitedLengthString { get; set; }

    [Field, PastConstraint(IsImmediate = true)]
    public DateTime PastDateTime { get; set; }

    [Field, FutureConstraint(IsImmediate = true)]
    public DateTime FutureDateTime { get; set; }

    [Field, RangeConstraint(Min = 3, Max = 10, IsImmediate = true)]
    public int IntValueInRange { get; set; }

    [Field(Length = 50), RegexConstraint(@"^(\%){3,10}$", IsImmediate = true)]
    public string StringForRegexValidation { get; set; }

    [Field]
    public virtual string OverridedField { get; set; }

    [NotNullOrEmptyConstraint(IsImmediate = true)]
    public virtual string PropertyBecomesField { get; set; }
  }

  [HierarchyRoot]
  public class ChildClass : BaseEntity
  {
    [NotEmptyConstraint(IsImmediate = true)]
    public override string OverridedField { get; set; }
  }

  [HierarchyRoot]
  public class SecondChildClass : BaseEntity
  {
    [Field]
    public override string PropertyBecomesField { get; set; }
  }
}
#endregion

namespace Xtensive.Orm.Tests.Storage.AnotherValidation
{
  [TestFixture]
  class AnotherValidationTest : AutoBuildTest
  {
    private readonly string[] validEmails = new string[] {
        @"Abcdef@example.com",
        @"FredBloggs@example.com",
        @"Joe.Blow@example.com",
        @"A.b.c.d.e.f@example.com",
        @"F+r+e+d+B+loggs@example.com",
        @"user+mailbox@example.com",
        @"customer-department-shipping@example.com",
        @"A12345@example.com",
        @"123A45@example.com",
        @"A12345A@example.com",
        @"s'o'm'e'n'a'm'e@example.com",
        @"some_name@example.com",
        @"s_o_m_e_n_a_m_e@example.com",
        @"_somename@example.com",
        @"_so__mename@example.com",
        @"__________@example.com",
        @"_____@_____.___",
      };
    private readonly string[] notValidEmails = new string[] {
        @".Ablxdf@example",
        @"A...fdsfdfgs@example.com",
        @"+hgksdfhhs@example.com",
        @"+sdjf++@example.com",
        @"'jdhhdhdhdh@example",
        @"d''f'df@example.com",
        @"--------@example.com",
        @"a---@example.com",
        @"h!fkdsdfsdf@example.com",
        @"h#fkdsdfsdf@example.com",
        @"h$fkdsdfsdf@example.com",
        @"h%fkdsdfsdf@example.com",
        @"h&fkdsdfsdf@example.com",
        @"h*fkdsdfsdf@example.com",
        @"h//fkdsdfsdf@example.com",
        @"h\fkdsdfsdf@example.com",
        @"hf?kdsdfsdf@example.com",
        @"hf^kdsdfsdf@example.com",
        @"hf{kdsdfsdf@example.com",
        @"hfk|dsdfsdf@example.com",
        @"hf}kdsdfsdf@example.com",
        @"hfk~dsdfsdf@example.com",
        @"hfkdsdfsdf@.com",
        @"hfkdsdfsdf@example.",
        @"hfkdsdfsdf@example",
        @"hfkdsdfsdf@.example.com",
        @"hfkdsdfsdf@a----.com",
        @"hfkdsdfsdf@------.com"
      };

    private DomainConfiguration configurationForLaterValidation;
    private DomainConfiguration configurationForImmediateValidation;
    

    [TestFixtureSetUp]
    public void Setup()
    {
      configurationForLaterValidation = DomainConfigurationFactory.Create();
      configurationForLaterValidation.UpgradeMode = DomainUpgradeMode.Perform;
      configurationForLaterValidation.Types.Register(typeof (model1.Customer).Assembly, typeof (model1.Customer).Namespace);

      configurationForImmediateValidation = DomainConfigurationFactory.Create();
      configurationForImmediateValidation.UpgradeMode = DomainUpgradeMode.Perform;
      configurationForImmediateValidation.Types.Register(typeof (model2.Customer).Assembly, typeof (model2.Customer).Namespace);
      
      FillData();
    }

    private void FillData()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var mesure = new model1.Measure {Name = "each", ShortName = "ea"};
        var product = new model1.Product {Name = "Coca-Cola bottle 0.5l", Measure = mesure};
        var review = new model1.Review {Product = product, Rating = 5, Text = "Cool. Great drink"};
        var passport = new model1.Passport {Number = "357867", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
        var customer = new model1.Customer {FirstName = "Henry", LastName = "Smith", Birthday = new DateTime(1969, 3, 8), Passport = passport, Email = "login@domain.ru", Phone = "+79879454568"};
        var order = new model1.Order {Product = product, Customer = customer, Date = DateTime.Now, DeliveryDate = DateTime.Now.AddDays(2), DeliveryIsOrdered = true};
        var childClass = new model1.ChildClass {
          NotEmptyString = "d",
          NotNullOrEmptyString = "sdf",
          NotNullString = "skdjfh",
          FutureDateTime = DateTime.Now.AddDays(2),
          IntValueInRange = 5,
          PastDateTime = DateTime.Now,
          StringForRegexValidation = "%%%%",
          OverridedField = "sdfd",
          PropertyBecomesField = "sdfljdkf",
          LimitedLengthString = "uuuuu"
        };
        var secondChildClass = new model1.SecondChildClass {
          NotEmptyString = "d",
          NotNullOrEmptyString = "sdf",
          NotNullString = "skdjfh",
          FutureDateTime = DateTime.Now.AddDays(2),
          IntValueInRange = 5,
          PastDateTime = DateTime.Now,
          StringForRegexValidation = "%%%%",
          OverridedField = "sdfd",
          PropertyBecomesField = "sdfljdkf",
          LimitedLengthString = "uuuuu"
        };
        session.Validate();
        transaction.Complete();
      }
    }

    private Domain BuildDomain(bool isImmediate)
    {
      return isImmediate ? Domain.Build(configurationForImmediateValidation) : Domain.Build(configurationForLaterValidation);
    }

    #region ModelTests
    [Test]
    public void ModelValidationTest()
    {
      var domain = BuildDomain(false);
      var passportType = domain.Model.Types[typeof (model1.Passport)];

      Assert.That(passportType.HasValidators, Is.EqualTo(true));
      Assert.That(passportType.Validators.Count, Is.EqualTo(0));

      var passportTypeField = passportType.Fields["Number"];
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(2));

      passportTypeField = passportType.Fields["Series"];
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(3));

      passportTypeField = passportType.Fields["Department"];
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(1));

      passportTypeField = passportType.Fields["DepartmentNumber"];
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(3));

      passportTypeField = passportType.Fields["DistributeDate"];
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(1));


      domain = BuildDomain(true);
      var passportType1 = domain.Model.Types[typeof (model2.Passport)];

      passportTypeField = passportType1.Fields["Number"];
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.HasImmediateValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(2));

      passportTypeField = passportType1.Fields["Series"];
      Assert.That(passportTypeField.HasImmediateValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(3));

      passportTypeField = passportType1.Fields["Department"];
      Assert.That(passportTypeField.HasImmediateValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(1));

      passportTypeField = passportType1.Fields["DepartmentNumber"];
      Assert.That(passportTypeField.HasImmediateValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(3));

      passportTypeField = passportType1.Fields["DistributeDate"];
      Assert.That(passportTypeField.HasImmediateValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.HasValidators, Is.EqualTo(true));
      Assert.That(passportTypeField.Validators.Count, Is.EqualTo(1));
    }
    #endregion

    #region AdditionTests
    [Test]
    public void NotNullOrEmptyConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          ()=> {
            new model1.Measure {Name = "kilogramm", ShortName = "kg"};
            new model1.Measure {Name = "gramm", ShortName = "g"};
            new model1.Measure {Name = "liter"};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateNotNullOrEmptyConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          ()=> {
            new model2.Measure {Name = "kilogramm", ShortName = "kg"};
            new model2.Measure {Name = "gramm", ShortName = "g"};
            new model2.Measure {Name = "liter"};
          });
      }
    }

    [Test]
    public void NotNullOrEmptyConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(() => {
          new model1.Measure {Name = "", ShortName = "kg/m"};
          session.Validate();
        });
      }
    }

    [Test]
    public void ImmediateNotNullOrEmptyConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ArgumentException>(
          () => {
            new model2.Measure {Name = "", ShortName = "kg/m"};
          });
      }
    }

    [Test]
    public void NotNullConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(()=> {
          var measure = new model1.Measure {Name = "kilogramm", ShortName = "kg"};
          new model1.Product {Name = "Banana", Measure = measure};
          session.Validate();
        });
      }
    }

    [Test]
    public void ImmediateNotNullConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var measure = new model2.Measure {Name = "kilogramm", ShortName = "kg"};
            new model2.Product {Name = "Banana", Measure = measure};
          });
      }
    }

    [Test]
    public void NotNullConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(()=> {
          new model1.Product {Name = "Banana"};
          session.Validate();
        });
      }
    }

    [Test]
    public void ImmediateNotNullConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ArgumentException>(
          () => {
            new model2.Product {Name = "Banana", Measure = null};
          });
      }
    }

    [Test]
    public void NotEmptyConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var measure = new model1.Measure {Name = "kilogramm"};
            var product = new model1.Product {Name = "Banana", Measure = measure};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateNotEmptyConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var measure = new model2.Measure {Name = "kilogramm"};
            var product = new model2.Product {Name = "Banana", Measure = measure};
          });
      }
    }

    [Test]
    public void NotEmptyConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(
          () => {
            var measure = new model1.Measure {Name = "kilogramm"};
            var product = new model1.Product {Name = "", Measure = measure};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateNotEmptyConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ArgumentException>(
          () => {
            var measure = new model2.Measure {Name = "kilogramm"};
            var product = new model2.Product {Name = "", Measure = measure};
          });
      }
    }

    [Test]
    public void RangeConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var measure = new model1.Measure {Name = "kilogramm"};
            var product = new model1.Product {Name = "Banana", Measure = measure};
            new model1.Review {Product = product, Text = "Bananas is very tasty", Rating = 5};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateRangeConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var measure = new model2.Measure {Name = "kilogramm"};
            var product = new model2.Product {Name = "Banana", Measure = measure};
            new model2.Review {Product = product, Text = "Bananas is very tasty", Rating = 5};
          });
      }
    }

    [Test]
    public void RangeConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(
          () => {
            var measure = new model1.Measure {Name = "kilogramm"};
            var product = new model1.Product {Name = "Banana", Measure = measure};
            new model1.Review {Product = product, Text = "Bananas is very tasty", Rating = 6};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateRangeConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ArgumentException>(
          () => {
            var measure = new model2.Measure {Name = "kilogramm"};
            var product = new model2.Product {Name = "Banana", Measure = measure};
            new model2.Review {Product = product, Text = "Bananas is very tasty", Rating = 6};
          });
      }
    }

    [Test]
    public void LengthAndRegexConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var passport = new model1.Passport {Number = "357867", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru", Phone = "+79228963315"};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateLengthAndRegexConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var passport = new model2.Passport {Number = "357867", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru", Phone = "+79228963315"};
          });
      }
    }

    [Test]
    public void LengthConstraintInStructureOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(
          () => {
            var passport = new model1.Passport {Number = "357867", Series = "223", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateLengthConstraintInStructureOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var passport = new model2.Passport {Number = "357867", Series = "223", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
          });
      }
    }

    [Test]
    public void LengthConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(
          () => {
            var measure = new model1.Measure {Name = "kilogramm", ShortName = "kg"};
            var product = new model1.Product {Name = "Banana", Measure = measure};
            var review = new model1.Review {Rating = 5, Product = product, Text = "5"};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateLengthConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ArgumentException>(
          () => {
            var measure = new model2.Measure {Name = "kilogramm", ShortName = "kg"};
            var product = new model2.Product {Name = "Banana", Measure = measure};
            var review = new model2.Review {Rating = 5, Product = product, Text = "5"};
          });
      }
    }

    [Test]
    public void RegexConstraintInStructureOnNotValidData()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(
          () => {
            var passport = new model1.Passport {Number = "3578", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateRegexConstraintInStructureOnNotValidData()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var passport = new model2.Passport {Number = "ab8597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
          });
      }
    }

    [Test]
    public void RegexConstraintOnNotValidData()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(
          () => {
            var passport = new model1.Passport {Number = "357878", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru", Phone = "878d8re7878"};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateRegexConstraintOnNotValidData()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ArgumentException>(
          () => {
            var passport = new model2.Passport {Number = "878597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru", Phone = "878d8re7878"};
          });
      }
    }

    [Test]
    public void EmailConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        foreach (var email in validEmails) {
          Assert.DoesNotThrow(
          ()=> {
            var passport = new model1.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = email};
            session.Validate();
          });
        }
      }
    }

    [Test]
    public void ImmediateEmailConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        foreach (var email in validEmails) {
          Assert.DoesNotThrow(
          () => {
            var passport = new model2.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = email};
          });
        }
      }
    }

    [Test]
    public void EmailConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        foreach (var email in notValidEmails) {
          Assert.Throws<ValidationFailedException>(
          () => {
            var passport = new model1.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = email};
            session.Validate();
          });
        }
      }
    }

    [Test]
    public void ImmediateEmailConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        foreach (var email in notValidEmails) {
          Assert.Throws<ArgumentException>(
          () => {
            var passport = new model2.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = new DateTime(2012, 12, 1)};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = email};
          });
        }
      }
    }

    [Test]
    public void PastConstraintInStructureOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var passport = new model1.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediatePastConstraintInStructureOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var passport = new model2.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
          });
      }
    }

    [Test]
    public void PastConstraintInStructureOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(
          () => {
            var passport = new model1.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now.AddSeconds(2)};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediatePastConstraintInStructureOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var passport = new model2.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now.AddSeconds(2)};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
          });
      }
    }

    [Test]
    public void PastConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          ()=> {
            var product = new model1.Product { Name = "Banana", Measure = new model1.Measure { Name = "kilogramm" } };
            var passport = new model1.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            new model1.Order {Customer = customer, Product = product, Date = DateTime.Now, DeliveryIsOrdered = true, DeliveryDate = DateTime.Now.AddDays(1)};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediatePastConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var product = new model2.Product {Name = "Banana", Measure = new model2.Measure {Name = "kilogramm"}};
            var passport = new model2.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            new model2.Order {Customer = customer, Product = product, Date = DateTime.Now, DeliveryIdOrdered = true, DeliveryDate = DateTime.Now.AddDays(1)};
          });
      }
    }

    [Test]
    public void PastConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(
          ()=> {
            var product = new model1.Product {Name = "Banana", Measure = new model1.Measure {Name = "kilogramm"}};
            var passport = new model1.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            new model1.Order {Customer = customer, Product = product, Date = DateTime.Now.AddMinutes(2), DeliveryIsOrdered = true, DeliveryDate = DateTime.Now.AddDays(1)};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediatePastConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ArgumentException>(
          () => {
            var product = new model2.Product {Name = "Banana", Measure = new model2.Measure {Name = "kilogramm"}};
            var passport = new model2.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            new model2.Order {Customer = customer, Product = product, Date = DateTime.Now.AddMinutes(2), DeliveryIdOrdered = true, DeliveryDate = DateTime.Now.AddDays(1)};
          });
      }
    }

    [Test]
    public void FutureConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          ()=> {
            var product = new model1.Product {Name = "Banana", Measure = new model1.Measure {Name = "kilogramm"}};
            var passport = new model1.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            new model1.Order {Customer = customer, Product = product, Date = DateTime.Now, DeliveryIsOrdered = true, DeliveryDate = DateTime.Now.AddDays(1)};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateFutureConstraintOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var product = new model2.Product {Name = "Banana", Measure = new model2.Measure {Name = "kilogramm"}};
            var passport = new model2.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            new model2.Order {Customer = customer, Product = product, Date = DateTime.Now, DeliveryIdOrdered = true, DeliveryDate = DateTime.Now.AddDays(1)};
          });
      }
    }

    [Test]
    public void FutureConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ValidationFailedException>(
          () => {
            var product = new model1.Product {Name = "Banana", Measure = new model1.Measure {Name = "kilogramm"}};
            var passport = new model1.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model1.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            new model1.Order {Customer = customer, Product = product, Date = DateTime.Now, DeliveryIsOrdered = true, DeliveryDate = DateTime.Now};
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateFutureConstraintOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.Throws<ArgumentException>(
          () => {
            var product = new model2.Product {Name = "Banana", Measure = new model2.Measure {Name = "kilogramm"}};
            var passport = new model2.Passport {Number = "898597", Series = "2233", Department = "\"Horns & Hooves\" Corp.", DepartmentNumber = "358-845", DistributeDate = DateTime.Now};
            var customer = new model2.Customer {FirstName = "Alexey", LastName = "Kulakov", Birthday = new DateTime(2000, 3, 8), Passport = passport, Email = "login@domain.ru"};
            new model2.Order {Customer = customer, Product = product, Date = DateTime.Now, DeliveryIdOrdered = true, DeliveryDate = DateTime.Now};
            session.Validate();
          });
      }
    }

    [Test]
    public void InheritedFieldsValidationOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                    NotEmptyString = "d",
                    NotNullOrEmptyString = "sdf",
                    NotNullString = "skdjfh",
                    FutureDateTime = DateTime.Now.AddDays(2),
                    IntValueInRange = 5,
                    PastDateTime = DateTime.Now,
                    StringForRegexValidation = "%%%%",
                    OverridedField = "sdfd",
                    PropertyBecomesField = "sdfljdkf",
                    LimitedLengthString = "uuuuu"
                  }
                };
            session.Validate();
          });
      }
    }

    [Test]
    public void ImmediateInheritedFieldsValidationOnValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        Assert.DoesNotThrow(
          () => {
            var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                    NotEmptyString = "d",
                    NotNullOrEmptyString = "sdf",
                    NotNullString = "skdjfh",
                    FutureDateTime = DateTime.Now.AddDays(2),
                    IntValueInRange = 5,
                    PastDateTime = DateTime.Now,
                    StringForRegexValidation = "%%%%",
                    OverridedField = "sdfd",
                    PropertyBecomesField = "sdfljdkf",
                    LimitedLengthString = "uuuuu"
                  }
                };
          });
      }
    }

    [Test]
    public void InheritedFieldsConstraintsOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              }; 
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = null,
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = null,
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now,
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 11,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now.AddSeconds(2),
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%&%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuujhgdfhjdfhgjfdhgfjdhghfg"
                }
              };
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateInheritedFieldsConstraintsOnNotValidDataAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = null,
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = null,
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now,
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 11,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now.AddSeconds(2),
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%&%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "sdfd",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuujhgdfhjdfhgjfdhgfjdhghfg"
                }
              };
            });
        }
      }
    }

    [Test]
    public void OverridedFieldConsraintAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              var measure1 = new model1.Measure {
                Name = "measure",
                Field2 = new model1.SecondChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateOverridedFieldConsraintAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field2 = new model2.SecondChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "",
                  PropertyBecomesField = "sdfljdkf",
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
      }
    }

    [Test]
    public void InteritedPropertyBecomesFieldAdditionTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field1 = new model1.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "ddd",
                  PropertyBecomesField = null,
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              var measure = new model1.Measure {
                Name = "measure",
                Field2 = new model1.SecondChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "dfdf",
                  PropertyBecomesField = "",
                  LimitedLengthString = "uuuuu"
                }
              };
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateInteritedPropertyBecomesFieldAdditionTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field1 = new model2.ChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "ddd",
                  PropertyBecomesField = null,
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              var measure = new model2.Measure {
                Name = "measure",
                Field2 = new model2.SecondChildClass {
                  NotEmptyString = "d",
                  NotNullOrEmptyString = "sdf",
                  NotNullString = "skdjfh",
                  FutureDateTime = DateTime.Now.AddDays(2),
                  IntValueInRange = 5,
                  PastDateTime = DateTime.Now,
                  StringForRegexValidation = "%%%%",
                  OverridedField = "dfdf",
                  PropertyBecomesField = null,
                  LimitedLengthString = "uuuuu"
                }
              };
            });
        }
      }
    }
    #endregion

    #region ChangeTests
    [Test]
    public void NotNullOrEmptyConstraintOnValidDataChangingTest()
    {
      
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Measure measure;
        using (var transaction = session.OpenTransaction()) {
          measure = session.Query.All<model1.Measure>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
           () => {
             measure.Name = "Unit";
             session.Validate();
           });
        }
      }
    }

    [Test]
    public void ImmediateNotNullOrEmptyConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Measure measure;
        using (var transaction = session.OpenTransaction()) {
          measure = session.Query.All<model2.Measure>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
           () => {
             measure.Name = "Unit";
           });
        }
      }
    }

    [Test]
    public void NotNullOrEmptyConstraintOnNotValidDataChangingTest()
    {
      
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Measure measure;
        using (var transaction = session.OpenTransaction()) {
          measure = session.Query.All<model1.Measure>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              measure.Name = "";
              session.Validate();
            });
          Assert.Throws<ValidationFailedException>(
            () => {
              measure.Name = null;
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateNotNullOrEmptyConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Measure measure;
        using (var transaction = session.OpenTransaction()) {
          measure = session.Query.All<model2.Measure>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              measure.Name = "";
            });
          Assert.Throws<ArgumentException>(
            () => {
              measure.Name = null;
            });
        }
      }
    }

    [Test]
    public void NotNullConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Product product;
        using (var transaction = session.OpenTransaction()) {
          product = session.Query.All<model1.Product>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              var measure = new model1.Measure {Name = "bottle", ShortName = "bottle"};
              product.Measure = measure;
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateNotNullConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Product product;
        using (var transaction = session.OpenTransaction()) {
          product = session.Query.All<model2.Product>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              var measure = new model2.Measure {Name = "bottle", ShortName = "bottle"};
              product.Measure = measure;
            });
        }
      }
    }

    [Test]
    public void NotNullConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Product product;
        using (var transaction = session.OpenTransaction()) {
          product = session.Query.All<model1.Product>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              product.Measure = null;
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateNotNullConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Product product;
        using (var transaction = session.OpenTransaction()) {
          product = session.Query.All<model2.Product>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              product.Measure = null;
            });
        }
      }
    }

    [Test]
    public void NotEmptyConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Product product;
        using (var transaction = session.OpenTransaction()) {
          product = session.Query.All<model1.Product>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            ()=> {
              product.Name = "Orange";
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateNotEmptyConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Product product;
        using (var transaction = session.OpenTransaction()) {
          product = session.Query.All<model2.Product>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            ()=> {
              product.Name = "Orange";
            });
        }
      }
    }

    [Test]
    public void NotEmptyConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Product product;
        using (var transaction = session.OpenTransaction()) {
          product = session.Query.All<model1.Product>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              product.Name = "";
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateNotEmptyConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Product product;
        using (var transaction = session.OpenTransaction()) {
          product = session.Query.All<model2.Product>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              product.Name = "";
            });
        }
      }
    }

    [Test]
    public void RangeConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Review review;
        using (var transaction = session.OpenTransaction()) {
          review = session.Query.All<model1.Review>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            ()=> {
              review.Rating = 2;
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateRangeConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Review review;
        using (var transaction = session.OpenTransaction()) {
          review = session.Query.All<model2.Review>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            ()=> {
              review.Rating = 2;
            });
        }
      }
    }

    [Test]
    public void RangeConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Review review;
        using (var transaction = session.OpenTransaction()) {
          review = session.Query.All<model1.Review>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              review.Rating = 6;
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateRangeConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Review review;
        using (var transaction = session.OpenTransaction()) {
          review = session.Query.All<model2.Review>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              review.Rating = 6;
            });
        }
      }
    }

    [Test]
    public void LengthAndRegexConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model1.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            ()=> {
              customer.Passport.Series = "6743";
              customer.Passport.Number = "868764";
              session.Validate();
            });

        }
      }
    }

    [Test]
    public void ImmediateLengthAndRegexConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model2.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            ()=> {
              customer.Passport.Series = "7643";
              customer.Passport.Number = "797834";
            });
        }
      }
    }

    [Test]
    public void LengthConstraintInStructureOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model1.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            ()=> {
              customer.Passport.Series = "674";
              customer.Passport.Number = "868764";
              session.Validate();
            });

        }
      }
    }

    [Test]
    public void ImmediateLengthConstraintInStructureOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model2.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            ()=> {
              customer.Passport.Series = "764";
              customer.Passport.Number = "797834";
            });
        }
      }
    }

    [Test]
    public void LengthConstraingOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Review review;
        using (var transaction = session.OpenTransaction()) {
          review = session.Query.All<model1.Review>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              review.Text = "5";
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateLengthConstraingOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Review review;
        using (var transaction = session.OpenTransaction()) {
          review = session.Query.All<model2.Review>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              review.Text = "5";
            });
        }
      }
    }

    [Test]
    public void RegexConstraintInStructureOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model1.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              customer.Passport.Series = "6748";
              customer.Passport.Number = "8687";
              session.Validate();
            });
          Assert.Throws<ValidationFailedException>(
            () => {
              customer.Passport.Series = "6748";
              customer.Passport.Number = "8687av";
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateRegexConstraintInStructureOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model2.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              customer.Passport.Series = "7648";
              customer.Passport.Number = "7978";
            });
          Assert.Throws<ArgumentException>(
            () => {
              customer.Passport.Series = "7648";
              customer.Passport.Number = "7978av";
            });
        }
      }
    }

    [Test]
    public void RegexConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model1.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              customer.Phone = "88dfg78dfg";
              session.Validate();
            });

        }
      }
    }

    [Test]
    public void ImmediateRegexConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model2.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              customer.Phone = "88dfg78dfg";
            });
        }
      }
    }
    
    [Test]
    public  void EmailConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model1.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          foreach (var email in validEmails) {
            Assert.DoesNotThrow(
              () => {
                customer.Email = email;
                session.Validate();
              });
          }
        }
      }
    }

    [Test]
    public  void ImmediateEmailConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model2.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          foreach (var email in validEmails) {
            Assert.DoesNotThrow(
              () => {
                customer.Email = email;
              });
          }
        }
      }
    }

    [Test]
    public void EmailConstraintOnNotValidDataChangingData()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model1.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          foreach (var email in notValidEmails) {
            Assert.Throws<ValidationFailedException>(
              () => {
                customer.Email = email;
                session.Validate();
              });
          }
        }
      }
    }

    [Test]
    public void ImmediateEmailConstraintOnNotValidDataChangingData()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model2.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          foreach (var email in notValidEmails) {
            Assert.Throws<ArgumentException>(
              () => {
                customer.Email = email;
              });
          }
        }
      }
    }

    [Test]
    public void PastConstraintInStructureOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model1.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              customer.Passport.DistributeDate = DateTime.Now;
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediatePastConstraintInStructureOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model2.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              customer.Passport.DistributeDate = DateTime.Now;
            });
        }
      }
    }

    [Test]
    public void PastConstraintInStructureOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model1.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              customer.Passport.DistributeDate = DateTime.Now.AddSeconds(2);
              session.Validate();
            });
          
        }
      }
    }

    [Test]
    public void ImmediatePastConstraintInStructureOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Customer customer;
        using (var transaction = session.OpenTransaction()) {
          customer = session.Query.All<model2.Customer>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              customer.Passport.DistributeDate = DateTime.Now.AddSeconds(2);
            });
        }
      }
    }

    [Test]
    public void PastConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Order order;
        using (var transaction = session.OpenTransaction()) {
          order = session.Query.All<model1.Order>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              order.Date = DateTime.Now;
              session.Validate();
            });
          
        }
      }
    }

    [Test]
    public void ImmediatePastConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Order order;
        using (var transaction = session.OpenTransaction()) {
          order = session.Query.All<model2.Order>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              order.Date = DateTime.Now;
            });
        }
      }
    }

    [Test]
    public void PastConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Order order;
        using (var transaction = session.OpenTransaction()) {
          order = session.Query.All<model1.Order>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              order.Date = DateTime.Now.AddSeconds(2);
              session.Validate();
            });
          
        }
      }
    }

    [Test]
    public void ImmediatePastConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Order order;
        using (var transaction = session.OpenTransaction()) {
          order = session.Query.All<model2.Order>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              order.Date = DateTime.Now.AddSeconds(2);
            });
        }
      }
    }

    [Test]
    public void FutureConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Order order;
        using (var transaction = session.OpenTransaction()) {
          order = session.Query.All<model1.Order>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              order.DeliveryDate = DateTime.Now.AddDays(2);
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateFutureConstraintOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Order order;
        using (var transaction = session.OpenTransaction()) {
          order = session.Query.All<model2.Order>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              order.DeliveryDate = DateTime.Now.AddDays(2);
            });
        }
      }
    }

    [Test]
    public void FutureConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.Order order;
        using (var transaction = session.OpenTransaction()) {
          order = session.Query.All<model1.Order>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              order.DeliveryDate = DateTime.Now;
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateFutureConstraintOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.Order order;
        using (var transaction = session.OpenTransaction()) {
          order = session.Query.All<model2.Order>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              order.DeliveryDate = DateTime.Now;
            });
        }
      }
    }

    [Test]
    public void InheritedFieldsValidationOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.ChildClass childClass;
        using (var transaction = session.OpenTransaction()) {
          childClass = session.Query.All<model1.ChildClass>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              childClass.NotEmptyString = "d";
              childClass.NotNullOrEmptyString = "sdf";
              childClass.NotNullString = "skdjfh";
              childClass.FutureDateTime = DateTime.Now.AddDays(2);
              childClass.IntValueInRange = 5;
              childClass.PastDateTime = DateTime.Now;
              childClass.StringForRegexValidation = "%%%%";
              childClass.OverridedField = "sdfd";
              childClass.PropertyBecomesField = "sdfljdkf";
              childClass.LimitedLengthString = "uuuuu";
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateInheritedFieldsValidationOnValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.ChildClass childClass;
        using (var transaction = session.OpenTransaction()) {
          childClass = session.Query.All<model2.ChildClass>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              childClass.NotEmptyString = "d";
              childClass.NotNullOrEmptyString = "sdf";
              childClass.NotNullString = "skdjfh";
              childClass.FutureDateTime = DateTime.Now.AddDays(2);
              childClass.IntValueInRange = 5;
              childClass.PastDateTime = DateTime.Now;
              childClass.StringForRegexValidation = "%%%%";
              childClass.OverridedField = "sdfd";
              childClass.PropertyBecomesField = "sdfljdkf";
              childClass.LimitedLengthString = "uuuuu";
            });
        }
      }
    }

    [Test]
    public void InheritedFieldsConstraintsOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.ChildClass childClass;
        using (var transaction = session.OpenTransaction()) {
          childClass = session.Query.All<model1.ChildClass>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.NotEmptyString = "";
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.NotNullOrEmptyString = "";
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.NotNullOrEmptyString = null;
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.NotNullString = null;
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.FutureDateTime = DateTime.Now;
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.IntValueInRange = 11;
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.PastDateTime = DateTime.Now.AddSeconds(2);
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.StringForRegexValidation = "%&%%";
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.LimitedLengthString = "uuuuujhgdfhjdfhgjfdhgfjdhghfg";
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateInheritedFieldsConstraintsOnNotValidDataChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.ChildClass childClass;
        using (var transaction = session.OpenTransaction()) {
          childClass = session.Query.All<model2.ChildClass>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.NotEmptyString = "";
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.NotNullOrEmptyString = "";
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.NotNullOrEmptyString = null;
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.NotNullString = null;
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.FutureDateTime = DateTime.Now;
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.IntValueInRange = 11;
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.PastDateTime = DateTime.Now.AddSeconds(2);
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.StringForRegexValidation = "%&%%";
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.LimitedLengthString = "uuuuujhgdfhjdfhgjfdhgfjdhghfg";
            });
        }
      }
    }

    [Test]
    public void OverridedFieldConsraintChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.ChildClass childClass;
        model1.SecondChildClass secondChildClass;
        using (var transaction = session.OpenTransaction()) {
          childClass = session.Query.All<model1.ChildClass>().First();
          secondChildClass = session.Query.All<model1.SecondChildClass>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ValidationFailedException>(
            () => {
              childClass.OverridedField = "";
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              secondChildClass.OverridedField = "";
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateOverridedFieldConsraintChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.ChildClass childClass;
        model2.SecondChildClass secondChildClass;
        using (var transaction = session.OpenTransaction()) {
          childClass = session.Query.All<model2.ChildClass>().First();
          secondChildClass = session.Query.All<model2.SecondChildClass>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.Throws<ArgumentException>(
            () => {
              childClass.OverridedField = "";
            });
        }
        using (var transaction = session.OpenTransaction())
        {
          Assert.DoesNotThrow(
            () => {
              secondChildClass.OverridedField = "";
            });
        }
      }
    }

    [Test]
    public void InteritedPropertyBecomesFieldChangingTest()
    {
      using (var domain = BuildDomain(false))
      using (var session = domain.OpenSession()) {
        model1.ChildClass childClass;
        model1.SecondChildClass secondChildClass;
        using (var transaction = session.OpenTransaction()) {
          childClass = session.Query.All<model1.ChildClass>().First();
          secondChildClass = session.Query.All<model1.SecondChildClass>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              childClass.PropertyBecomesField = null;
              session.Validate();
            });
        }
        using (var transaction = session.OpenTransaction()) {
          Assert.DoesNotThrow(
            () => {
              secondChildClass.PropertyBecomesField = null;
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ImmediateInteritedPropertyBecomesFieldChangingTest()
    {
      using (var domain = BuildDomain(true))
      using (var session = domain.OpenSession()) {
        model2.ChildClass childClass;
        model2.SecondChildClass secondChildClass;
        using (var transaction = session.OpenTransaction()) {
          childClass = session.Query.All<model2.ChildClass>().First();
          secondChildClass = session.Query.All<model2.SecondChildClass>().First();
          transaction.Complete();
        }
        using (var transaction = session.OpenTransaction())
        {
          Assert.DoesNotThrow(
            () => {
              childClass.PropertyBecomesField = null;
            });
        }
        using (var transaction = session.OpenTransaction())
        {
          Assert.DoesNotThrow(
            () => {
              secondChildClass.PropertyBecomesField = null;
            });
        }
      }
    }
    #endregion
  }
}
