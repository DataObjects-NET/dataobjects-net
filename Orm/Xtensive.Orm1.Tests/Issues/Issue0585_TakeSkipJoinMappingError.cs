// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2010.01.19

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.Xtensive.Storage.Tests.Issues.Issue0585_TakeSkipJoinMappingError_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Xtensive.Storage.Tests.Issues.Issue0585_TakeSkipJoinMappingError_Model
  {
    // [Index("Name", Unique = true)]
    // [Index("UniqueIndentifier", Unique = true)]
    [HierarchyRoot]
    public class User : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Guid? UniqueIndentifier { get; set; }

      [Field(Length = 400)]
      public string Name { get; set; }

      [Field(Length = 400)]
      public string Email { get; set; }

      [Field(Length = 400)]
      public string Password { get; set; }

      [Field(Length = 400)]
      public string AlternativePassword { get; set; }

      [Field(Length = 400)]
      public string PasswordQuestion { get; set; }

      [Field(Length = 400)]
      public string PasswordAnswer { get; set; }

      [Field, Association(OnTargetRemove = OnRemoveAction.Clear, PairTo = "Users")]
      public EntitySet<Role> Roles { get; private set; }
    }

    [HierarchyRoot]
    public class UserActivity
      : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public DateTime CreationDate { get; set; }

      [Field]
      public DateTime? LastLoginAttemptDate { get; set; }

      [Field]
      public int LoginAttemptCount { get; set; }

      [Field]
      public DateTime LastPasswordChangeDate { get; set; }

      [Field]
      public DateTime LastLockoutDate { get; set; }

      [Field, Association(OnTargetRemove = OnRemoveAction.Cascade)]
      public User User { get; set; }

      [Field]
      public bool IsApproved { get; set; }

      [Field]
      public bool IsLockedOut { get; set; }

      [Field]
      public string Comment { get; set; }

      [Field]
      public DateTime LastLoginDate { get; set; }

      [Field]
      public DateTime LastActivityDate { get; set; }

      public static UserActivity GetOrCreate(User user)
      {
        var activity = user.Session.Query.All<UserActivity>().Where(ua => ua.User==user).FirstOrDefault();
        if (activity==null)
          activity = new UserActivity {User = user};
        return activity;
      }
    }


    // [Index("Name", Unique = true)]
    [HierarchyRoot]
    public class Role : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<User> Users { get; private set; }

      [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Role> Roles { get; private set; }
    }
  }

  [Serializable]
  public class Issue0585_TakeSkipJoinMappingError : AutoBuildTest
  {
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

    private void Fill()
    {
      for (int i = 0; i < 10; i++) {
        var user = new User {
          Name = string.Format("name_{0}", i), 
          Password = string.Format("password_{0}", i), 
          PasswordQuestion = string.Format("passwordQuestion_{0}", i), 
          Email = string.Format("email{0}", i)
        };
        for (int j = 0; j < 10; j++) {
          var activity = new UserActivity {
            Comment = string.Format("comment_{0}_{1}", i, j), 
            IsApproved = true, 
            IsLockedOut = false, 
            CreationDate = DateTime.Now, 
            LastLoginDate = DateTime.Now, 
            LastActivityDate = DateTime.Now, 
            LastPasswordChangeDate = DateTime.Now, 
            LastLockoutDate = DateTime.Now,
            User = user
          };
        }
      }
      Session.Current.SaveChanges();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (UserActivity).Assembly, typeof (UserActivity).Namespace);
      return config;
    }

    protected override void CheckRequirements()
    {
      base.CheckRequirements();
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          int pageIndex = 1;
          int pageSize = 1;
          IQueryable<User> usersQuery = session.Query.All<User>().Skip(pageIndex * pageSize).Take(pageSize);
          var query =
            from user in usersQuery
            from activity in session.Query.All<UserActivity>().Where(a => a.User==user).DefaultIfEmpty()
            select new {
              user.Name,
              user.UniqueIndentifier,
              user.Email,
              user.PasswordQuestion,
              activity.Comment,
              activity.IsApproved,
              activity.IsLockedOut,
              activity.CreationDate,
              activity.LastLoginDate,
              activity.LastActivityDate,
              activity.LastPasswordChangeDate,
              activity.LastLockoutDate
            };
          var result = query.ToList();
          Assert.Greater(result.Count, 0);
        }
      }
    }
  }
}