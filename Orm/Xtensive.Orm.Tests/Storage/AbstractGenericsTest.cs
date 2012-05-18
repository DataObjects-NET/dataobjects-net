// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.03

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Storage.AbstractGenericTest_Model;

namespace Xtensive.Orm.Tests.Storage
{
  namespace AbstractGenericTest_Model
  {
    [HierarchyRoot]
    public abstract class Media : Entity
    {
      [Field,Key]
      public int Id { get; private set; }

      [Field]
      public string Description { get; set; }
    }

    public class SimpleMedia : Media
    {
      [Field]
      public decimal Foo { get; set; }
    }

    public class ComplexMedia : Media
    {
      [Field]
      public DateTime Bar { get; set; }
    }

    [HierarchyRoot]
    public abstract class Track<T> : Entity
      where T : Media
    {
      [Field,Key]
      public int Id { get; private set; }

      [Field]
      public T Media { get; private set; }

      [Field]
      public TimeSpan Duration { get; private set; }

      [Field]
      public TimeSpan StartPosition { get; private set; }

      protected Track(TimeSpan duration, TimeSpan startPosition, T media)
      {
        Duration = duration;
        StartPosition = startPosition;
        Media = media;
      }
    }

    public class SimpleTrack : Track<SimpleMedia>
    {
      public SimpleTrack(TimeSpan duration, TimeSpan startPosition, SimpleMedia media)
        : base(duration, startPosition, media)
      {}
    }

    public class ComplexTrack : Track<ComplexMedia>
    {
      public ComplexTrack(TimeSpan duration, TimeSpan startPosition, ComplexMedia media)
        : base(duration, startPosition, media)
      {}
    }
  }

  public class AbstractGenericsTest
  {
    [Test]
    public void MediaTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Media));
      config.Types.Register(typeof(Track<SimpleMedia>));
      config.Types.Register(typeof(Track<ComplexMedia>));
      config.Types.Register(typeof(SimpleTrack));
      config.Types.Register(typeof(ComplexTrack));
      var domain = Domain.Build(config);
      using (var session = domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new SimpleTrack(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(10), new SimpleMedia() {Description = "Simple", Foo = 23});
        new ComplexTrack(TimeSpan.FromMinutes(6), TimeSpan.FromMinutes(15), new ComplexMedia() {Description = "Simple", Bar = DateTime.Now});
        var listSimpleMedia = session.Query.All<Track<SimpleMedia>>().ToList();
        var listComplexMedia = session.Query.All<Track<ComplexMedia>>().ToList();
        Assert.AreEqual(1,listSimpleMedia.Count);
        Assert.AreEqual(1,listComplexMedia.Count);
      }
    }
  }
}