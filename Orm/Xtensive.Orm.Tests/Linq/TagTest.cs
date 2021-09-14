using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using Xtensive.Orm.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Orm.Tests.Linq
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key] 
    public int Id { get; private set; }
  
    [Field]
    public string Name { get; private set; }
  }

  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key] 
    public int Id { get; private set; }

    [Field]
    public string FullName { get; private set; }
    
    [Field]
    public EntitySet<Book> Books { get; private set; }
  }

  [HierarchyRoot]
  public class TagType : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
    [Field(Length = 255, Nullable = false)]
    public string Name { get; set; }
    [Field] public bool Active { get; set; }
  }

  [HierarchyRoot]
  [Index(nameof(Tag.ModifiedOn))]
  [Index(nameof(Tag.RemovedOn))]
  public class Tag : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
    
    [Field(Length = 255, Nullable = false)]
    public string Name { get; set; }
    [Field(Nullable = false)] public TagType Type { get; set; }

    [Field] public string Memo { get; set; }

    [Field] public DateTime? RemovedOn { get; set; }

    [Field(DefaultSqlExpression = "GETUTCDATE()")]
    public DateTime ModifiedOn { get; set; }
    [Field] public bool Active { get; set; }
  }

  [HierarchyRoot()]
  [Index("Name", Unique = true)]
  public class BusinessUnit : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
    
    [Field(Length = 255, Nullable = false)]
    public string Name { get; set; }
    [Field] public bool Active { get; set; }
  }

  public class TagTypePair
  {
    public Tag Tag { get; set; }
    public TagType Type { get; set; }
  }

  public class TagModel
  {
    public string Memo { get; set; }
  }

  [Category("Linq")]
  [TestFixture]
  public class TagTest : AutoBuildTest
  {
    private List<X> all;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      configuration.Types.Register(typeof (Book));
      configuration.Types.Register(typeof (Author));
      configuration.Types.Register(typeof (TagType));
      configuration.Types.Register(typeof (BusinessUnit));
      configuration.Types.Register(typeof (Tag));
      return configuration;
    }
    
    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      CreateSessionAndTransaction();
    }

    [Test]
    public void LatestTagWins()
    {
      var query = Session.Demand().Query.All<X>()
        .Tag("firstTag")
        .OrderBy(x => x.Id)
        .Tag("secondTag");
      
      var queryFormatter = Session.Demand().Services.Demand<QueryFormatter>();
      var queryString = queryFormatter.ToSqlString(query);
      
      Assert.IsFalse(queryString.Contains("/*firstTag*/"));
      Assert.IsTrue(queryString.Contains("/*secondTag*/"));
    }
    
    [Test]
    public void SingleTag()
    {
      var query = Session.Demand().Query.All<X>()
        .Tag("singleTag");
      
      var queryFormatter = Session.Demand().Services.Demand<QueryFormatter>();
      var queryString = queryFormatter.ToSqlString(query);
      
      Assert.IsTrue(queryString.Contains("/*singleTag*/"));
    }
    
    [Test]
    public void TagInJoin()
    {
      var query = Session.Demand().Query.All<Author>()
        .Tag("superCoolTag")
        .Where(author => author.Books.Any(book => book.Name.Equals("something")));
      
      var queryFormatter = Session.Demand().Services.Demand<QueryFormatter>();
      var queryString = queryFormatter.ToSqlString(query);
      
      Assert.IsTrue(queryString.Contains("/*superCoolTag*/"));
    }
    
    [Test]
    public void TagInPredicateJoin()
    {
      var realSession = Session.Demand();

      var tagLookup = (
          from tag in realSession.Query.All<Tag>().Tag("BU0001")
          from tagType in realSession.Query.All<TagType>().Tag("BU0002")
            .Where(tagType => tagType == tag.Type && tagType.Active == true)
          where realSession.Query.All<BusinessUnit>().Tag("BU0003")
            .Any(bu => bu.Active == true && bu.Active == tag.Active)
          select new TagTypePair { Tag = tag, Type = tagType })
        .Select(pair => new TagModel { Memo = pair.Tag.Memo });

      var queryFormatter = Session.Demand().Services.Demand<QueryFormatter>();
      var queryString = queryFormatter.ToSqlString(tagLookup);
      
      // Currently we don't enforce which tag should be in resulting query
      // when there are many of them in sqlexpression tree
      Assert.IsTrue(queryString.Contains("/*BU000"));
    }
  }
}