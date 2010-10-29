// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.01.19

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue679And681;

namespace Xtensive.Orm.Tests.Issues.Issue679And681
{
  [HierarchyRoot]
  public class Doctor : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string FullName { get; set; }

    [Field]
    public string Email { get; set; }

    [Field]
    [Association(PairTo = "Doctors")]
    public EntitySet<Speciality> Specialities { get; private set; }
  }

  [HierarchyRoot]
  public class Speciality : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string Title { get; set; }

    [Field]
    public bool Activ { get; set; }

    [Field]
    // [Association(PairTo = "Specialities")] // Must lead to an exception!
    public EntitySet<Doctor> Doctors { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue679And681Test : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Doctor).Assembly, typeof (Doctor).Namespace);
      return config;
    }

    [Test]
    public void BuildTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var s1 = new Speciality {
            Title = "Speciality1"
          };
          var d1 = new Doctor {
            FullName = "Doctor1",
            Email = "email1@host.com"
          };
          var d2 = new Doctor {
            FullName = "Doctor2",
            Email = "email2@host.com"
          };
          d1.Specialities.Add(s1);
          d2.Specialities.Add(s1);
          t.Complete();
        }
      }
      Assert.IsTrue(true);
    }

    [Test]
    public void QueryTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var speciality = session.Query.Single<Speciality>(1);
          var list =
            from doctor in speciality.Doctors
            select new {
              speciality.Title,
              doctor.FullName,
              doctor.Email
            };

          foreach (var item in list) {
            Console.WriteLine("{0} {1} {2}", 
              item.Title, item.FullName, item.Email);
          }
          Assert.AreEqual(2, list.Count());
          t.Complete();
        }
      }
    }
  }
}