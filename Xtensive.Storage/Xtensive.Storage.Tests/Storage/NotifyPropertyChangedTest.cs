// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.10.02

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Tests.Storage.StructureModel;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class  NotifyPropertyChangedTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config =  base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Ray).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using(var t = Transaction.Open()) {
          Ray ray = new Ray();
          ray.PropertyChanged += ray_PropertyChanged;
          ray.Vertex.PropertyChanged += ray_PropertyChanged;
          ray.Direction = Direction.Negative;
          ray.Vertex.X = 5;
          ray.Vertex = new Point(4, 6);
        }
      }
    }

    void ray_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      Console.WriteLine(sender + " " + e.PropertyName);
    }
  }
}