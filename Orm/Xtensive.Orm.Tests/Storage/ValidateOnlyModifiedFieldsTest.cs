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
using constraintFreeModel = Xtensive.Orm.Tests.Storage.SkipValidationOnCommitTest2Model.NoConstraints;
using model1 = Xtensive.Orm.Tests.Storage.SkipValidationOnCommitTest2Model.ValidateIfChanged;
using model2 = Xtensive.Orm.Tests.Storage.SkipValidationOnCommitTest2Model.ValidateIfChanged_SkipOnTransactionCommit;
using model3 = Xtensive.Orm.Tests.Storage.SkipValidationOnCommitTest2Model.ValidateIfChanged_IsImmediate;
using model4 = Xtensive.Orm.Tests.Storage.SkipValidationOnCommitTest2Model.ValidateIfChanged_IsImmediate_SkipOnTransactionCommit;

namespace Xtensive.Orm.Tests.Storage.SkipValidationOnCommitTest2Model
{
  namespace NoConstraints
  {
    [HierarchyRoot]
    public class LengthTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class NotNullOrEmptyTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class PastConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class FutureConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public DateTime ValidatedIfChangedField { get; set; }

      [Field]
      public DateTime ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class EmailConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public string ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class RangeConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public int ValidatedIfChangedField { get; set; }

      [Field]
      public int ValidatedField { get; set; }
    }

    [HierarchyRoot]
    public class RegExConstraintTestEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }

      [Field]
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public string ValidatedField { get; set; }
    }

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
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public Structure2 EnclosedStructureField { get; set; }
    }

    public class Structure2 : Structure
    {
      [Field]
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
      public string ValidatedIfChangedField { get; set; }

      [Field]
      public string ValidatedField { get; set; }
    }
  }

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

    public class TestStructure : Orm.Structure
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

    [SetUp]
    public void TestSetup()
    {
      BuildInitialDomain();
      PoppulateData();
    }

    #region General

    [Test]
    public void PostPersistValidationTest()
    {
      var configuration = BuildConfiguration(typeof (model1.LengthTestEntity));
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuration);

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
      var configuarion = BuildConfiguration(typeof (model1.LengthTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.LengthTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.LengthTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.NotEmptyTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.NotNullTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.NotNullOrEmptyTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.PastConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.FutureConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.EmailConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.RangeConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.RegExConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.LengthTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.NotEmptyTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.NotNullTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.NotNullOrEmptyTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.PastConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.FutureConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.EmailConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.RangeConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.RegExConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.LengthTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.NotEmptyTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.NotNullTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.NotNullOrEmptyTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.PastConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.FutureConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.EmailConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.RangeConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.RegExConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.LengthTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.NotEmptyTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.NotNullTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.NotNullOrEmptyTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.PastConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.FutureConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.EmailConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.RangeConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.RegExConstraintTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model1.TestStructure));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model2.StructureTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model3.StructureTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.StructureTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuarion = BuildConfiguration(typeof (model4.StructureTestEntity));
      configuarion.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuarion);

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
      var configuration = BuildConfiguration(typeof (model1.IncludedStructure));
      configuration.UpgradeMode = DomainUpgradeMode.PerformSafely;
      BuildDomain(configuration);

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

    private void PoppulateData()
    {
      using (var session = domain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        new constraintFreeModel.LengthTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" };
        new constraintFreeModel.NotEmptyTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" };
        new constraintFreeModel.NotNullTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" };
        new constraintFreeModel.NotNullOrEmptyTestEntity() { ValidatedIfChangedField = "Some string", ValidatedField = "Some string" };
        new constraintFreeModel.PastConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now - TimeSpan.FromHours(1), ValidatedField = DateTime.Now - TimeSpan.FromHours(1) };
        new constraintFreeModel.FutureConstraintTestEntity() { ValidatedIfChangedField = DateTime.Now + TimeSpan.FromHours(1), ValidatedField = DateTime.Now + TimeSpan.FromHours(1) };
        new constraintFreeModel.EmailConstraintTestEntity() { ValidatedIfChangedField = "julian1990@mail.ru", ValidatedField = "julian1990@mail.ru" };
        new constraintFreeModel.RangeConstraintTestEntity() { ValidatedIfChangedField = 6, ValidatedField = 6 };
        new constraintFreeModel.RegExConstraintTestEntity() { ValidatedIfChangedField = "abc", ValidatedField = "abc" };
        new constraintFreeModel.IncludedStructure() {
          StructureField = new constraintFreeModel.Structure1() {
            ValidatedIfChangedField = "valid",
            EnclosedStructureField = new constraintFreeModel.Structure2() {
              ValidatedIfChangedField2 = "valid"
            }
          }
        };
        new constraintFreeModel.StructureTestEntity() {
          StructureField = new constraintFreeModel.TestStructure() {
            ValidatedField = "valid",
            ValidatedIfChangedField = "valid"
          }
        };
        transaction.Complete();
      }
    }

    private void BuildDomain(DomainConfiguration configuration)
    {
      domain = Domain.Build(configuration);
    }

    private void BuildInitialDomain()
    {
      var config = BuildConfiguration(typeof (constraintFreeModel.LengthTestEntity));
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      BuildDomain(config);
    }

    private DomainConfiguration BuildConfiguration(Type type)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(type.Assembly, type.Namespace);
      return configuration;
    }
  }
}
