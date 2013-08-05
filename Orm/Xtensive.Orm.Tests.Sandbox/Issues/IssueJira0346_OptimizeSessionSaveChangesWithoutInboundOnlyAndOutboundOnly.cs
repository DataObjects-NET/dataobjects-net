// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.07.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0346_OptimizeSessionSaveChangesWithoutInboundOnlyAndOutboundOnlyModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0346_OptimizeSessionSaveChangesWithoutInboundOnlyAndOutboundOnlyModel
{
  [HierarchyRoot]
  public class User : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Login { get; set; }

    [Field]
    public string Password { get; set; }

    [Field]
    public UserInfo UserInfo { get; set; }

    [Field]
    [Association(PairTo = "User")]
    public EntitySet<AssignedRole> AssignedRoles { get; set; }

    [Field]
    public EntitySet<Group> Groups { get; set; }

    [Field]
    public UserState UserState { get; set; }
  }

  [HierarchyRoot]
  public class UserInfo : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    [Field]
    [Association(PairTo = "UserInfo")]
    public User User { get; set; }
  }

  [HierarchyRoot]
  public class Role : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public string RoleName { get; set; }

    [Field]
    public EntitySet<AssignedRole> AssignedRoles { get; set; } 
  }

  [HierarchyRoot]
  public class AssignedRole : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    [Association(PairTo = "AssignedRoles")]
    public Role Role { get; set; }

    [Field(Nullable = false)]
    public User User { get; set; }
  }

  [HierarchyRoot]
  public class Group : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Groups")]
    public EntitySet<User> Users { get; set; }
  }

  [HierarchyRoot]
  public class UserState : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0346_OptimizeSessionSaveChangesWithoutInboundOnlyAndOutboundOnly : AutoBuildTest
  {
    private List<Role> roles;
    private List<User> users;
    private List<UserState> userStates; 
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (User).Assembly, typeof (User).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      roles = new List<Role>();
      users = new List<User>();
      userStates = new List<UserState>();

      using(var session = Domain.OpenSession())
      using(var transaction = session.OpenTransaction()) {
        roles.Add(new Role {RoleName = "Admin"});
        roles.Add(new Role {RoleName = "User"});
        roles.Add(new Role {RoleName = "Moderator"});

        userStates.Add(new UserState { Name = "Activated"});
        userStates.Add(new UserState { Name = "NotActivated" });
        userStates.Add(new UserState { Name = "Blocked" });

        users.Add(new User {
          Login = "walli98",
          Password = "1", 
          UserInfo = new UserInfo {
            FirstName = "Иван",
            LastName = "Бобышкин"
          },
          UserState = userStates.Find(el=>el.Name=="Activated")
        });
        users.Add(new User {
          Login = "visa43",
          Password = "1",
          UserInfo = new UserInfo {
            FirstName = "Иван",
            LastName = "Похлебкин"
          },
          UserState = userStates.Find(el=>el.Name=="Blocked")
        });
        users.Add(new User {
          Login = "csi90210",
          Password = "1",
          UserInfo = new UserInfo {
            FirstName = "Алексей",
            LastName = "Алексеев"
          },
          UserState = userStates.Find(el => el.Name == "Activated")
        });

        new AssignedRole {
          User = users.Find(el => el.Login=="walli98"),
          Role = roles.Find(el => el.RoleName=="Admin")
        };
        new AssignedRole {
          User = users.Find(el => el.Login == "visa43"),
          Role = roles.Find(el => el.RoleName == "User")
        };
        new AssignedRole {
          User = users.Find(el => el.Login == "csi90210"),
          Role = roles.Find(el => el.RoleName == "Moderator")
        };

        var group = new Group {
          Name = "Closed for everyone"
        };
        group = new Group {
          Name = "Admin's Group"
        };
        group.Users.Add(users.Find(el => el.Login == "walli98"));
        group = new Group {
          Name = "Moderator's and Admin's Group"
        };
        group.Users.Add(users.Find(el => el.Login=="walli98"));
        group.Users.Add(users.Find(el => el.Login=="walli98"));
        transaction.Complete();
      }
    }

    [Test]
    public void SelectTest()
    {
      using(var session = Domain.OpenSession())
      using(var transaction = session.OpenTransaction()) {
        var result = from u in session.Query.All<User>()
          join ui in session.Query.All<UserInfo>() on u.UserInfo.Id equals ui.Id
          where u.Login=="visa43"
          select u;

        Assert.That(result.First().Login, Is.EqualTo("visa43"));
      }
    }
  }
}
