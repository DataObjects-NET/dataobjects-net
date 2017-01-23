using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Linq;
using Xtensive.Orm.Tests.Model.Association;
using Xtensive.Orm.Validation;
using model1 = Xtensive.Orm.Tests.Storage.SkippingValidationOnCommitTestModel.NoConstraintsModel;
using model2 = Xtensive.Orm.Tests.Storage.SkippingValidationOnCommitTestModel.ImmediateConstrainstModel;
using model3 = Xtensive.Orm.Tests.Storage.SkippingValidationOnCommitTestModel.ImmediateAndSkippedContstraintsModel;
using model4 = Xtensive.Orm.Tests.Storage.SkippingValidationOnCommitTestModel.NonImmediateConstraintsModel;
using model5 = Xtensive.Orm.Tests.Storage.SkippingValidationOnCommitTestModel.NonImmediateAndSkippedConstrainstsModel;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class SkippingValidationOnCommitTest
  {
    [SetUp]
    public void BeforeEachTestSetup()
    {
      try {
        using (var domain = Domain.Build(GetInitialDomainConfiguration())) {
          PopulateData(domain);
        }
      }
      catch (IgnoreException) {
        throw;
      }
      catch (Exception e) {
        Debug.WriteLine(e);
        throw;
      }
    }

    private DomainConfiguration GetInitialDomainConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (model1.BaseEntity).Assembly, typeof (model1.BaseEntity).Namespace);
      return configuration;
    }

    private DomainConfiguration GetDomainConfiguration(Type typeOfTargetNamespace)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      configuration.Types.Register(typeOfTargetNamespace.Assembly, typeOfTargetNamespace.Namespace);
      return configuration;
    }

    private void PopulateData(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new model1.EmailTestEntity() {NeverValidatedField = "Email", EmailField = "not email"};
        new model1.FutureTestEntity() {NeverValidatedField = "Future", DateField = DateTime.Now.AddMonths(-1)};
        new model1.LenghtTestEntity() {NeverValidatedField = "Length", StringField = "too short"};
        new model1.NotEmptyTestEntity() {NeverValidatedField = "NotEmpty", StringField = string.Empty};
        new model1.NotNullOrEmptyTestEntity() {NeverValidatedField = "NotNullOrEmpty", StringField = null};
        new model1.NotNullTestEntity() {NeverValidatedField = "NotNull", StringField = null};
        new model1.PastTestEntity() {NeverValidatedField = "Past", DateField = DateTime.Now.AddMonths(1)};
        new model1.RangeTestEntity() {NeverValidatedField = "Range", LongField = -100};
        new model1.RegexTestEntity() {NeverValidatedField = "Regex", StringField = "abcd"};

        transaction.Complete();
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void EmailConstraintImmediateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model2.EmailTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(()=>entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void EmailConstraintImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model3.EmailTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void EmailConstraintNotImmidiateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model4.EmailTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void EmailConstraintNotImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model5.EmailTestEntity))))
      {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          new model5.EmailTestEntity() {NeverValidatedField = "", EmailField = "not email"};//no exception
          transaction.Complete();
          // no exception
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void FutureConstraintImmediateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model2.FutureTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(()=>entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void FutureConstraintImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model3.FutureTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void FutureConstraintNotImmidiateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model4.FutureTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void FutureConstraintNotImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model5.FutureTestEntity))))
      {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.FutureTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          new model5.FutureTestEntity() {NeverValidatedField = "", DateField = DateTime.Now.AddMonths(-1)};//no exception
          transaction.Complete();
          // no exception
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void LenghtConstraintImmediateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model2.EmailTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(()=>entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void LenghtConstraintImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model3.LenghtTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void LenghtConstraintNotImmidiateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model4.LenghtTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void LenghtConstraintNotImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model5.LenghtTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.LenghtTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          new model5.LenghtTestEntity() {NeverValidatedField = "", StringField = "not email"};//no exception
          transaction.Complete();
          // no exception
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void NotEmptyConstraintImmediateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model2.NotEmptyTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(()=>entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void NotEmptyConstraintImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model3.NotEmptyTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void NotEmptyConstraintNotImmidiateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model4.NotEmptyTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.EmailTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void NotEmptyConstraintNotImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model5.NotEmptyTestEntity))))
      {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          new model5.NotEmptyTestEntity() {NeverValidatedField = "NotEmpty", StringField = ""};//no exception
          transaction.Complete();
          // no exception
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void NotNullConstraintImmediateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model2.NotNullTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(()=>entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void NotNullConstraintImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model3.NotNullTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void NotNullConstraintNotImmidiateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model4.NotNullTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void NotNullConstraintNotImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model5.NotNullTestEntity))))
      {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotNullTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          new model5.NotNullTestEntity() {NeverValidatedField = "NotNull", StringField = null};//no exception
          transaction.Complete();
          // no exception
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void NotNullOrEmptyConstraintImmediateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model2.NotNullOrEmptyTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(()=>entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void NotNullOrEmptyConstraintImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model3.NotNullOrEmptyTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void NotNullOrEmptyConstraintNotImmidiateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model4.NotNullOrEmptyTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void NotNullOrEmptyConstraintNotImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model5.NotNullOrEmptyTestEntity))))
      {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.NotNullOrEmptyTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          new model5.NotNullOrEmptyTestEntity() {NeverValidatedField = "NotNullOrEmpty", StringField = null};//no exception
          transaction.Complete();
          // no exception
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void PastConstraintImmediateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model2.PastTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(()=>entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void PastConstraintImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model3.EmailTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void PastConstraintNotImmidiateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model4.PastTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void PastConstraintNotImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model5.PastTestEntity))))
      {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.PastTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          new model5.PastTestEntity() {NeverValidatedField = "Past", DateField = DateTime.Now.AddMonths(2)};//no exception
          transaction.Complete();
          // no exception
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void RangeConstraintImmediateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model2.RangeTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(()=>entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void RangeConstraintImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model3.RangeTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void RangeConstraintNotImmidiateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model4.RangeTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void RangeConstraintNotImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model5.RangeTestEntity))))
      {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.RangeTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          new model5.RangeTestEntity() {NeverValidatedField = "Range", LongField = -200};//no exception
          transaction.Complete();
          // no exception
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void RegexConstraintImmediateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model2.RegexTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(()=>entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model2.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void RegexConstraintImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model3.RegexTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model3.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }
      }
    }

    [Test]
    [ExpectedException(typeof (ValidationFailedException))]
    public void RegexConstraintNotImmidiateNotSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model4.RegexTestEntity)))) {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model4.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //exception on commit
        }
      }
    }

    [Test]
    public void RegexConstraintNotImmediateSkippedTest()
    {
      using (var domain = Domain.Build(GetDomainConfiguration(typeof(model5.RegexTestEntity))))
      {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => entity.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          Assert.Throws<ValidationFailedException>(() => session.Validate());
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          var errors = session.ValidateAndGetErrors();
          Assert.That(errors.Count, Is.EqualTo(1));
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model5.RegexTestEntity>().First();
          entity.NeverValidatedField = entity.NeverValidatedField + "(changed)";
          transaction.Complete();
          //no exception on commit
        }

        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          new model5.RegexTestEntity() {NeverValidatedField = "Regex", StringField = "bbbbbb"};//no exception
          transaction.Complete();
          // no exception
        }
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.SkippingValidationOnCommitTestModel
{
  namespace NoConstraintsModel
  {
    public abstract class BaseEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public string NeverValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailTestEntity : BaseEntity
    {
      [Field]
      public string EmailField { get; set; }
    }

    [HierarchyRoot]
    public class FutureTestEntity : BaseEntity
    {
      [Field]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class LenghtTestEntity : BaseEntity
    {
      [Field]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : BaseEntity
    {
      [Field]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : BaseEntity
    {
      [Field]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : BaseEntity
    {
      [Field]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class PastTestEntity : BaseEntity
    {
      [Field]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class RangeTestEntity : BaseEntity
    {
      [Field]
      public long LongField { get; set; }
    }

    [HierarchyRoot]
    public class RegexTestEntity : BaseEntity
    {
      [Field]
      public string StringField { get; set; }
    } 
  }

  namespace ImmediateConstrainstModel
  {
    public abstract class BaseEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public string NeverValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailTestEntity : BaseEntity
    {
      [Field]
      [EmailConstraint(IsImmediate = true)]
      public string EmailField { get; set; }
    }

    [HierarchyRoot]
    public class FutureTestEntity : BaseEntity
    {
      [Field]
      [FutureConstraint(IsImmediate = true)]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class LenghtTestEntity : BaseEntity
    {
      [Field]
      [LengthConstraint(IsImmediate = true, Min = 16)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : BaseEntity
    {
      [Field]
      [NotEmptyConstraint(IsImmediate = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : BaseEntity
    {
      [Field]
      [NotNullConstraint(IsImmediate = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : BaseEntity
    {
      [Field]
      [NotNullOrEmptyConstraint(IsImmediate = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class PastTestEntity : BaseEntity
    {
      [Field]
      [PastConstraint(IsImmediate = true)]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class RangeTestEntity : BaseEntity
    {
      [Field]
      [RangeConstraint(IsImmediate = true, Min = 10, Max = 20)]
      public long LongField { get; set; }
    }

    [HierarchyRoot]
    public class RegexTestEntity : BaseEntity
    {
      [Field]
      [RegexConstraint("\b[A-Z][A-Z0-9]+\b", IsImmediate = true)]
      public string StringField { get; set; }
    }
  }

  namespace ImmediateAndSkippedContstraintsModel
  {
    public abstract class BaseEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public string NeverValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailTestEntity : BaseEntity
    {
      [Field]
      [EmailConstraint(IsImmediate = true, SkipOnTransactionComitting = true)]
      public string EmailField { get; set; }
    }

    [HierarchyRoot]
    public class FutureTestEntity : BaseEntity
    {
      [Field]
      [FutureConstraint(IsImmediate = true, SkipOnTransactionComitting = true)]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class LenghtTestEntity : BaseEntity
    {
      [Field]
      [LengthConstraint(IsImmediate = true, SkipOnTransactionComitting = true, Min = 16)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : BaseEntity
    {
      [Field]
      [NotEmptyConstraint(IsImmediate = true, SkipOnTransactionComitting = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : BaseEntity
    {
      [Field]
      [NotNullConstraint(IsImmediate = true, SkipOnTransactionComitting = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : BaseEntity
    {
      [Field]
      [NotNullOrEmptyConstraint(IsImmediate = true, SkipOnTransactionComitting = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class PastTestEntity : BaseEntity
    {
      [Field]
      [PastConstraint(IsImmediate = true, SkipOnTransactionComitting = true)]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class RangeTestEntity : BaseEntity
    {
      [Field]
      [RangeConstraint(IsImmediate = true, SkipOnTransactionComitting = true, Min = 10, Max = 20)]
      public long LongField { get; set; }
    }

    [HierarchyRoot]
    public class RegexTestEntity : BaseEntity
    {
      [Field]
      [RegexConstraint("\b[A-Z][A-Z0-9]+\b", IsImmediate = true, SkipOnTransactionComitting = true)]
      public string StringField { get; set; }
    }
  }

  namespace NonImmediateConstraintsModel
  {
    public abstract class BaseEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public string NeverValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailTestEntity : BaseEntity
    {
      [Field]
      [EmailConstraint(IsImmediate = false)]
      public string EmailField { get; set; }
    }

    [HierarchyRoot]
    public class FutureTestEntity : BaseEntity
    {
      [Field]
      [FutureConstraint(IsImmediate = false)]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class LenghtTestEntity : BaseEntity
    {
      [Field]
      [LengthConstraint(IsImmediate = false, Min = 16)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : BaseEntity
    {
      [Field]
      [NotEmptyConstraint(IsImmediate = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : BaseEntity
    {
      [Field]
      [NotNullConstraint(IsImmediate = false)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : BaseEntity
    {
      [Field]
      [NotNullOrEmptyConstraint(IsImmediate = false)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class PastTestEntity : BaseEntity
    {
      [Field]
      [PastConstraint(IsImmediate = false)]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class RangeTestEntity : BaseEntity
    {
      [Field]
      [RangeConstraint(IsImmediate = false, Min = 10, Max = 20)]
      public long LongField { get; set; }
    }

    [HierarchyRoot]
    public class RegexTestEntity : BaseEntity
    {
      [Field]
      [RegexConstraint("\b[A-Z][A-Z0-9]+\b", IsImmediate = false)]
      public string StringField { get; set; }
    }
  }

  namespace NonImmediateAndSkippedConstrainstsModel
  {
    public abstract class BaseEntity : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public string NeverValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailTestEntity : BaseEntity
    {
      [Field]
      [EmailConstraint(IsImmediate = false, SkipOnTransactionComitting = true)]
      public string EmailField { get; set; }
    }

    [HierarchyRoot]
    public class FutureTestEntity : BaseEntity
    {
      [Field]
      [FutureConstraint(IsImmediate = false, SkipOnTransactionComitting = true)]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class LenghtTestEntity : BaseEntity
    {
      [Field]
      [LengthConstraint(IsImmediate = false, SkipOnTransactionComitting = true, Min = 16)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : BaseEntity
    {
      [Field]
      [NotEmptyConstraint(IsImmediate = false, SkipOnTransactionComitting = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : BaseEntity
    {
      [Field]
      [NotNullConstraint(IsImmediate = false, SkipOnTransactionComitting = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : BaseEntity
    {
      [Field]
      [NotNullOrEmptyConstraint(IsImmediate = false, SkipOnTransactionComitting = true)]
      public string StringField { get; set; }
    }

    [HierarchyRoot]
    public class PastTestEntity : BaseEntity
    {
      [Field]
      [PastConstraint(IsImmediate = false, SkipOnTransactionComitting = true)]
      public DateTime DateField { get; set; }
    }

    [HierarchyRoot]
    public class RangeTestEntity : BaseEntity
    {
      [Field]
      [RangeConstraint(IsImmediate = false, SkipOnTransactionComitting = true, Min = 10, Max = 20)]
      public long LongField { get; set; }
    }

    [HierarchyRoot]
    public class RegexTestEntity : BaseEntity
    {
      [Field]
      [RegexConstraint("\b[A-Z][A-Z0-9]+\b", IsImmediate = false, SkipOnTransactionComitting = true)]
      public string StringField { get; set; }
    }
  }
}
