// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.26

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0452_OrderByIncorrectColumn_Model;
using Xtensive.Storage.Tests.Linq;

namespace Xtensive.Storage.Tests.Issues.Issue0452_OrderByIncorrectColumn_Model
{
  [HierarchyRoot]
  public class Image : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  [HierarchyRoot]
  public class SitePage : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  [HierarchyRoot]
  public class Category : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

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

  public sealed class Article : BlogPost
  {
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0452_OrderByIncorrectColumn : AutoBuildTest
  {
    private const int Count = 1000;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (BlogPost).Assembly, typeof (BlogPost).Namespace);
      return config;
    }

    [Test]
    public void TakeTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Fill();
          var result = Query<Article>.All.OrderByDescending(a => a.PublishedOn).Take(Count - 1);
          var expected = Query<Article>.All.AsEnumerable().OrderByDescending(a => a.PublishedOn).Take(Count - 1).ToList();
          Assert.IsTrue(expected.Count>0);
          Assert.IsTrue(expected.SequenceEqual(result));
        }
      }
    }

    [Test]
    public void SkipTakeTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Fill();
          var result = Query<Article>.All.OrderByDescending(a => a.PublishedOn).Skip(5).Take(Count - 10);
          var expected = Query<Article>.All.AsEnumerable().OrderByDescending(a => a.PublishedOn).Skip(5).Take(Count - 10).ToList();
          Assert.IsTrue(expected.Count>0);
          Assert.IsTrue(expected.SequenceEqual(result));
        }
      }
    }

    [Test]
    public void SelectCategoryTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          Fill();
          var result = Query<Article>.All.Select(p => p.Category);
          foreach (var category in result) {
            Assert.IsNotNull(category);
          }
        }
      }
    }

    private void Fill()
    {
        var random = RandomManager.CreateRandom();
      for (int i = 0; i < Count; i++) {
        var person = new Person();
        var category = new Category();
        var sitePage = new SitePage();
        var image = new Image();
        var article = new Article {
          Category = category,
          CreatedBy = person,
          CreatedOn =  new DateTime(1 + Count + i, 1, 1),
          PublishedOn = new DateTime(random.Next(1,1000), 1, 1),
          SitePage = sitePage,
          Title = string.Format("Article_{0}", i),
          TeaserImage = image
        };
      }
      Session.Current.Persist();
    }
  }
}