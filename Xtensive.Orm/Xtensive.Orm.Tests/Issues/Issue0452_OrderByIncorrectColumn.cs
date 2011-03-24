// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.26

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.Issues.Issue0452_OrderByIncorrectColumn_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0452_OrderByIncorrectColumn_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Image : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class SitePage : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
    [Field]
    public string Url { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Category : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public abstract class AbstractObject : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }

    [Field]
    public DateTime CreatedOn{ get; set;}

    [Field]
    public Person CreatedBy{ get; set;}
  }

  [Serializable]
  public abstract class BlogPost : AbstractObject
  {
    [Field]
    public Category Category{ get; set;}

    [Field]
    public string Title { get; set; }

    [Field]
    public DateTime? PublishedOn { get; set; }

    [Field]
    public Image TeaserImage { get; set; }

    [Field]
    public SitePage SitePage { get; set; }
  }

  [Serializable]
  public sealed class Article : BlogPost
  {
  }

  [Serializable]
  public sealed class Article2 : BlogPost
  {
    
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0452_OrderByIncorrectColumn : AutoBuildTest
  {
    private const int Count = 100;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (BlogPost).Assembly, typeof (BlogPost).Namespace);
      return config;
    }

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Fill();
          t.Complete();
        }
      }
    }

    [Test]
    public void TakeTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var result = session.Query.All<Article>().OrderByDescending(a => a.PublishedOn).Take(Count - 1).ToList();
          var expected = session.Query.All<Article>().AsEnumerable().OrderByDescending(a => a.PublishedOn).Take(Count - 1).ToList();
          Assert.IsTrue(expected.Count>0);
          Assert.AreEqual(expected.Count, result.Count);
          for (int i = 0; i < expected.Count; i++) {
            var areMatch = Equals(expected[i], result[i]);
            Assert.IsTrue(areMatch);
          }
          Assert.IsTrue(expected.SequenceEqual(result));
        }
      }
    }

    [Test]
    public void SkipTakeTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var result = session.Query.All<Article>().OrderByDescending(a => a.PublishedOn).Skip(5).Take(Count - 10).ToList();
          var expected = session.Query.All<Article>().AsEnumerable().OrderByDescending(a => a.PublishedOn).Skip(5).Take(Count - 10).ToList();
          Assert.IsTrue(expected.Count>0);
          Assert.AreEqual(expected.Count, result.Count);
          for (int i = 0; i < expected.Count; i++) {
            bool areMatch = Equals(expected[i], result[i]);
            Assert.IsTrue(areMatch);
          }
          Assert.IsTrue(expected.SequenceEqual(result));
        }
      }
    }

    [Test]
    public void SkipTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var result = session.Query.All<Article>().OrderByDescending(a => a.PublishedOn).Skip(5).ToList();
          var expected = session.Query.All<Article>().AsEnumerable().OrderByDescending(a => a.PublishedOn).Skip(5).ToList();
          Assert.AreEqual(expected.Count, result.Count);
          Assert.IsTrue(expected.Count>0);
          for (int i = 0; i < expected.Count; i++) {
            bool areMatch = Equals(expected[i], result[i]);
            Assert.IsTrue(areMatch);
          }
          Assert.IsTrue(expected.SequenceEqual(result));
        }
      }
    }

    [Test]
    public void SelectCategoryTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var result = session.Query.All<Article>().Select(p => p.Category);
          foreach (var category in result) {
            Assert.IsNotNull(category);
          }
        }
      }
    }

    [Test]
    public void QuerySingleTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var a1 = session.Query.All<Article>().First();
        var id = a1.Id;

        var a2 = session.Query.SingleOrDefault<Article2>(id);
        Assert.IsNull(a2);
      }
    }

    [Test]
    public void QuerySingleOrDefaultTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var url = "http://localhost/page_1.htm";
        var result = session.Query.All<BlogPost>().Where(p => p.SitePage.Url == url).SingleOrDefault();
        Assert.IsNotNull(result);
      }
    }

    private void Fill()
    {
      for (int i = 0; i < Count; i++) {
        var person = new Person();
        var category = new Category();
        var sitePage = new SitePage() {
          Url = string.Format("http://localhost/page_{0}.htm", i)
        };
        var image = new Image();
        var article = new Article {
          Category = category,
          CreatedBy = person,
          CreatedOn =  new DateTime(1800 + i, 1, 1),
          PublishedOn = new DateTime(1800 + (i % 2) * Count + i, 1, 1),
          SitePage = sitePage,
          Title = string.Format("Article_{0}", i),
          TeaserImage = image
        };
      }
      Session.Current.SaveChanges();
    }
  }
}