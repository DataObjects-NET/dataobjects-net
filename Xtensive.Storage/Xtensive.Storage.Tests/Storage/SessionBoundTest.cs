// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.06.25

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Aspects;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.StructureModel;

namespace Xtensive.Storage.Tests.Storage
{
  public class SessionBoundTest : AutoBuildTest
  {

    internal class TestHelper : SessionBound
    {
      public void CheckSessionBoundObjects(Ray x, Ray y, Session parentSession)
      {
        Assert.AreNotEqual(Session.Current, parentSession);
        Assert.AreNotEqual(x.Session, y.Session);

        Assert.AreEqual(Session.Current, x.Session);
        Assert.AreNotEqual(Session.Current, y.Session);

        Assert.AreEqual(x.Direction, y.Direction);
      }

      public TestHelper(Session session)
        : base(session)
      {
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.StructureModel");
      return config;
    }

    [Test]    
    public void Test()
    {      
      using (var scope1 = Domain.OpenSession()) {
        using (Transaction.Open()) {
          Ray ray1 = new Ray();
          var testHelper = new TestHelper(Session.Current);
          Session.Current.Persist();

          using (var scope2 = Domain.OpenSession()) {
            Assert.IsNull(Transaction.Current);

            using (Transaction.Open()) {
              Ray ray2 = new Ray();

              Assert.AreNotEqual(scope1.Session, scope2.Session);
              testHelper.CheckSessionBoundObjects(ray1, ray2, Session.Current);
            }            
          }          
        }        
      }
    }
  }
}