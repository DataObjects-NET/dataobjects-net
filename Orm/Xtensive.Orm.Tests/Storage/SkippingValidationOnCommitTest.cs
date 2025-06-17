// Copyright (C) 2017-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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
using Xtensive.Orm.Tests.Storage.SkippingValidationOnCommitTestModel;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class SkippingValidationOnCommitTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.RegisterCaching(typeof(BaseEntity).Assembly, typeof(BaseEntity).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      var sessionConfiguration = new SessionConfiguration(SessionOptions.AutoSaveChanges);
      using (var session = Domain.OpenSession(sessionConfiguration))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {

        _ = new EmailTestEntity1() { NeverValidatedField = "Email", EmailField = "not email" };
        _ = new EmailTestEntity2() { NeverValidatedField = "Email", EmailField = "not email" };
        _ = new EmailTestEntity3() { NeverValidatedField = "Email", EmailField = "not email" };
        _ = new EmailTestEntity4() { NeverValidatedField = "Email", EmailField = "not email" };

        _ = new FutureTestEntity1() { NeverValidatedField = "Future", DateField = DateTime.Now.AddMonths(-1) };
        _ = new FutureTestEntity2() { NeverValidatedField = "Future", DateField = DateTime.Now.AddMonths(-1) };
        _ = new FutureTestEntity3() { NeverValidatedField = "Future", DateField = DateTime.Now.AddMonths(-1) };
        _ = new FutureTestEntity4() { NeverValidatedField = "Future", DateField = DateTime.Now.AddMonths(-1) };

        _ = new LenghtTestEntity1() { NeverValidatedField = "Length", StringField = "too short" };
        _ = new LenghtTestEntity2() { NeverValidatedField = "Length", StringField = "too short" };
        _ = new LenghtTestEntity3() { NeverValidatedField = "Length", StringField = "too short" };
        _ = new LenghtTestEntity4() { NeverValidatedField = "Length", StringField = "too short" };

        _ = new NotEmptyTestEntity1() { NeverValidatedField = "NotEmpty", StringField = string.Empty };
        _ = new NotEmptyTestEntity2() { NeverValidatedField = "NotEmpty", StringField = string.Empty };
        _ = new NotEmptyTestEntity3() { NeverValidatedField = "NotEmpty", StringField = string.Empty };
        _ = new NotEmptyTestEntity4() { NeverValidatedField = "NotEmpty", StringField = string.Empty };

        _ = new NotNullOrEmptyTestEntity1() { NeverValidatedField = "NotNullOrEmpty", StringField = null };
        _ = new NotNullOrEmptyTestEntity2() { NeverValidatedField = "NotNullOrEmpty", StringField = null };
        _ = new NotNullOrEmptyTestEntity3() { NeverValidatedField = "NotNullOrEmpty", StringField = null };
        _ = new NotNullOrEmptyTestEntity4() { NeverValidatedField = "NotNullOrEmpty", StringField = null };

        _ = new NotNullTestEntity1() { NeverValidatedField = "NotNull", StringField = null };
        _ = new NotNullTestEntity2() { NeverValidatedField = "NotNull", StringField = null };
        _ = new NotNullTestEntity3() { NeverValidatedField = "NotNull", StringField = null };
        _ = new NotNullTestEntity4() { NeverValidatedField = "NotNull", StringField = null };

        _ = new PastTestEntity1() { NeverValidatedField = "Past", DateField = DateTime.Now.AddMonths(1) };
        _ = new PastTestEntity2() { NeverValidatedField = "Past", DateField = DateTime.Now.AddMonths(1) };
        _ = new PastTestEntity3() { NeverValidatedField = "Past", DateField = DateTime.Now.AddMonths(1) };
        _ = new PastTestEntity4() { NeverValidatedField = "Past", DateField = DateTime.Now.AddMonths(1) };

        _ = new RangeTestEntity1() { NeverValidatedField = "Range", LongField = -100 };
        _ = new RangeTestEntity2() { NeverValidatedField = "Range", LongField = -100 };
        _ = new RangeTestEntity3() { NeverValidatedField = "Range", LongField = -100 };
        _ = new RangeTestEntity4() { NeverValidatedField = "Range", LongField = -100 };

        _ = new RegexTestEntity1() { NeverValidatedField = "Regex", StringField = "abcd" };
        _ = new RegexTestEntity2() { NeverValidatedField = "Regex", StringField = "abcd" };
        _ = new RegexTestEntity3() { NeverValidatedField = "Regex", StringField = "abcd" };
        _ = new RegexTestEntity4() { NeverValidatedField = "Regex", StringField = "abcd" };

        transaction.Complete();
      }
    }

    [Test]
    public void EmailConstraintImmediateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<EmailTestEntity1>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void EmailConstraintImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }
    }

    [Test]
    public void EmailConstraintNotImmidiateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<EmailTestEntity3>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void EmailConstraintNotImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new EmailTestEntity4() { NeverValidatedField = "", EmailField = "not email" };//no exception
        transaction.Complete();
        // no exception
      }
    }

    [Test]
    public void FutureConstraintImmediateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<FutureTestEntity1>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void FutureConstraintImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }
    }

    [Test]
    public void FutureConstraintNotImmidiateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<FutureTestEntity3>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void FutureConstraintNotImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<FutureTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new FutureTestEntity4() { NeverValidatedField = "", DateField = DateTime.Now.AddMonths(-1) };//no exception
        transaction.Complete();
        // no exception
      }
    }

    [Test]
    public void LenghtConstraintImmediateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<LenghtTestEntity1>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void LenghtConstraintImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }
    }

    [Test]
    public void LenghtConstraintNotImmidiateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<LenghtTestEntity3>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void LenghtConstraintNotImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<LenghtTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new LenghtTestEntity4() { NeverValidatedField = "", StringField = "not email" };//no exception
        transaction.Complete();
        // no exception
      }
    }

    [Test]
    public void NotEmptyConstraintImmediateNotSkippedTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.TreatEmptyStringAsNull);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<NotEmptyTestEntity1>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void NotEmptyConstraintImmediateSkippedTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.TreatEmptyStringAsNull);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }
    }

    [Test]
    public void NotEmptyConstraintNotImmidiateNotSkippedTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.TreatEmptyStringAsNull);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<EmailTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<NotEmptyTestEntity3>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void NotEmptyConstraintNotImmediateSkippedTest()
    {
      Require.AllFeaturesNotSupported(ProviderFeatures.TreatEmptyStringAsNull);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotEmptyTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new NotEmptyTestEntity4() { NeverValidatedField = "NotEmpty", StringField = "" };//no exception
        transaction.Complete();
        // no exception
      }
    }

    [Test]
    public void NotNullConstraintImmediateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<NotNullTestEntity1>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void NotNullConstraintImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }
    }

    [Test]
    public void NotNullConstraintNotImmidiateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<NotNullTestEntity3>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void NotNullConstraintNotImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new NotNullTestEntity4() { NeverValidatedField = "NotNull", StringField = null };//no exception
        transaction.Complete();
        // no exception
      }
    }

    [Test]
    public void NotNullOrEmptyConstraintImmediateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<NotNullOrEmptyTestEntity1>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void NotNullOrEmptyConstraintImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }
    }

    [Test]
    public void NotNullOrEmptyConstraintNotImmidiateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<NotNullOrEmptyTestEntity3>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void NotNullOrEmptyConstraintNotImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<NotNullOrEmptyTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new NotNullOrEmptyTestEntity4() { NeverValidatedField = "NotNullOrEmpty", StringField = null };//no exception
        transaction.Complete();
        // no exception
      }
    }

    [Test]
    public void PastConstraintImmediateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<PastTestEntity1>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void PastConstraintImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }
    }

    [Test]
    public void PastConstraintNotImmidiateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<PastTestEntity3>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void PastConstraintNotImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<PastTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new PastTestEntity4() { NeverValidatedField = "Past", DateField = DateTime.Now.AddMonths(2) };//no exception
        transaction.Complete();
        // no exception
      }
    }

    [Test]
    public void RangeConstraintImmediateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<RangeTestEntity1>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void RangeConstraintImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }
    }

    [Test]
    public void RangeConstraintNotImmidiateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<RangeTestEntity3>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void RangeConstraintNotImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RangeTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new RangeTestEntity4() { NeverValidatedField = "Range", LongField = -200 };//no exception
        transaction.Complete();
        // no exception
      }
    }

    [Test]
    public void RegexConstraintImmediateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity1>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }
      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<RegexTestEntity1>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void RegexConstraintImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity2>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }
    }

    [Test]
    public void RegexConstraintNotImmidiateNotSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity3>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      _ = Assert.Throws<ValidationFailedException>(() => {
        using (var session = Domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<RegexTestEntity3>().First();
          entity.NeverValidatedField += "(changed)";
          transaction.Complete();
          //exception on commit
        }
      });
    }

    [Test]
    public void RegexConstraintNotImmediateSkippedTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => entity.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        _ = Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entity = session.Query.All<RegexTestEntity4>().First();
        entity.NeverValidatedField += "(changed)";
        transaction.Complete();
        //no exception on commit
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        _ = new RegexTestEntity4() { NeverValidatedField = "Regex", StringField = "bbbbbb" };//no exception
        transaction.Complete();
        // no exception
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Storage.SkippingValidationOnCommitTestModel
{
  public abstract class BaseEntity : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string NeverValidatedField { get; set; }
  }

  #region ImmediateConstraints

  [HierarchyRoot]
  public class EmailTestEntity1 : BaseEntity
  {
    [Field]
    [EmailConstraint(IsImmediate = true)]
    public string EmailField { get; set; }
  }

  [HierarchyRoot]
  public class FutureTestEntity1 : BaseEntity
  {
    [Field]
    [FutureConstraint(IsImmediate = true)]
    public DateTime DateField { get; set; }
  }

  [HierarchyRoot]
  public class LenghtTestEntity1 : BaseEntity
  {
    [Field]
    [LengthConstraint(IsImmediate = true, Min = 16)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotEmptyTestEntity1 : BaseEntity
  {
    [Field]
    [NotEmptyConstraint(IsImmediate = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotNullTestEntity1 : BaseEntity
  {
    [Field]
    [NotNullConstraint(IsImmediate = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotNullOrEmptyTestEntity1 : BaseEntity
  {
    [Field]
    [NotNullOrEmptyConstraint(IsImmediate = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class PastTestEntity1 : BaseEntity
  {
    [Field]
    [PastConstraint(IsImmediate = true)]
    public DateTime DateField { get; set; }
  }

  [HierarchyRoot]
  public class RangeTestEntity1 : BaseEntity
  {
    [Field]
    [RangeConstraint(IsImmediate = true, Min = 10, Max = 20)]
    public long LongField { get; set; }
  }

  [HierarchyRoot]
  public class RegexTestEntity1 : BaseEntity
  {
    [Field]
    [RegexConstraint("\b[A-Z][A-Z0-9]+\b", IsImmediate = true)]
    public string StringField { get; set; }
  }

  #endregion

  #region ImmediateAndSkippedContstraints

  [HierarchyRoot]
  public class EmailTestEntity2 : BaseEntity
  {
    [Field]
    [EmailConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
    public string EmailField { get; set; }
  }

  [HierarchyRoot]
  public class FutureTestEntity2 : BaseEntity
  {
    [Field]
    [FutureConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
    public DateTime DateField { get; set; }
  }

  [HierarchyRoot]
  public class LenghtTestEntity2 : BaseEntity
  {
    [Field]
    [LengthConstraint(IsImmediate = true, SkipOnTransactionCommit = true, Min = 16)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotEmptyTestEntity2 : BaseEntity
  {
    [Field]
    [NotEmptyConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotNullTestEntity2 : BaseEntity
  {
    [Field]
    [NotNullConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotNullOrEmptyTestEntity2 : BaseEntity
  {
    [Field]
    [NotNullOrEmptyConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class PastTestEntity2 : BaseEntity
  {
    [Field]
    [PastConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
    public DateTime DateField { get; set; }
  }

  [HierarchyRoot]
  public class RangeTestEntity2 : BaseEntity
  {
    [Field]
    [RangeConstraint(IsImmediate = true, SkipOnTransactionCommit = true, Min = 10, Max = 20)]
    public long LongField { get; set; }
  }

  [HierarchyRoot]
  public class RegexTestEntity2 : BaseEntity
  {
    [Field]
    [RegexConstraint("\b[A-Z][A-Z0-9]+\b", IsImmediate = true, SkipOnTransactionCommit = true)]
    public string StringField { get; set; }
  }

  #endregion

  #region NonImmediateConstraints

  [HierarchyRoot]
  public class EmailTestEntity3 : BaseEntity
  {
    [Field]
    [EmailConstraint(IsImmediate = false)]
    public string EmailField { get; set; }
  }

  [HierarchyRoot]
  public class FutureTestEntity3 : BaseEntity
  {
    [Field]
    [FutureConstraint(IsImmediate = false)]
    public DateTime DateField { get; set; }
  }

  [HierarchyRoot]
  public class LenghtTestEntity3 : BaseEntity
  {
    [Field]
    [LengthConstraint(IsImmediate = false, Min = 16)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotEmptyTestEntity3 : BaseEntity
  {
    [Field]
    [NotEmptyConstraint(IsImmediate = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotNullTestEntity3 : BaseEntity
  {
    [Field]
    [NotNullConstraint(IsImmediate = false)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotNullOrEmptyTestEntity3 : BaseEntity
  {
    [Field]
    [NotNullOrEmptyConstraint(IsImmediate = false)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class PastTestEntity3 : BaseEntity
  {
    [Field]
    [PastConstraint(IsImmediate = false)]
    public DateTime DateField { get; set; }
  }

  [HierarchyRoot]
  public class RangeTestEntity3 : BaseEntity
  {
    [Field]
    [RangeConstraint(IsImmediate = false, Min = 10, Max = 20)]
    public long LongField { get; set; }
  }

  [HierarchyRoot]
  public class RegexTestEntity3 : BaseEntity
  {
    [Field]
    [RegexConstraint("\b[A-Z][A-Z0-9]+\b", IsImmediate = false)]
    public string StringField { get; set; }
  }

  #endregion

  #region NonImmediateAndSkippedConstrainsts

  [HierarchyRoot]
  public class EmailTestEntity4 : BaseEntity
  {
    [Field]
    [EmailConstraint(IsImmediate = false, SkipOnTransactionCommit = true)]
    public string EmailField { get; set; }
  }

  [HierarchyRoot]
  public class FutureTestEntity4 : BaseEntity
  {
    [Field]
    [FutureConstraint(IsImmediate = false, SkipOnTransactionCommit = true)]
    public DateTime DateField { get; set; }
  }

  [HierarchyRoot]
  public class LenghtTestEntity4 : BaseEntity
  {
    [Field]
    [LengthConstraint(IsImmediate = false, SkipOnTransactionCommit = true, Min = 16)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotEmptyTestEntity4 : BaseEntity
  {
    [Field]
    [NotEmptyConstraint(IsImmediate = false, SkipOnTransactionCommit = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotNullTestEntity4 : BaseEntity
  {
    [Field]
    [NotNullConstraint(IsImmediate = false, SkipOnTransactionCommit = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class NotNullOrEmptyTestEntity4 : BaseEntity
  {
    [Field]
    [NotNullOrEmptyConstraint(IsImmediate = false, SkipOnTransactionCommit = true)]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class PastTestEntity4 : BaseEntity
  {
    [Field]
    [PastConstraint(IsImmediate = false, SkipOnTransactionCommit = true)]
    public DateTime DateField { get; set; }
  }

  [HierarchyRoot]
  public class RangeTestEntity4 : BaseEntity
  {
    [Field]
    [RangeConstraint(IsImmediate = false, SkipOnTransactionCommit = true, Min = 10, Max = 20)]
    public long LongField { get; set; }
  }

  [HierarchyRoot]
  public class RegexTestEntity4 : BaseEntity
  {
    [Field]
    [RegexConstraint("\b[A-Z][A-Z0-9]+\b", IsImmediate = false, SkipOnTransactionCommit = true)]
    public string StringField { get; set; }
  }

  #endregion
}
