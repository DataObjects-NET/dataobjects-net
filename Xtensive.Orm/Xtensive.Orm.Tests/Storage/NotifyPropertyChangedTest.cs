// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.02

using System;
using System.ComponentModel;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests.Storage.StructureModel;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class  NotifyPropertyChangedTest : AutoBuildTest
  {
    private static bool isEntityPropertyEventRaised;
    private static bool isStructureEventRaised;
    private static bool isStructurePropertyEventRaised;
    
    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config =  base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Ray).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Ray ray = new Ray();
          Reset();
          ray.PropertyChanged += ray_PropertyChanged;
          ray.Vertex.PropertyChanged += ray_PropertyChanged;
          Assert.IsFalse(isEntityPropertyEventRaised);
          Assert.IsFalse(isStructureEventRaised);
          Assert.IsFalse(isStructurePropertyEventRaised);

          Reset();
          ray.Direction = Direction.Negative;
          Assert.IsTrue(isEntityPropertyEventRaised);
          Assert.IsFalse(isStructureEventRaised);
          Assert.IsFalse(isStructurePropertyEventRaised);

          Reset();
          ray.Vertex.X = 5;
          Assert.IsFalse(isEntityPropertyEventRaised);
          Assert.IsTrue(isStructureEventRaised);
          Assert.IsTrue(isStructurePropertyEventRaised);

          Reset();
          ray.Vertex = new Point(4, 6);
          Assert.IsFalse(isEntityPropertyEventRaised);
          Assert.IsTrue(isStructureEventRaised);
          Assert.IsFalse(isStructurePropertyEventRaised);
        }
      }
    }

    private static void Reset()
    {
      isEntityPropertyEventRaised = false;
      isStructureEventRaised = false;
      isStructurePropertyEventRaised = false;
    }

    private void ray_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      Console.WriteLine(sender.GetType().Name + "." + e.PropertyName);
      if (sender.GetType() == typeof(Ray) && e.PropertyName == "Direction")
        isEntityPropertyEventRaised = true;
      if (sender.GetType() == typeof(Ray) && e.PropertyName == "Vertex")
        isStructureEventRaised = true;
      if (sender.GetType() == typeof(Point) && e.PropertyName == "X")
        isStructurePropertyEventRaised = true;
    }
  }
}