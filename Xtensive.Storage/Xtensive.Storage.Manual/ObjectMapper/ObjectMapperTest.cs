// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.28

using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core.ObjectMapping;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.ObjectMapper
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class User : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get;  set; }

    [Field]
    public EntitySet<WebPage> FavoritePages { get; private set; }

    [Field]
    [Association(
      OnOwnerRemove = OnRemoveAction.Cascade,
      OnTargetRemove = OnRemoveAction.Clear)]
    public WebPage PersonalPage { get; set; }

    [Field]
    [Association(PairTo = "Author")]
    public EntitySet<BlogPost> BlogPosts { get; private set; }

    [Field]
    [Association(PairTo = "Friends")]
    public EntitySet<User> Friends { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class WebPage : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Title { get; set; }

    [Field(Length = 200)]
    public string Url { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class BlogPost : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 100)]
    public string Title { get; set; }

    [Field(Length = 1000)]
    public string Content { get; set; }

    [Field]
    public User Author { get; set;}
  }

  #endregion

  #region DTO

  [Serializable]
  public abstract class IdentifiableDto
  {
    public string Key {get; set; }
  }

  [Serializable]
  public class UserDto : IdentifiableDto
  {
    public string Name { get;  set; }

    public List<WebPageDto> FavoritePages { get; set; }

    public WebPageDto PersonalPage { get; set; }

    public BlogPostDto[] BlogPosts { get; private set; }

    public HashSet<UserDto> Friends { get; private set; }
  }

  public class WebPageDto : IdentifiableDto
  {
    public string Title { get; set; }

    public string Url { get; set; }
  }

  public class BlogPostDto : IdentifiableDto
  {
    public string Title { get; set; }

    public string Content { get; set; }

    public UserDto Author { get; set;}
  }

  #endregion

  [TestFixture]
  public sealed class ObjectMapperTest
  {
    [Test]
    public void MainTest()
    {
      // The key type must be string
      var mapping = new MappingBuilder()
        .MapType<Entity, IdentifiableDto, string>(m => m.Key.Format(), d => d.Key)
        .Inherit<IdentifiableDto, User, UserDto>()
        .Inherit<IdentifiableDto, WebPage, WebPageDto>()
        .Inherit<IdentifiableDto, BlogPost, BlogPostDto>().Build();

      var domain = BuildDomain();
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        var user = new User();

        var firstPost = new BlogPost {Title = "First post"};
        user.BlogPosts.Add(firstPost);

        Assert.AreEqual(user, firstPost.Author);

        var secondPost = new BlogPost {Title = "Second post"};
        secondPost.Author = user;

        Assert.IsTrue(user.BlogPosts.Contains(secondPost));

        user.BlogPosts.Remove(secondPost);

        Assert.IsNull(secondPost.Author);
      }
    }

    private static Domain BuildDomain()
    {
      var config = new DomainConfiguration("sqlserver://localhost/DO40-Tests") {
        UpgradeMode = DomainUpgradeMode.Recreate
      };
      config.Types.Register(typeof(User).Assembly, typeof(User).Namespace);
      return Domain.Build(config);
    }
  }
}