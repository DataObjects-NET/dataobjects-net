// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2008.08.21

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Aspects;
using Xtensive.Orm.Configuration;
using Xtensive.Orm;

namespace Xtensive.Orm.Tests.Storage.AspectsTest
{
  public class AspectsTest : AutoBuildTest
  {
    [Serializable]
    [HierarchyRoot]
    public class BusinessObject : Entity
    {
      [Field, Key]
      public int ID { get; private set; }

      public void PublicMethod(Action<BusinessObject> callback)
      {        
        callback.Invoke(this);
      }

      [Infrastructure]
      public void InfrastructureMethod(Action<BusinessObject> callback)
      {
        callback.Invoke(this);
      }      
      
      [Infrastructure]
      public void CallPrivateMethod(Action<BusinessObject> callback)
      {
        PrivateMethod(callback);
      }

      private void PrivateMethod(Action<BusinessObject> callback)
      {
        callback.Invoke(this);
      }

      [Infrastructure]
      public void CallProtectedMethod(Action<BusinessObject> callback)
      {
        ProtectedMethod(callback);
      }

      protected void ProtectedMethod(Action<BusinessObject> callback)
      {
        callback.Invoke(this);
      }

      [Infrastructure]
      public void CallInternalMethod(Action<BusinessObject> callback)
      {
        InternalMethod(callback);
      }

      internal void InternalMethod(Action<BusinessObject> callback)
      {
        callback.Invoke(this);
      }
    }

    [Serializable]
    [HierarchyRoot]
    public class MasterEntity : Entity
    {
      [Field, Key]
      public Guid ID { get; private set; }

      [Field]
      public EntitySet<SlaveEntity> Slaves { get; private set; }

      [Field]
      public SlaveEntity PrimarySlave { get; set; }
    }

    [Serializable]
    [HierarchyRoot]
    public class SlaveEntity : Entity
    {
      [Field, Key]
      public Guid ID { get; private set; }

      [Field, Association(PairTo = "Slaves")]
      public MasterEntity Master { get; set; }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.AspectsTest");
      return config;
    }
    
    [Test]
    public void TransactionalAspectTest()
    {
      using (var session = Domain.OpenSession()) {
        BusinessObject obj;
        using (session.OpenTransaction()) {
          obj = new BusinessObject();
        }
        using (session.OpenTransaction()) {
          obj.PublicMethod(
            o => Assert.IsNotNull(o.Session.Transaction));
        }

        obj.InfrastructureMethod(
          o => Assert.IsNull(o.Session.Transaction));

        obj.CallInternalMethod(
          o => Assert.IsNull(o.Session.Transaction));

        obj.CallProtectedMethod(
          o => Assert.IsNull(o.Session.Transaction));

        obj.CallPrivateMethod(
          o => Assert.IsNull(o.Session.Transaction));
      }
    }

    [Test]
    public void AspectsPriorityTest()
    {            
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var obj = new BusinessObject();

          using (Domain.OpenSession()) {
            var session2 = Session.Current;
            Assert.AreNotEqual(obj.Session, session2);

            // Check that transaction will be started in obj.Session, but not in second session.

            using (Session.Deactivate()) { // Prevents Session switching check error
              obj.PublicMethod(
                o => Assert.IsNotNull(o.Session.Transaction));

              obj.PublicMethod(
                o => Assert.IsNull(session2.Transaction));
            }
          }
        }
      }      
    }

    [Test]
    public void MasterSlave()
    {
      using (var session = Domain.OpenSession()) {
        using (var tran = session.OpenTransaction()) {
          var master = new MasterEntity();
          var slave1 = new SlaveEntity();
          slave1.Master = master;

          var slave2 = new SlaveEntity();
          slave2.Master = master;
          master.PrimarySlave = slave2;

          var slave3 = new SlaveEntity();
          slave3.Master = master;

          foreach (SlaveEntity slave in master.Slaves) {
            Assert.IsTrue(slave.Master == master);
          }
        }
      }
    }
  }
}