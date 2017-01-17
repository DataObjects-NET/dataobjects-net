// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.08.08

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0659_GroupByConditionalExpressionWithEnumsModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0659_GroupByConditionalExpressionWithEnumsModel
{
  [HierarchyRoot]
  public class EntityWithGender : Entity
  {
    [Field]
    [Key]
    public Guid Id { get; set; }

    [Field]
    public Gender? NullableGender { get; set; }

    [Field]
    public Gender Gender { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithExtendedGender : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public ExtendedGender? NullableGender { get; set; }

    [Field]
    public ExtendedGender Gender { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithIntFlags : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public SomeIntFlags Flags { get; set; }

    [Field]
    public SomeIntFlags? NullableFlags { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithLongFlags : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public SomeLongFlags Flags { get; set; }

    [Field]
    public SomeLongFlags? NullableFlags { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithDateTime : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public DateTime? NullableDateTime { get; set; }

    [Field]
    public DateTime DateTime { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithInt : Entity
  {
    [Field, Key]
    public int Id { get; set; } 

    [Field]
    public int Int { get; set; }

    [Field]
    public int? NullableInt { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithDecimal : Entity
  {
    [Field, Key]
    public Guid Id { get; set; }

    [Field]
    public decimal Sum { get; set; }
  }

  public enum Gender
  {
    None = 0,
    Male = 1,
    Female = 2
  }

  public enum ExtendedGender : long
  {
    None = 0,
    Male = 1,
    Female = 2,
    Transgender = 4
  }

  [Flags]
  public enum SomeIntFlags : int
  {
    None = 0,
    Flag1 = 1 << 0,
    Flag2 = 1 << 1,
    Flag3 = 1 << 2,
    Flag4 = 1 << 3,
    Flag5 = 1 << 4,
    Flag6 = 1 << 5,
    Flag7 = 1 << 6,
    Flag8 = 1 << 7,
    Flag9 = 1 << 8,
    Flag10 = 1 << 9,
    Flag11 = 1 << 10,
    Flag12 = 1 << 11,
    Flag13 = 1 << 12,
    Flag14 = 1 << 13,
    Flag15 = 1 << 14,
    Flag16 = 1 << 15,
    Flag17 = 1 << 16,
    Flag18 = 1 << 17,
    Flag19 = 1 << 18,
    Flag20 = 1 << 19,
  }

  [Flags]
  public enum SomeLongFlags : long
  {
    None = 0,
    Flag1 = 1L << 0,
    Flag2 = 1L << 1,
    Flag3 = 1L << 2,
    Flag4 = 1L << 3,
    Flag5 = 1L << 4,
    Flag6 = 1L << 5,
    Flag7 = 1L << 6,
    Flag8 = 1L << 7,
    Flag9 = 1L << 8,
    Flag10 = 1L << 9,
    Flag11 = 1L << 10,
    Flag12 = 1L << 11,
    Flag13 = 1L << 12,
    Flag14 = 1L << 13,
    Flag15 = 1L << 14,
    Flag16 = 1L << 15,
    Flag17 = 1L << 16,
    Flag18 = 1L << 17,
    Flag19 = 1L << 18,
    Flag20 = 1L << 19,
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0659_EnumExpressionsAndConstantsTranslationBug : AutoBuildTest
  {
    private readonly DateTime nullableFieldDateTime = new DateTime(2015, 6, 6);
    private readonly DateTime firstDateTime = new DateTime(2013, 10, 10);
    private readonly DateTime secondDateTime = new DateTime(2014, 12, 10);
    private readonly DateTime defaultDateTime = new DateTime(2012, 12, 12);

    [Test]
    public void DirectGroupByByNotNullEnum()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithGender>()
          .GroupBy(x => x.Gender)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g=> g.Key==Gender.Male));
        Assert.That(result.Any(g=>g.Key==Gender.Female));

      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByConditionalOperatorForIntEnumTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithGender>()
          .GroupBy(x => x.NullableGender.HasValue ? x.NullableGender.Value : Gender.None)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==Gender.None));
        Assert.That(genderGroups.Any(g => g.Key==Gender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByConditionalOperatorForLongEnumTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithExtendedGender>()
          .GroupBy(x => x.NullableGender.HasValue ? x.NullableGender.Value : ExtendedGender.None)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==ExtendedGender.None));
        Assert.That(genderGroups.Any(g => g.Key==ExtendedGender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByConditionalOperatorForIntFlagTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithIntFlags>()
          .GroupBy(x => x.NullableFlags.HasValue ? x.NullableFlags.Value : SomeIntFlags.None)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==SomeIntFlags.None));
        Assert.That(genderGroups.Any(g => g.Key==SomeIntFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByConditionalOperatorForLongFlagTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithLongFlags>()
          .GroupBy(x => x.NullableFlags.HasValue ? x.NullableFlags.Value : SomeLongFlags.None)
          .Select(g => new { g.Key, Items = g })
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==SomeLongFlags.None));
        Assert.That(genderGroups.Any(g => g.Key==SomeLongFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByConditionalOperatorForDateTimeTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithDateTime>()
          .GroupBy(x => x.NullableDateTime.HasValue ? x.NullableDateTime.Value : new DateTime(2012, 12, 12))
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==new DateTime(2012, 12, 12)));
        Assert.That(genderGroups.Any(g => g.Key==nullableFieldDateTime));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByConditionalOperatorForIntTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithInt>()
          .GroupBy(x => x.NullableInt.HasValue ? x.NullableInt : -1)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==-1));
        Assert.That(result.Any(g => g.Key==10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByConditionalOperatorForIntEnumTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithGender>()
          .Select(el => new {
            Id = el.Id,
            Gender = el.NullableGender.HasValue ? el.NullableGender.Value : Gender.None
          })
          .GroupBy(x => x.Gender)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==Gender.None));
        Assert.That(genderGroups.Any(g => g.Key==Gender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByConditionalOperatorForLongEnumTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithExtendedGender>()
          .Select(el => new {
            Id = el.Id,
            Gender = el.NullableGender.HasValue ? el.NullableGender.Value : ExtendedGender.None
          })
          .GroupBy(x => x.Gender)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==ExtendedGender.None));
        Assert.That(genderGroups.Any(g => g.Key==ExtendedGender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByConditionalOperatorForIntFlagTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithIntFlags>()
          .Select(el => new {
            Id = el.Id,
            Flags = el.NullableFlags.HasValue ? el.NullableFlags.Value : SomeIntFlags.None
          })
          .GroupBy(x => x.Flags)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==SomeIntFlags.None));
        Assert.That(genderGroups.Any(g => g.Key==SomeIntFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByConditionalOperatorForLongFlagTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithLongFlags>()
          .Select(el => new {
            Id = el.Id,
            Flags = el.NullableFlags.HasValue ? el.NullableFlags.Value : SomeLongFlags.None
          })
          .GroupBy(x => x.Flags)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==SomeLongFlags.None));
        Assert.That(genderGroups.Any(g => g.Key==SomeLongFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByConditionalOperatorForDateTimeTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithDateTime>()
          .Select(el => new {
            Id = el.Id,
            DateTime = el.NullableDateTime.HasValue ? el.NullableDateTime.Value : new DateTime(2012, 12, 12)
          })
          .GroupBy(x => x.DateTime)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==new DateTime(2012, 12, 12)));
        Assert.That(genderGroups.Any(g => g.Key==nullableFieldDateTime));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByConditionalOperatorForIntTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithInt>()
          .Select(el => new {
            Id = el.Id,
            NullableInt = el.NullableInt.HasValue ? el.NullableInt : -1
          })
          .GroupBy(x => x.NullableInt)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==-1));
        Assert.That(result.Any(g => g.Key==10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForIntEnumTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithGender>()
          .GroupBy(x => x.NullableGender ?? Gender.None)
          .Select(g => new { Key = g.Key, Items = g })
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==Gender.None));
        Assert.That(result.Any(g => g.Key==Gender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForLongEnumTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithExtendedGender>()
          .GroupBy(x => x.NullableGender ?? ExtendedGender.None)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==ExtendedGender.None));
        Assert.That(result.Any(g => g.Key==ExtendedGender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForIntFlagTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithIntFlags>()
          .GroupBy(x => x.NullableFlags ?? SomeIntFlags.None)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==SomeIntFlags.None));
        Assert.That(result.Any(g => g.Key==SomeIntFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForLongFlagTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithLongFlags>()
          .GroupBy(x => x.NullableFlags ?? SomeLongFlags.None)
          .Select(g => new { Key = g.Key, Items = g })
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==SomeLongFlags.None));
        Assert.That(result.Any(g => g.Key==SomeLongFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForDateTimeTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithDateTime>()
          .GroupBy(x => x.NullableDateTime ?? new DateTime(2012, 12, 12))
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==nullableFieldDateTime));
        Assert.That(result.Any(g => g.Key==new DateTime(2012, 12, 12)));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForIntTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithInt>()
          .GroupBy(x => x.NullableInt ?? -1)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==-1));
        Assert.That(result.Any(g => g.Key==10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForIntEnumTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithGender>()
          .Select(el => new {
            Id = el.Id,
            NullableGender = el.NullableGender ?? Gender.None
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==Gender.None));
        Assert.That(result.Any(g => g.Key==Gender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForLongEnumTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithExtendedGender>()
          .Select(el => new {
            Id = el.Id,
            NullableGender = el.NullableGender ?? ExtendedGender.None
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==ExtendedGender.None));
        Assert.That(result.Any(g => g.Key==ExtendedGender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForIntFlagTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithIntFlags>()
          .Select(el => new {
            Id = el.Id,
            NullableGender = el.NullableFlags ?? SomeIntFlags.None
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==SomeIntFlags.None));
        Assert.That(result.Any(g => g.Key==SomeIntFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForLongFlagTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithLongFlags>()
          .Select(el => new {
            Id = el.Id,
            NullableGender = el.NullableFlags ?? SomeLongFlags.None
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==SomeLongFlags.None));
        Assert.That(result.Any(g => g.Key==SomeLongFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForDateTimeTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithDateTime>()
          .Select(el => new {
            Id = el.Id,
            NullableGender = el.NullableDateTime ?? new DateTime(2012, 12, 12)
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==new DateTime(2012, 12, 12)));
        Assert.That(result.Any(g => g.Key==nullableFieldDateTime));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForIntTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithInt>()
          .Select(el => new {
            Id = el.Id,
            NullableInt = el.NullableInt ?? -1
          })
          .GroupBy(x => x.NullableInt)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==-1));
        Assert.That(result.Any(g => g.Key==10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByGetValueOrDefaultForIntEnumTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithGender>()
          .GroupBy(x => x.NullableGender.GetValueOrDefault(Gender.None))
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==Gender.None));
        Assert.That(result.Any(g => g.Key==Gender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByGetValueOrDefaultForLongEnumTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithExtendedGender>()
          .GroupBy(x => x.NullableGender.GetValueOrDefault(ExtendedGender.None))
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==ExtendedGender.None));
        Assert.That(result.Any(g => g.Key==ExtendedGender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByGetValueOrDefaultForIntFlagTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithIntFlags>()
          .GroupBy(x => x.NullableFlags.GetValueOrDefault(SomeIntFlags.None))
          .Select(g => new { Key = g.Key, Items = g })
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==SomeIntFlags.None));
        Assert.That(result.Any(g => g.Key==SomeIntFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByGetValueOrDefaultForLongFlagTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithLongFlags>()
          .GroupBy(x => x.NullableFlags.GetValueOrDefault(SomeLongFlags.None))
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==SomeLongFlags.None));
        Assert.That(result.Any(g => g.Key==SomeLongFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByGetValueOrDefaultForDateTimeTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithDateTime>()
          .GroupBy(x => x.NullableDateTime.GetValueOrDefault(new DateTime(2012, 12, 12)))
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==new DateTime(2012, 12, 12)));
        Assert.That(result.Any(g => g.Key==nullableFieldDateTime));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByGetValueOrDefaultForIntTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithInt>()
          .GroupBy(x => x.NullableInt.GetValueOrDefault(-1))
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==-1));
        Assert.That(result.Any(g => g.Key==10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForIntEnumTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithGender>()
          .Select(el => new {
            el.Id,
            NullableGender = el.NullableGender.GetValueOrDefault(Gender.None)
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==Gender.None));
        Assert.That(result.Any(g => g.Key==Gender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForLongEnumTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithExtendedGender>()
          .Select(el => new {
            el.Id,
            NullableGender = el.NullableGender.GetValueOrDefault(ExtendedGender.None)
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==ExtendedGender.None));
        Assert.That(result.Any(g => g.Key==ExtendedGender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForFlagTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithIntFlags>()
          .Select(el => new {
            el.Id,
            NullableGender = el.NullableFlags.GetValueOrDefault(SomeIntFlags.None)
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==SomeIntFlags.None));
        Assert.That(result.Any(g => g.Key==SomeIntFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForLongFlagTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithLongFlags>()
          .Select(el => new {
            el.Id,
            NullableGender = el.NullableFlags.GetValueOrDefault(SomeLongFlags.None)
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==SomeLongFlags.None));
        Assert.That(result.Any(g => g.Key==SomeLongFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForDateTimeTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithDateTime>()
          .Select(el => new {
            el.Id,
            NullableGender = el.NullableDateTime.GetValueOrDefault(new DateTime(2012, 12, 12))
          })
          .GroupBy(x => x.NullableGender)
          .Select(g => new {Key = g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==new DateTime(2012, 12, 12)));
        Assert.That(result.Any(g => g.Key==nullableFieldDateTime));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForIntTest()
    {
      Action<Session> testAction = (session) => {
        var result = session.Query.All<EntityWithInt>()
          .Select(el => new {
            Id = el.Id,
            NullableInt = el.NullableInt.GetValueOrDefault(-1)
          })
          .GroupBy(x => x.NullableInt)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result.Any(g => g.Key==-1));
        Assert.That(result.Any(g => g.Key==10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void OrderByHasFlagExpressionTest()
    {
      Action<Session> testAction = (session) => {
        var result1 = session.Query.All<EntityWithIntFlags>()
          .OrderBy(el => el.Flags.HasFlag(SomeIntFlags.Flag10)).ToArray();
        var result2 = session.Query.All<EntityWithLongFlags>()
          .OrderBy(el => el.Flags.HasFlag(SomeLongFlags.Flag10)).ToArray();
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void OrderByBitAndExpressionTest()
    {
      Action<Session> testAction = (session) => {
        var result1 = session.Query.All<EntityWithIntFlags>()
          .OrderBy(el => (el.Flags & SomeIntFlags.Flag1)==SomeIntFlags.Flag1).ToArray();
        var result2 = session.Query.All<EntityWithLongFlags>()
          .OrderBy(el => (el.Flags & SomeLongFlags.Flag1)==SomeLongFlags.Flag1).ToArray();
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void OrderByCoalescingOperatorTest()
    {
      Action<Session> testAction = (session) => {
        var result1 = session.Query.All<EntityWithIntFlags>()
          .OrderBy(el => el.NullableFlags ?? SomeIntFlags.Flag1).ToArray();
        var result2 = session.Query.All<EntityWithLongFlags>()
          .OrderBy(el => el.NullableFlags ?? SomeLongFlags.Flag1).ToArray();
        var result3 = session.Query.All<EntityWithGender>()
          .OrderBy(el => el.NullableGender ?? Gender.None).ToArray();
        var result4 = session.Query.All<EntityWithExtendedGender>()
          .OrderBy(el => el.NullableGender ?? ExtendedGender.None).ToArray();
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void OrderByInequalityExpressionTest()
    {
      Action<Session> testAction = (session) => {
        var result1 = session.Query.All<EntityWithGender>()
          .OrderBy(el => el.Gender!=Gender.Male).ToArray();
        var result2 = session.Query.All<EntityWithGender>()
          .OrderBy(el => Gender.Male!=el.Gender).ToArray();

        var result3 = session.Query.All<EntityWithExtendedGender>()
          .OrderBy(el => el.Gender!=ExtendedGender.Male).ToArray();
        var result4 = session.Query.All<EntityWithExtendedGender>()
          .OrderBy(el => ExtendedGender.Male!=el.Gender).ToArray();

        var result5 = session.Query.All<EntityWithIntFlags>()
          .OrderBy(el => el.Flags!=SomeIntFlags.Flag1).ToArray();
        var result6 = session.Query.All<EntityWithIntFlags>()
          .OrderBy(el => SomeIntFlags.Flag1!=el.Flags).ToArray();

        var result7 = session.Query.All<EntityWithLongFlags>()
          .OrderBy(el => el.Flags!=SomeLongFlags.Flag1).ToArray();
        var result8 = session.Query.All<EntityWithLongFlags>()
          .OrderBy(el => SomeLongFlags.Flag1!=el.Flags).ToArray();
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void OrderByEqualityExpression()
    {
      Action<Session> testAction = (session) => {
        var result1 = session.Query.All<EntityWithGender>()
          .OrderBy(el => el.Gender==Gender.Male).ToArray();
        var result2 = session.Query.All<EntityWithGender>()
          .OrderBy(el => Gender.Male==el.Gender).ToArray();

        var result3 = session.Query.All<EntityWithExtendedGender>()
          .OrderBy(el => el.Gender==ExtendedGender.Male).ToArray();
        var result4 = session.Query.All<EntityWithExtendedGender>()
          .OrderBy(el => ExtendedGender.Male==el.Gender).ToArray();

        var result5 = session.Query.All<EntityWithIntFlags>()
          .OrderBy(el => el.Flags==SomeIntFlags.Flag1).ToArray();
        var result6 = session.Query.All<EntityWithIntFlags>()
          .OrderBy(el => SomeIntFlags.Flag1==el.Flags).ToArray();

        var result7 = session.Query.All<EntityWithLongFlags>()
          .OrderBy(el => el.Flags==SomeLongFlags.Flag1).ToArray();
        var result8 = session.Query.All<EntityWithLongFlags>()
          .OrderBy(el => SomeLongFlags.Flag1==el.Flags).ToArray();
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void WhereByHasValueTest()
    {
      Action<Session> testAction = (session) => {
        var result1 = session.Query.All<EntityWithIntFlags>()
          .Where(el => el.Flags.HasFlag(SomeIntFlags.Flag1)).ToArray();
        var result2 = session.Query.All<EntityWithLongFlags>()
          .Where(el => el.Flags.HasFlag(SomeLongFlags.Flag1)).ToArray();
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void WhereByBitAndExpressionTest()
    {
      Action<Session> testAction = (session) => {
        var result1 = session.Query.All<EntityWithIntFlags>()
          .Where(el => (el.Flags & SomeIntFlags.Flag1)==SomeIntFlags.Flag1).ToArray();

        var result2 = session.Query.All<EntityWithLongFlags>()
          .Where(el => (el.Flags & SomeLongFlags.Flag1)==SomeLongFlags.Flag1).ToArray();
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void WhereByEqualityTest()
    {
      Action<Session> testAction = (session) => {
        var result1 = session.Query.All<EntityWithGender>()
          .Where(el => el.Gender==Gender.Male).ToArray();
        var result2 = session.Query.All<EntityWithGender>()
          .Where(el => Gender.Male==el.Gender).ToArray();

        var result3 = session.Query.All<EntityWithExtendedGender>()
          .Where(el => el.Gender==ExtendedGender.Male).ToArray();
        var result4 = session.Query.All<EntityWithExtendedGender>()
          .Where(el => ExtendedGender.Male==el.Gender).ToArray();

        var result5 = session.Query.All<EntityWithIntFlags>()
          .Where(el => el.Flags==SomeIntFlags.Flag1).ToArray();
        var result6 = session.Query.All<EntityWithIntFlags>()
          .Where(el => SomeIntFlags.Flag1==el.Flags).ToArray();

        var result7 = session.Query.All<EntityWithLongFlags>()
          .Where(el => el.Flags==SomeLongFlags.Flag1).ToArray();
        var result8 = session.Query.All<EntityWithLongFlags>()
          .Where(el => SomeLongFlags.Flag1==el.Flags).ToArray();
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void WhereByInequalityTest()
    {
      Action<Session> testAction = (session) => {
        var result1 = session.Query.All<EntityWithGender>()
          .Where(el => el.Gender!=Gender.Female).ToArray();
        var result2 = session.Query.All<EntityWithGender>()
          .Where(el => Gender.Female!=el.Gender).ToArray();

        var result3 = session.Query.All<EntityWithExtendedGender>()
          .Where(el => el.Gender!=ExtendedGender.Female).ToArray();
        var result4 = session.Query.All<EntityWithExtendedGender>()
          .Where(el => ExtendedGender.Female!=el.Gender).ToArray();

        var result5 = session.Query.All<EntityWithIntFlags>()
          .Where(el => el.Flags!=SomeIntFlags.Flag1).ToArray();
        var result6 = session.Query.All<EntityWithIntFlags>()
          .Where(el => SomeIntFlags.Flag1!=el.Flags).ToArray();

        var result7 = session.Query.All<EntityWithLongFlags>()
          .Where(el => el.Flags!=SomeLongFlags.Flag1).ToArray();
        var result8 = session.Query.All<EntityWithLongFlags>()
          .Where(el => SomeLongFlags.Flag1!=el.Flags).ToArray();
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void MathRoundTest()
    {
      Action<Session> testAction = (session) => {
        //OK
        Assert.DoesNotThrow(() => session.Query.All<EntityWithDecimal>().Select(e => (Math.Truncate(Math.Round(e.Sum, 2) * 100) / 100)).ToArray());
        //session.Query.All<EntityWithDecimal>().Select(e => (Math.Truncate(Math.Round(e.Sum, 2) * 100) / 100)).ToArray();

        //OK
        Assert.DoesNotThrow(() => session.Query.All<EntityWithDecimal>().Where(e => (Math.Truncate(Math.Round(e.Sum, 2) * 100) / 100) > 1).ToArray());

        //OK
        Assert.DoesNotThrow(() => session.Query.All<EntityWithDecimal>().OrderBy(e => Math.Truncate(Math.Round(e.Sum, 2) * 100) / 100).ToArray());

        //OK
        Assert.DoesNotThrow(() => session.Query.All<EntityWithDecimal>().Sum(e => Math.Truncate(Math.Round(e.Sum, 2) * 100) / 100));

        //OK
        Assert.DoesNotThrow(() => session.Query.All<EntityWithDecimal>().Select(e => (Math.Truncate(Math.Round(e.Sum, 2, MidpointRounding.AwayFromZero) * 100) / 100)).ToArray());

        //OK
        //Assert.DoesNotThrow(() => session.Query.All<EntityWithDecimal>().Where(e => (Math.Truncate(Math.Round(e.Sum, 2, MidpointRounding.AwayFromZero) * 100) / 100) > 1).ToArray());
        session.Query.All<EntityWithDecimal>().Where(e => (Math.Truncate(Math.Round(e.Sum, 2, MidpointRounding.AwayFromZero) * 100) / 100) > 1).ToArray();

        //FAIL
        Assert.DoesNotThrow(() => session.Query.All<EntityWithDecimal>().OrderBy(e => Math.Truncate(Math.Round(e.Sum, 2, MidpointRounding.AwayFromZero) * 100) / 100).ToArray());

        //FAIL
        Assert.DoesNotThrow(() => session.Query.All<EntityWithDecimal>().Sum(e => Math.Truncate(Math.Round(e.Sum, 2, MidpointRounding.AwayFromZero) * 100) / 100));
      };
      RunTestInSession(testAction);
    }

    private void RunTestInSession(Action<Session> testBody)
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        testBody.Invoke(session);
      }
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new EntityWithGender {Gender = Gender.Male};
        new EntityWithGender {Gender = Gender.Female, NullableGender = Gender.Female};

        new EntityWithExtendedGender {Gender = ExtendedGender.Male};
        new EntityWithExtendedGender {Gender = ExtendedGender.Female, NullableGender = ExtendedGender.Female};

        new EntityWithIntFlags {Flags = SomeIntFlags.Flag1 | SomeIntFlags.Flag20};
        new EntityWithIntFlags {Flags = SomeIntFlags.Flag1 | SomeIntFlags.Flag10, NullableFlags = SomeIntFlags.Flag10};

        new EntityWithLongFlags {Flags = SomeLongFlags.Flag1 | SomeLongFlags.Flag20};
        new EntityWithLongFlags {Flags = SomeLongFlags.Flag1 | SomeLongFlags.Flag20, NullableFlags = SomeLongFlags.Flag10};

        new EntityWithDateTime {DateTime = firstDateTime};
        new EntityWithDateTime {DateTime = secondDateTime, NullableDateTime = nullableFieldDateTime};

        new EntityWithInt {Int = 20};
        new EntityWithInt {Int = 20, NullableInt = 10};
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration() {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (EntityWithGender).Assembly, typeof (EntityWithGender).Namespace);
      return configuration;
    }
  }
}
