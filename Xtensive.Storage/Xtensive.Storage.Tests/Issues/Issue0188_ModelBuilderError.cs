// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.12

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0188_ModelBuilderError_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0188_ModelBuilderError_Model
{
  [Serializable]
  [HierarchyRoot]
  public class A : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
    
    [Field]
    public B B { get; set; }
  }

  [Serializable]
  public class B : A
  {
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0188_ModelBuilderError : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (A).Assembly, typeof (A).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          



          // Rollback
        }
      }
    }
  }
}