// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.27

using System;
using NUnit.Framework;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.IssueJira0196_GuidAsTypeDiscriminatorModel;

namespace Xtensive.Storage.Tests.Issues.IssueJira0196_GuidAsTypeDiscriminatorModel
{
  [HierarchyRoot, TypeDiscriminatorValue("8F68958C-E7F5-4CC2-BCFD-B0F24426191F")]
  public class User : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field, TypeDiscriminator]
    public Guid UserType { get; set; }
  }

  [TypeDiscriminatorValue("FDC50B92-8150-42F1-9AAD-17B36DBFD55C")]
  public class AdminUser : User
  {
  }

  [TypeDiscriminatorValue("4118F961-8CFA-4C16-BCC6-B1A8662FCE31", Default = true)]
  public class GuestUser : User
  {
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class IssueJira0196_GuidAsTypeDiscriminator : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(User).Assembly, typeof(User).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          CheckUserType(new AdminUser());
          CheckUserType(new GuestUser());
          CheckUserType(new User());
          t.Complete();
          // Rollback
        }
      }
    }

    private void CheckUserType(User user)
    {
      var expected = new Guid(user.GetType().GetAttribute<TypeDiscriminatorValueAttribute>().Value.ToString());
      Assert.AreEqual(expected, user.UserType);
    }
  }
}