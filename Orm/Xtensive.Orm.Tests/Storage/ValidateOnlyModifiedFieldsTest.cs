// Copyright (C) 2003-2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2017.08.18

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Validation;
using model1 = Xtensive.Orm.Tests.Storage.ValidateOnlyModifiedFieldsTestModel.ValidateIfChanged;
using model2 = Xtensive.Orm.Tests.Storage.ValidateOnlyModifiedFieldsTestModel.ValidateIfChanged_SkipOnTransactionCommit;
using model3 = Xtensive.Orm.Tests.Storage.ValidateOnlyModifiedFieldsTestModel.ValidateIfChanged_IsImmediate;
using model4 = Xtensive.Orm.Tests.Storage.ValidateOnlyModifiedFieldsTestModel.ValidateIfChanged_IsImmediate_SkipOnTransactionCommit;

namespace Xtensive.Orm.Tests.Storage.ValidateOnlyModifiedFieldsTestModel
{
  namespace ValidateIfChanged
  {
    [HierarchyRoot]
    public class IncludedStructure : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public Structure1 StructureField { get; set; }
    }

    public class Structure1 : Structure
    {
      [Field]
      [LengthConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public Structure2 EnclosedStructureField { get; set; }
    }

    public class Structure2 : Structure
    {
      [Field]
      [LengthConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true)]
      public string ValidatedIfChangedField2 { get; set; }
    }

    [HierarchyRoot]
    public class StructureTestEntity : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public TestStructure StructureField { get; set; }
    }

    public class TestStructure : Structure
    {
      [Field]
      [LengthConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [LengthConstraint(Max = 10, Min = 5)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class LengthTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [LengthConstraint(Max = 20, Min = 2, ValidateOnlyIfModified = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [LengthConstraint(Max = 20, Min = 2)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotEmptyConstraint(ValidateOnlyIfModified = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotEmptyConstraint]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotNullConstraint]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotNullOrEmptyConstraint(ValidateOnlyIfModified = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotNullOrEmptyConstraint]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class PastConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [PastConstraint(ValidateOnlyIfModified = true)]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      [PastConstraint]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class FutureConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [FutureConstraint(ValidateOnlyIfModified = true)]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      [FutureConstraint]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [EmailConstraint(ValidateOnlyIfModified = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [EmailConstraint]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class RangeConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [RangeConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true)]
      public int ValidatedIfChangedField { get; set; }

      [Field]
      [RangeConstraint(Max = 10, Min = 5)]
      public int ValidatedField { get; set; }
    }


    [HierarchyRoot]
    public class RegExConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [RegexConstraint("[a-zA-Z]+", ValidateOnlyIfModified = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [RegexConstraint("[a-zA-Z]+")]
      public string ValidatedField { get; set; }
    }
  }

  namespace ValidateIfChanged_SkipOnTransactionCommit
  {
    [HierarchyRoot]
    public class IncludedStructure : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public Structure1 StructureField { get; set; }
    }

    public class Structure1 : Structure
    {
      [Field]
      [LengthConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public Structure2 EnclosedStructureField { get; set; }
    }

    public class Structure2 : Structure
    {
      [Field]
      [LengthConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField2 { get; set; }
    }

    [HierarchyRoot]
    public class StructureTestEntity : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      public TestStructure StructureField { get; set; }
    }

    public class TestStructure : Structure
    {
      [Field]
      [LengthConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [LengthConstraint(Max = 10, Min = 5, SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class LengthTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [LengthConstraint(Max = 20, Min = 2, ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [LengthConstraint(Max = 20, Min = 2, SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotEmptyConstraint(ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotEmptyConstraint(SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotNullConstraint(SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotNullOrEmptyConstraint(ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotNullOrEmptyConstraint(SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class PastConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [PastConstraint(ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      [PastConstraint(SkipOnTransactionCommit = true)]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class FutureConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [FutureConstraint(ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      [FutureConstraint(SkipOnTransactionCommit = true)]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [EmailConstraint(ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [EmailConstraint(SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }


    [HierarchyRoot]
    public class RangeConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [RangeConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public int ValidatedIfChangedField { get; set; }

      [Field]
      [RangeConstraint(Max = 10, Min = 5, SkipOnTransactionCommit = true)]
      public int ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class RegExConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [RegexConstraint("[a-zA-Z]+", ValidateOnlyIfModified = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [RegexConstraint("[a-zA-Z]+", SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }
  }

  namespace ValidateIfChanged_IsImmediate
  {
    [HierarchyRoot]
    public class IncludedStructure : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public Structure1 StructureField { get; set; }
    }

    public class Structure1 : Structure
    {
      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public Structure2 EnclosedStructureField { get; set; }
    }

    public class Structure2 : Structure
    {
      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true, IsImmediate = true)]
      public string ValidatedIfChangedField2 { get; set; }
    }

    [HierarchyRoot]
    public class StructureTestEntity : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true, IsImmediate = true)]
      public TestStructure StructureField { get; set; }
    }

    public class TestStructure : Orm.Structure
    {
      [Field]
      [LengthConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [LengthConstraint(Max = 10, Min = 5, IsImmediate = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class LengthTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [LengthConstraint(Max = 20, Min = 2, ValidateOnlyIfModified = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [LengthConstraint(Max = 20, Min = 2, IsImmediate = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotEmptyConstraint(ValidateOnlyIfModified = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotEmptyConstraint(IsImmediate = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotNullConstraint(IsImmediate = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotNullOrEmptyConstraint(ValidateOnlyIfModified = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotNullOrEmptyConstraint(IsImmediate = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class PastConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [PastConstraint(ValidateOnlyIfModified = true, IsImmediate = true)]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      [PastConstraint(IsImmediate = true)]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class FutureConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [FutureConstraint(ValidateOnlyIfModified = true, IsImmediate = true)]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      [FutureConstraint(IsImmediate = true)]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [EmailConstraint(ValidateOnlyIfModified = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [EmailConstraint(IsImmediate = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class RangeConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [RangeConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true, IsImmediate = true)]
      public int ValidatedIfChangedField { get; set; }

      [Field]
      [RangeConstraint(Max = 10, Min = 5, IsImmediate = true)]
      public int ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class RegExConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [RegexConstraint("[a-zA-Z]+", ValidateOnlyIfModified = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [RegexConstraint("[a-zA-Z]+", IsImmediate = true)]
      public string ValidatedField { get; set; }
    }
  }

  namespace ValidateIfChanged_IsImmediate_SkipOnTransactionCommit
  {
    [HierarchyRoot]
    public class IncludedStructure : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public Structure1 StructureField { get; set; }
    }

    public class Structure1 : Structure
    {
      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true, SkipOnTransactionCommit = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public Structure2 EnclosedStructureField { get; set; }
    }

    public class Structure2 : Structure
    {
      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true, SkipOnTransactionCommit = true, IsImmediate = true)]
      public string ValidatedIfChangedField2 { get; set; }
    }

    [HierarchyRoot]
    public class StructureTestEntity : Entity
    {
      [Field, Key]
      public long Id { get; set; }

      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true, SkipOnTransactionCommit = true, IsImmediate = true)]
      public TestStructure StructureField { get; set; }
    }

    public class TestStructure : Orm.Structure
    {
      [Field]
      [LengthConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true, SkipOnTransactionCommit = true, IsImmediate = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [LengthConstraint(Max = 10, Min = 5, SkipOnTransactionCommit = true, IsImmediate = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class LengthTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [LengthConstraint(Max = 20, Min = 2, ValidateOnlyIfModified = true, IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [LengthConstraint(Max = 20, Min = 2, IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotEmptyConstraint(ValidateOnlyIfModified = true, IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotEmptyConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotNullConstraint(ValidateOnlyIfModified = true, IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotNullConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [NotNullOrEmptyConstraint(ValidateOnlyIfModified = true, IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [NotNullOrEmptyConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class PastConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [PastConstraint(ValidateOnlyIfModified = true, IsImmediate = true, SkipOnTransactionCommit = true)]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      [PastConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class FutureConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [FutureConstraint(ValidateOnlyIfModified = true, IsImmediate = true, SkipOnTransactionCommit = true)]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      [FutureConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [EmailConstraint(ValidateOnlyIfModified = true, IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [EmailConstraint(IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class RangeConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [RangeConstraint(Max = 10, Min = 5, ValidateOnlyIfModified = true, IsImmediate = true, SkipOnTransactionCommit = true)]
      public int ValidatedIfChangedField { get; set; }

      [Field]
      [RangeConstraint(Max = 10, Min = 5, IsImmediate = true, SkipOnTransactionCommit = true)]
      public int ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class RegExConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      [RegexConstraint("[a-zA-Z]+", ValidateOnlyIfModified = true, IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      [RegexConstraint("[a-zA-Z]+", IsImmediate = true, SkipOnTransactionCommit = true)]
      public string ValidatedField { get; set; }
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class ValidateOnlyModifiedFieldsTest
  {
    private Domain domain;

    #region General

    [Test]
    public void PostPersistValidationTest()
    {
      PrepareDomain<model1.LengthTestEntity>(() => 
        new model1.LengthTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession()) {
          using (var transaction = session.OpenTransaction()) {
            var entity = session.Query.All<model1.LengthTestEntity>().Single();
            entity.ValidatedField = "";
            entity.ValidatedIfChangedField = "";
            session.SaveChanges();
            transaction.Complete();
          }
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession()) {
          using (var transaction = session.OpenTransaction()) {
            var entity = session.Query.All<model1.LengthTestEntity>().Single();
            entity.ValidatedField = "valid";
            entity.ValidatedIfChangedField = "valid";
            session.SaveChanges();
            transaction.Complete();
          }
        }
      });
    }

    [Test]
    public void NewEntityValidationTest()
    {
      PrepareDomain<model1.LengthTestEntity>(() =>
        new model1.LengthTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = new model1.LengthTestEntity() {
            ValidatedField = string.Empty, 
            ValidatedIfChangedField = string.Empty
          };
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = new model1.LengthTestEntity() {
            ValidatedField = string.Empty,
            ValidatedIfChangedField = string.Empty
          };
          session.Validate();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = new model1.LengthTestEntity() {
            ValidatedField = string.Empty,
            ValidatedIfChangedField = string.Empty
          };
          entity.Validate();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = new model1.LengthTestEntity() {
            ValidatedField = "valid",
            ValidatedIfChangedField = "valid"
          };
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = new model1.LengthTestEntity()  {
            ValidatedField = "valid",
            ValidatedIfChangedField = "valid"
          };
          session.Validate();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = new model1.LengthTestEntity() {
            ValidatedField = "valid",
            ValidatedIfChangedField = "valid"
          };
          entity.Validate();
        }
      });
    }

    [Test]
    public void UnchangedEntityTest()
    {
      PrepareDomain<model1.LengthTestEntity>(() =>
        new model1.LengthTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.LengthTestEntity>().Single();
          entityToChange.ValidatedField = entityToChange.ValidatedField;
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.LengthTestEntity>().Single();
          entityToChange.ValidatedField = entityToChange.ValidatedField;
          session.Validate();
        }
      });
    }
    #endregion

    #region ValidateIfChanged

    [Test]
    public void LengthConstraintTest1()
    {
      PrepareDomain<model1.LengthTestEntity>(() =>
        new model1.LengthTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      using (var session = domain.OpenSession()) 
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.LengthTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "";
        Assert.Throws<ValidationFailedException>(() => {
          session.Validate();
        });
      }

      using (var session = domain.OpenSession()) 
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.LengthTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "";
        Assert.Throws<ValidationFailedException>(() => {
          entityToChange.Validate();
        });
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.LengthTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "";
        var errors = session.ValidateAndGetErrors();
        var temp = errors;
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.LengthTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "x";
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.LengthTestEntity>().Single();
          entityToChange.ValidatedField = "";
          transaction.Complete();
        }
      });
    }

    [Test]
    public void NotEmptyConstraintTest1()
    {
      PrepareDomain<model1.NotEmptyTestEntity>(() =>
        new model1.NotEmptyTestEntity() {  ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.NotEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = string.Empty;
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.NotEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = string.Empty;
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.NotEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = string.Empty;
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.NotEmptyTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = string.Empty;
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.NotEmptyTestEntity>().Single();
          entityToChange.ValidatedField = string.Empty;
          transaction.Complete();
        }
      });
    }

    [Test]
    public void NotNullConstraintTest1()
    {
      PrepareDomain<model1.NotNullTestEntity>(() => 
        new model1.NotNullTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.NotNullTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.NotNullTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.NotNullTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.NotNullTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = null;
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.NotNullTestEntity>().Single();
          entityToChange.ValidatedField = null;
          transaction.Complete();
        }
      });
    }

    [Test]
    public void NotNullOrEmptyConstraintTest1()
    {
      PrepareDomain<model1.NotNullOrEmptyTestEntity>(() => 
        new model1.NotNullOrEmptyTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      using (var session = domain.OpenSession()) 
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.NotNullOrEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.NotNullOrEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.NotNullOrEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.NotNullOrEmptyTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = null;
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.NotNullOrEmptyTestEntity>().Single();
          entityToChange.ValidatedField = null;
          transaction.Complete();
        }
      });
    }

    [Test]
    public void PastConstraintTest1()
    {
      PrepareDomain<model1.PastConstraintTestEntity>(() => 
        new model1.PastConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1), ValidatedField = DateTime.Now - TimeSpan.FromHours(1) });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.PastConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.PastConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.PastConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.PastConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.PastConstraintTestEntity>().Single();
          entityToChange.ValidatedField = DateTime.Now + TimeSpan.FromHours(1);
          transaction.Complete();
        }
      });
    }

    [Test]
    public void FutureConstraintTest1()
    {
      PrepareDomain<model1.FutureConstraintTestEntity>(() => 
        new model1.FutureConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1), ValidatedField = DateTime.Now + TimeSpan.FromHours(1) });

      using (var session = domain.OpenSession()) 
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.FutureConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.FutureConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.FutureConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.FutureConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.FutureConstraintTestEntity>().Single();
          entityToChange.ValidatedField = DateTime.Now - TimeSpan.FromHours(1);
          transaction.Complete();
        }
      });
    }

    [Test]
    public void EmailConstraintTest1()
    {
      PrepareDomain<model1.EmailConstraintTestEntity>(() => 
        new model1.EmailConstraintTestEntity() { ValidatedIfChangedField = "julian1990@mail.ru", ValidatedField = "julian1990@mail.ru" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.EmailConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "lol";
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession()) 
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.EmailConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "lol";
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.EmailConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "lol";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.EmailConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "lol";
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.EmailConstraintTestEntity>().Single();
          entityToChange.ValidatedField = "lol";
          transaction.Complete();
        }
      });
    }

    [Test]
    public void RangeConstraintTest1()
    {
      PrepareDomain<model1.RangeConstraintTestEntity>(() => 
        new model1.RangeConstraintTestEntity() { ValidatedIfChangedField = 6, ValidatedField = 6 });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.RangeConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = 12;
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.RangeConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = 12;
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.RangeConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = 12;
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.RangeConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = 12;
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.RangeConstraintTestEntity>().Single();
          entityToChange.ValidatedField = 12;
          transaction.Complete();
        }
      });
    }

    [Test]
    public void RegExConstraintTest1()
    {
      PrepareDomain<model1.RegExConstraintTestEntity>(() => 
        new model1.RegExConstraintTestEntity() { ValidatedIfChangedField = "abc", ValidatedField = "abc" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.RegExConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "***";
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.RegExConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "***";
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.RegExConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "***";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.RegExConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "***";
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.RegExConstraintTestEntity>().Single();
          entityToChange.ValidatedField = "***";
          transaction.Complete();
        }
      });
    }
    #endregion

    #region ValidateIfChanged_SkipOnTransactionCommit

    [Test]
    public void LengthConstraintTest2()
    {
      PrepareDomain<model2.LengthTestEntity>(() => 
        new model2.LengthTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.LengthTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "";
        Assert.Throws<ValidationFailedException>(() => { session.Validate(); });
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.LengthTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "";
        Assert.Throws<ValidationFailedException>(() => { entityToChange.Validate(); });
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.LengthTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.LengthTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "";
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.LengthTestEntity>().Single();
          entityToChange.ValidatedField = "";
          transaction.Complete();
        }
      });
    }

    [Test]
    public void NotEmptyConstraintTest2()
    {
      PrepareDomain<model2.NotEmptyTestEntity>(() => 
        new model2.NotEmptyTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.NotEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = string.Empty;
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.NotEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = string.Empty;
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.NotEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = string.Empty;
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.NotEmptyTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = string.Empty;
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.NotEmptyTestEntity>().Single();
          entityToChange.ValidatedField = string.Empty;
          transaction.Complete();
        }
      });
    }

    [Test]
    public void NotNullConstraintTest2()
    {
      PrepareDomain<model2.NotNullTestEntity>(() => 
        new model2.NotNullTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.NotNullTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.NotNullTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.NotNullTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.NotNullTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = null;
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.NotNullTestEntity>().Single();
          entityToChange.ValidatedField = null;
          transaction.Complete();
        }
      });
    }

    [Test]
    public void NotNullOrEmptyConstraintTest2()
    {
      PrepareDomain<model2.NotNullOrEmptyTestEntity>(() => 
        new model2.NotNullOrEmptyTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.NotNullOrEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.NotNullOrEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.NotNullOrEmptyTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = null;
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.NotNullOrEmptyTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = null;
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.NotNullOrEmptyTestEntity>().Single();
          entityToChange.ValidatedField = null;
          transaction.Complete();
        }
      });
    }

    [Test]
    public void PastConstraintTest2()
    {
      PrepareDomain<model2.PastConstraintTestEntity>(() => 
        new model2.PastConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1), ValidatedField = DateTime.Now - TimeSpan.FromHours(1) });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.PastConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.PastConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.PastConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.PastConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.PastConstraintTestEntity>().Single();
          entityToChange.ValidatedField = DateTime.Now + TimeSpan.FromHours(1);
          transaction.Complete();
        }
      });
    }

    [Test]
    public void FutureConstraintTest2()
    {
      PrepareDomain<model2.FutureConstraintTestEntity>(() => 
        new model2.FutureConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1), ValidatedField = DateTime.Now + TimeSpan.FromHours(1) });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.FutureConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.FutureConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.FutureConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.FutureConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.FutureConstraintTestEntity>().Single();
          entityToChange.ValidatedField = DateTime.Now - TimeSpan.FromHours(1);
          transaction.Complete();
        }
      });
    }

    [Test]
    public void EmailConstraintTest2()
    {
      PrepareDomain<model2.EmailConstraintTestEntity>(() => 
        new model2.EmailConstraintTestEntity() { ValidatedIfChangedField = "julian1990@mail.ru", ValidatedField = "julian1990@mail.ru" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.EmailConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "lol";
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()){
        var entityToChange = session.Query.All<model2.EmailConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "lol";
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.EmailConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "lol";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.EmailConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "lol";
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.EmailConstraintTestEntity>().Single();
          entityToChange.ValidatedField = "lol";
          transaction.Complete();
        }
      });
    }

    [Test]
    public void RangeConstraintTest2()
    {
      PrepareDomain<model2.RangeConstraintTestEntity>(() =>
        new model2.RangeConstraintTestEntity() { ValidatedIfChangedField = 6, ValidatedField = 6 });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.RangeConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = 12;
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.RangeConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = 12;
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.RangeConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = 12;
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.RangeConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = 12;
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.RangeConstraintTestEntity>().Single();
          entityToChange.ValidatedField = 12;
          transaction.Complete();
        }
      });
    }

    [Test]
    public void RegExConstraintTest2()
    {
      PrepareDomain<model2.RegExConstraintTestEntity>(() => 
        new model2.RegExConstraintTestEntity() { ValidatedIfChangedField = "abc", ValidatedField = "abc" });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.RegExConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "***";
        Assert.Throws<ValidationFailedException>(() => session.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.RegExConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "***";
        Assert.Throws<ValidationFailedException>(() => entityToChange.Validate());
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.RegExConstraintTestEntity>().Single();
        entityToChange.ValidatedIfChangedField = "***";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.RegExConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "***";
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.RegExConstraintTestEntity>().Single();
          entityToChange.ValidatedField = "***";
          transaction.Complete();
        }
      });
    }

    #endregion

    #region ValidateIfChanged_IsImmediate

    [Test]
    public void LengthConstraintTest3() {
      PrepareDomain<model3.LengthTestEntity>(() => 
        new model3.LengthTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });
     
      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model3.LengthTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "";
        }
      });
    }

    [Test]
    public void NotEmptyConstraintTest3()
    {
      PrepareDomain<model3.NotEmptyTestEntity>(() =>
        new model3.NotEmptyTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model3.NotEmptyTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = string.Empty;
        }
      });
    }

    [Test]
    public void NotNullConstraintTest3()
    {
      PrepareDomain<model3.NotNullTestEntity>(() => 
        new model3.NotNullTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model3.NotNullTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = null;
        }
      });
    }

    [Test]
    public void NotNullOrEmptyConstraintTest3()
    {
      PrepareDomain<model3.NotNullOrEmptyTestEntity>(() =>
        new model3.NotNullOrEmptyTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model3.NotNullOrEmptyTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = null;
        }
      });
    }

    [Test]
    public void PastConstraintTest3()
    {
      PrepareDomain<model3.PastConstraintTestEntity>(() =>
        new model3.PastConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1), ValidatedField = DateTime.Now - TimeSpan.FromHours(1) });


      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model3.PastConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
        }
      });
    }

    [Test]
    public void FutureConstraintTest3()
    {
      PrepareDomain<model3.FutureConstraintTestEntity>(() => 
        new model3.FutureConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1), ValidatedField = DateTime.Now + TimeSpan.FromHours(1) });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model3.FutureConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
        }
      });
    }

    [Test]
    public void EmailConstraintTest3()
    {
      PrepareDomain<model3.EmailConstraintTestEntity>(() =>
        new model3.EmailConstraintTestEntity() { ValidatedIfChangedField = "julian1990@mail.ru", ValidatedField = "julian1990@mail.ru" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model3.EmailConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "lol";
        }
      });
    }

    [Test]
    public void RangeConstraintTest3()
    {
      PrepareDomain<model3.RangeConstraintTestEntity>(() =>
        new model3.RangeConstraintTestEntity() { ValidatedIfChangedField = 6, ValidatedField = 6 });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model3.RangeConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = 12;
        }
      });
    }

    [Test]
    public void RegExConstraintTest3()
    {
      PrepareDomain<model3.RegExConstraintTestEntity>(() => 
        new model3.RegExConstraintTestEntity() { ValidatedIfChangedField = "abc", ValidatedField = "abc" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model3.RegExConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "***";
        }
      });
    }

    #endregion

    #region ValidateIfChanged_IsImmediate_SkipOnTransactionCommit

    [Test]
    public void LengthConstraintTest4() {
      PrepareDomain<model4.LengthTestEntity>(() => 
        new model4.LengthTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model4.LengthTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "";
        }
      });
    }

    [Test]
    public void NotEmptyConstraintTest4()
    {
      PrepareDomain<model4.NotEmptyTestEntity>(() =>
        new model4.NotEmptyTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model4.NotEmptyTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = string.Empty;
        }
      });
    }

    [Test]
    public void NotNullConstraintTest4()
    {
      PrepareDomain<model4.NotNullTestEntity>(() => 
        new model4.NotNullTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model4.NotNullTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = null;
        }
      });
    }

    [Test]
    public void NotNullOrEmptyConstraintTest4()
    {
      PrepareDomain<model4.NotNullOrEmptyTestEntity>(() => 
        new model4.NotNullOrEmptyTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model4.NotNullOrEmptyTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = null;
        }
      });
    }

    [Test]
    public void PastConstraintTest4()
    {
      PrepareDomain<model4.PastConstraintTestEntity>(() =>
        new model4.PastConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1), ValidatedField = DateTime.Now - TimeSpan.FromHours(1) });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model4.PastConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1);
        }
      });
    }

    [Test]
    public void FutureConstraintTest4()
    {
      PrepareDomain<model4.FutureConstraintTestEntity>(() =>
        new model4.FutureConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1), ValidatedField = DateTime.Now + TimeSpan.FromHours(1) });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model4.FutureConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1);
        }
      });
    }

    [Test]
    public void EmailConstraintTest4()
    {
      PrepareDomain<model4.EmailConstraintTestEntity>(() =>
        new model4.EmailConstraintTestEntity() { ValidatedIfChangedField = "julian1990@mail.ru", ValidatedField = "julian1990@mail.ru" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model4.EmailConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "lol";
        }
      });
    }

    [Test]
    public void RangeConstraintTest4()
    {
      PrepareDomain<model4.RangeConstraintTestEntity>(() =>
        new model4.RangeConstraintTestEntity() { ValidatedIfChangedField = 6, ValidatedField = 6 });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model4.RangeConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = 12;
        }
      });
    }

    [Test]
    public void RegExConstraintTest4()
    {
      PrepareDomain<model4.RegExConstraintTestEntity>(() =>
        new model4.RegExConstraintTestEntity() { ValidatedIfChangedField = "abc", ValidatedField = "abc" });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model4.RegExConstraintTestEntity>().Single();
          entityToChange.ValidatedIfChangedField = "***";
        }
      });
    }

    #endregion

    #region Structure

    [Test]
    public void StructureTest1()
    {
      PrepareDomain<model1.StructureTestEntity>(() => new model1.StructureTestEntity() {
        StructureField = new model1.TestStructure() {
          ValidatedField = "valid",
          ValidatedIfChangedField = "valid"
        }
      });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction())  {
        var entityToChange = session.Query.All<model1.StructureTestEntity>().Single();
        entityToChange.StructureField.ValidatedIfChangedField = "lol";
        Assert.Throws<ValidationFailedException>(() => { session.Validate(); });
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.StructureTestEntity>().Single();
        entityToChange.StructureField.ValidatedIfChangedField = "lol";
        Assert.Throws<ValidationFailedException>(() => {
          entityToChange.Validate();
        });
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model1.StructureTestEntity>().Single();
        entityToChange.StructureField.ValidatedIfChangedField = "lol";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.StructureTestEntity>().Single();
          entityToChange.StructureField.ValidatedIfChangedField = "lol";
          transaction.Complete();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model1.StructureTestEntity>().Single();
          entityToChange.StructureField.ValidatedField = "lol";
          transaction.Complete();
        }
      });
    }

    [Test]
    public void StructureTest2()
    {
      PrepareDomain<model2.StructureTestEntity>(() => new model2.StructureTestEntity() {
        StructureField = new model2.TestStructure() {
          ValidatedField = "valid",
          ValidatedIfChangedField = "valid"
        }
      });

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.StructureTestEntity>().Single();
        entityToChange.StructureField.ValidatedIfChangedField = "lol";
        Assert.Throws<ValidationFailedException>(() => {
          session.Validate();
        });
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.StructureTestEntity>().Single();
        entityToChange.StructureField.ValidatedIfChangedField = "lol";
        Assert.Throws<ValidationFailedException>(() => {
          entityToChange.Validate();
        });
      }

      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var entityToChange = session.Query.All<model2.StructureTestEntity>().Single();
        entityToChange.StructureField.ValidatedIfChangedField = "lol";
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
      }

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entityToChange = session.Query.All<model2.StructureTestEntity>().Single();
          entityToChange.StructureField.ValidatedIfChangedField = "lol";
          transaction.Complete();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction())  {
          var entityToChange = session.Query.All<model2.StructureTestEntity>().Single();
          entityToChange.StructureField.ValidatedField = "lol";
          transaction.Complete();
        }
      });
    }

    [Test]
    public void StructureTest3()
    {
      PrepareDomain<model3.StructureTestEntity>(() => new model3.StructureTestEntity() {
        StructureField = new model3.TestStructure() {
          ValidatedField = "valid",
          ValidatedIfChangedField = "valid"
        }
      });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction())  {
          var entityToChange = session.Query.All<model3.StructureTestEntity>().Single();
          entityToChange.StructureField.ValidatedIfChangedField = "***";
        }
      });
    }

    [Test]
    public void StructureTest4()
    {
      PrepareDomain<model4.StructureTestEntity>(() => new model4.StructureTestEntity() {
        StructureField = new model4.TestStructure() {
          ValidatedField = "valid",
          ValidatedIfChangedField = "valid"
        }
      });

      Assert.Throws<ArgumentException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction())  {
          var entityToChange = session.Query.All<model4.StructureTestEntity>().Single();
          entityToChange.StructureField.ValidatedIfChangedField = "***";
        }
      });
    }

    [Test]
    public void UnchangedStructureTest()
    {
      PrepareDomain<model4.StructureTestEntity>(() => new model4.StructureTestEntity() {
        StructureField = new model4.TestStructure() {
          ValidatedField = "valid",
          ValidatedIfChangedField = "valid"
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction())  {
          var entityToChange = session.Query.All<model4.StructureTestEntity>().Single();
          entityToChange.StructureField.ValidatedIfChangedField = entityToChange.StructureField.ValidatedIfChangedField;
          transaction.Complete();
        }
      });
    }

    [Test]
    public void IncludedStructureTest()
    {
      PrepareDomain<model1.IncludedStructure>(() => new model1.IncludedStructure() {
        StructureField = new model1.Structure1() {
          ValidatedIfChangedField = "valid",
          EnclosedStructureField = new model1.Structure2() {
            ValidatedIfChangedField2 = "valid"
          }
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession()) 
        using (var transaction = session.OpenTransaction())  {
          var entity = session.Query.All<model1.IncludedStructure>().Single();
          entity.StructureField.ValidatedIfChangedField = "";
          session.Validate();
        }
      });

      Assert.Throws<ValidationFailedException>(() => {
        using (var session = domain.OpenSession())
        using (var transaction = session.OpenTransaction()) {
          var entity = session.Query.All<model1.IncludedStructure>().Single();
          entity.StructureField.EnclosedStructureField.ValidatedIfChangedField2 = "";
          session.Validate();
        }
      });

      Assert.DoesNotThrow(() => {
        using (var session = domain.OpenSession()) {
          using (var transaction = session.OpenTransaction()) {
            var entity = session.Query.All<model1.IncludedStructure>().Single();
            entity.StructureField.ValidatedIfChangedField = entity.StructureField.ValidatedIfChangedField;
            entity.StructureField.EnclosedStructureField.ValidatedIfChangedField2 = entity.StructureField.EnclosedStructureField.ValidatedIfChangedField2;
            transaction.Complete();
          }
        }
      });
    }
    #endregion

    private void PrepareDomain<T>(Func<Entity> entity) where T : Entity
    {
      BuildDomain(typeof (T));
      PoppualateData(entity);
    }

    private void PoppualateData(Func<Entity> entity)
    {
      using (var session = domain.OpenSession()) 
      using (var transaction = session.OpenTransaction()) {
        entity.Invoke();
        transaction.Complete();
      }
    }

    private void BuildDomain(Type type)
    {
      var config = BuildConfiguration(type);
      domain = Domain.Build(config);
    }

    private DomainConfiguration BuildConfiguration(Type type)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(type);
      return configuration;
    }
  }
}
