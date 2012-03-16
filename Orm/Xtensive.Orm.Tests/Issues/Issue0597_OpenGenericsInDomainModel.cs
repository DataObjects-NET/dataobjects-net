// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.08

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0597_OpenGenericsInDomainModel_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0597_OpenGenericsInDomainModel_Model
  {
    public abstract class MediaType : Entity
    {
      public MediaType()
      {
        ID = Guid.NewGuid();
      }

      [Field, Key]
      public Guid ID { get; private set; }
    }

    [HierarchyRoot]
    public class VirtualMedia : MediaType
    {
      public VirtualMedia() : base() { }
    }

    public class VirtualMediaDescendant : VirtualMedia
    {
      public VirtualMediaDescendant() : base() { }
    }

    [HierarchyRoot]
    public class ConcreteMedia : MediaType
    {
      public ConcreteMedia() : base() { }
    }

    public abstract class MediaBase : Entity
    {
      public MediaBase()
      {
        ID = Guid.NewGuid();
      }

      [Field, Key]
      public Guid ID { get; private set; }

      [Field]
      public abstract Int64 Duration { get; }
    }

    [HierarchyRoot]
    public class WrongMediaItemBase<MT> : MediaBase 
    {
      public override long Duration
      {
        get { throw new NotImplementedException(); }
      }

      public WrongMediaItemBase() : base() { }
    }

    public abstract class MediaItemBase<MT> : MediaBase 
      where MT : MediaType
    {
      public MediaItemBase() : base() { }
    }

    public abstract class MediaItem<MT, OT> : MediaItemBase<MT> 
      where MT : MediaType
    {

      protected MediaItem() : base() { }

      protected MediaItem(IList<OT> outputTracks) : this() { }
    }

    public abstract class OutputTrack : Entity
    {
      protected OutputTrack() { }

      protected OutputTrack(Int32 trackDefinition)
      {
        TrackDefinition = trackDefinition;
      }

      [Field]
      public Int32 TrackDefinition { get; private set; }
    }

    [HierarchyRoot]
    public class SimpleOutputTrack : OutputTrack
    {
      public SimpleOutputTrack() { }

      public SimpleOutputTrack(Int32 trackDefinition)
        : base(trackDefinition) { }

      [Field, Key]
      public Guid ID { get; private set; }
    }

    public abstract class SimpleMedia<MT> : MediaItem<MT, SimpleOutputTrack>
      where MT : MediaType
    {
      private Int64 _duration = 0;

      public SimpleMedia() { }

      public SimpleMedia(Int64 duration, IList<SimpleOutputTrack> outputTracks)
        : base(outputTracks)
      {
        _duration = duration;
      }

      public override Int64 Duration { get { return _duration; } }
    }

    [HierarchyRoot]
    public class SimpleVirtualMedia : SimpleMedia<VirtualMedia>
    {
      public SimpleVirtualMedia() { }

      public SimpleVirtualMedia(Int64 duration, IList<SimpleOutputTrack> outputTracks)
        : base(duration, outputTracks) { }
    }
  }

  [Serializable]
  public class Issue0597_OpenGenericsInDomainModel : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(MediaBase).Assembly, typeof(MediaBase).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Assert.IsFalse(Domain.Model.Types.Contains(typeof(WrongMediaItemBase<VirtualMedia>)));
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
      }
    }
  }
}