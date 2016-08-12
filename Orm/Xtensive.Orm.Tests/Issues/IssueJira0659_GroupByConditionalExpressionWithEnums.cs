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
  public class IssueJira0659_GroupByConditionalExpressionWithEnums : AutoBuildTest
  {
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
          .Select(g => new { g.Key, Items = g })
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==ExtendedGender.None));
        Assert.That(genderGroups.Any(g => g.Key==ExtendedGender.Female));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByConditionalOperatorForFlagTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithIntFlags>()
          .GroupBy(x => x.NullableFlags.HasValue ? x.NullableFlags.Value : SomeIntFlags.None)
          .Select(g => new { g.Key, Items = g })
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==SomeIntFlags.None));
        Assert.That(genderGroups.Any(g => g.Key==SomeIntFlags.Flag10));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void DirectGroupByByCondigionalOperatorForLongFlagTest()
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
          .GroupBy(x => x.NullableDateTime.HasValue ? x.NullableDateTime.Value : DateTime.MinValue)
          .Select(g => new { g.Key, Items = g })
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==DateTime.MaxValue));
        Assert.That(genderGroups.Any(g => g.Key==DateTime.Now.Date));
      };
      RunTestInSession(testAction);
    }

    [Test]
    public void IndirectGroupByByConditionalOperatorForIntEnumTest()
    {
      Action<Session> testAction = (session) => {
        var genderGroups = session.Query.All<EntityWithGender>()
          .Select(el=> new {
            Id = el.Id,
            Gender = el.NullableGender.HasValue ? el.NullableGender.Value : Gender.None
          })
          .GroupBy(x => x.Gender)
          .Select(g => new { g.Key, Items = g })
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
    public void IndirectGroupByConditionalOperatorForFlagTest()
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
    public void IndirectGroupByByCondigionalOperatorForLongFlagTest()
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
            DateTime = el.NullableDateTime.HasValue ? el.NullableDateTime.Value : DateTime.MinValue
          })
          .GroupBy(x => x.DateTime)
          .Select(g => new {g.Key, Items = g})
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==DateTime.MinValue));
        Assert.That(genderGroups.Any(g => g.Key==DateTime.Now.Date));
      };
      RunTestInSession(testAction);
    }
    
    [Test]
    public void GroupByConditionalOperatorTest2()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var genderGroups = session.Query.All<EntityWithGender>()
          .GroupBy(x => x.NullableGender.HasValue ? x.NullableGender : Gender.None)
          .Select(g=>g.Key)
          .ToArray();
      }
    }
    
    [Test]
    public void GroupByNullCoalescingOperatorTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var genderGroups = session.Query.All<EntityWithGender>()
          .GroupBy(x => x.NullableGender ?? Gender.None)
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==Gender.None));
        Assert.That(genderGroups.Any(g => g.Key==Gender.Female));
      }
    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForIntEnumTest()
    {

    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForLongEnumTest()
    {

    }

    [Test]
    public void DirectGroupByCoalescingOperatorForFlagTest()
    {

    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForLongFlagTest()
    {

    }

    [Test]
    public void DirectGroupByByCoalescingOperatorForDateTimeTest()
    {

    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForIntEnumTest()
    {

    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForLongEnumTest()
    {

    }

    [Test]
    public void IndirectGroupByCoalescingOperatorForFlagTest()
    {

    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForLongFlagTest()
    {

    }

    [Test]
    public void IndirectGroupByByCoalescingOperatorForDateTimeTest()
    {

    }


    [Test]
    public void GroupByGetValueOrDefaultTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var genderGroups = session.Query.All<EntityWithGender>()
          .GroupBy(x => x.NullableGender.GetValueOrDefault(Gender.None))
          .ToArray();
        Assert.That(genderGroups.Length, Is.EqualTo(2));
        Assert.That(genderGroups.Any(g => g.Key==Gender.None));
        Assert.That(genderGroups.Any(g => g.Key==Gender.Female));
      }
    }

    [Test]
    public void DirectGroupByByGetValueOrDefaultForIntEnumTest()
    {
      
    }

    [Test]
    public void DirectGroupByByGetValueOrDefaultForLongEnumTest()
    {

    }

    [Test]
    public void DirectGroupByGetValueOrDefaultForFlagTest()
    {

    }

    [Test]
    public void DirectGroupByByGetValueOrDefaultForLongFlagTest()
    {

    }

    [Test]
    public void DirectGroupByByGetValueOrDefaultForDateTimeTest()
    {

    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForIntEnumTest()
    {

    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForLongEnumTest()
    {

    }

    [Test]
    public void IndirectGroupByGetValueOrDefaultForFlagTest()
    {

    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForLongFlagTest()
    {

    }

    [Test]
    public void IndirectGroupByByGetValueOrDefaultForDateTimeTest()
    {

    }




    [Test]
    public void ConditionalOperationInSelectTest()
    {
      var a = typeof (Gender).IsAssignableFrom(typeof (int));
      var b = typeof (int).IsAssignableFrom(typeof (Gender));
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var result = session.Query.All<EntityWithGender>().Select(el => new {
          Id = el.Id,
          Gender = el.NullableGender.HasValue ? el.NullableGender : Gender.None
        })
        .GroupBy(el=>el.Gender).ToArray();
      }
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
        new EntityWithGender();
        new EntityWithGender { NullableGender = Gender.Female };
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
