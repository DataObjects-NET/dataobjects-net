// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.07.22

#if NET40
using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0232_SupportForEnumHasFlageModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0232_SupportForEnumHasFlageModel
{
  [Flags]
  public enum PenColorsLong : long
  {
    White = 0,
    Red = 1,
    Green = 2,
    Blue = 4,
    Yellow = 8,
  }
  [Flags]
  public enum PenColors
  {
    White = 0,
    Red = 1,
    Green = 2,
    Blue = 4,
    Yellow = 8,
  }

  [HierarchyRoot]
  public class Preference : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public PenColors FavoritePanColores { get; set; }
  }

  [HierarchyRoot]
  public class PreferenceLong : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public PenColorsLong FavoritePanColorses { get; set; }
  }

  [HierarchyRoot]
  public class Pen : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public PenColors Color { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0232_SupportForEnumHasFlag : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Preference).Assembly, typeof (Preference).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession()) 
      using (var transaction = session.OpenTransaction()) {
        for(int i = 0; i < 9; i++) {
          new Preference {FavoritePanColores = (PenColors) i};
          new PreferenceLong {FavoritePanColorses = (PenColorsLong) i};
        }
        new Pen {Color = PenColors.Red};
        new Pen {Color = PenColors.Blue};
        new Pen {Color = PenColors.Red | PenColors.Blue};
        new Pen {Color = PenColors.Red | PenColors.Green};
        transaction.Complete();
      }
    }

    [Test]
    public void WhereSimplePenColorIntermediate()
    {
      using (var session = Domain.OpenSession()) 
      using (var transaction = session.OpenTransaction()) {
        var expected = PenColors.White;

        var result = from a in Query.All<Preference>()
          let v = PenColors.White
          where a.FavoritePanColores.HasFlag(v)
          select a;

        Assert.That(result.First().FavoritePanColores,Is.EqualTo(expected));
      }
    }

    [Test]
    public void WhereSimplePenColorDirectly()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var expected = PenColors.Blue;

        var result = from a in Query.All<Preference>()
          where a.FavoritePanColores.HasFlag(PenColors.Blue)
          select a;

        Assert.That(result.First().FavoritePanColores, Is.EqualTo(expected));
      }
    }

    [Test]
    public void WhenComplexPenColorIntermediate()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var expected = PenColors.Blue | PenColors.Green | PenColors.Red;

        var result = from a in Query.All<Preference>()
          let v = PenColors.Blue | PenColors.Green | PenColors.Red
          where a.FavoritePanColores.HasFlag(v)
          select a;

        Assert.That(result.First().FavoritePanColores, Is.EqualTo(expected));
      }
    }

    [Test]
    public void WhenComplexPenColorDirectly()
    {
      using (var session = Domain.OpenSession()) 
      using (var transaction = session.OpenTransaction()) {
        var expected = PenColors.Blue | PenColors.Green | PenColors.Red;

        var result = from a in Query.All<Preference>()
          where a.FavoritePanColores.HasFlag(PenColors.Blue | PenColors.Green | PenColors.Red)
          select a;

        Assert.That(result.First().FavoritePanColores, Is.EqualTo(expected));
      }
    }

    [Test]
    public void WhereSimplePenColorLongDirectly()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var expected = PenColorsLong.White;
        
        var result = from a in Query.All<PreferenceLong>()
            where a.FavoritePanColorses.HasFlag(PenColorsLong.White)
            select a;
          
        Assert.That(result.First().FavoritePanColorses,Is.EqualTo(expected));
      }
    }

    [Test]
    public void WhereSimplePenColorLongIntermediate()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var expected = PenColorsLong.White;

        var result = from a in Query.All<PreferenceLong>()
          let v = PenColorsLong.White
          where a.FavoritePanColorses.HasFlag(v)
          select a;

          Assert.That(result.First().FavoritePanColorses, Is.EqualTo(expected));
      }
    }

    [Test]
    public void WhereComplexPenColorLongDirectly()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var expected = PenColorsLong.White | PenColorsLong.Blue;

        var result = from a in Query.All<PreferenceLong>()
          where a.FavoritePanColorses.HasFlag(PenColorsLong.White | PenColorsLong.Blue)
          select a;

        Assert.That(result.First().FavoritePanColorses, Is.EqualTo(expected));
      }
    }

    [Test]
    public void WhereComplexPenColorLongIntermediate()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var expected = PenColorsLong.White | PenColorsLong.Blue;

        var result = from a in Query.All<PreferenceLong>()
          let v = PenColorsLong.White | PenColorsLong.Blue
          where a.FavoritePanColorses.HasFlag(v)
          select a;

        Assert.That(result.First().FavoritePanColorses, Is.EqualTo(expected));
      }
    }

    [Test]
    public void SimpleWithLeftSidePenColorIntermediate()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var expected = PenColors.Red;

        var s = from p in Query.All<Pen>()
          let v = PenColors.Red
          where p.Color.HasFlag(v)==(from pref in Query.All<Preference>()
            where pref.FavoritePanColores.HasFlag(v)
            select pref).First().FavoritePanColores.HasFlag(v)
          select p;
          
        Assert.That(s.First().Color,Is.EqualTo(expected));
      }
    }
  }
}
#endif