// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2012.01.25

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0232_SupportForEnumHasFlageModel;
#if NET40
namespace Xtensive.Orm.Tests.Issues.IssueJira0232_SupportForEnumHasFlageModel
{
  [Flags]
  public enum PenColorsLong : long
  {
    White   = 0,
    Red     = 1,
    Green   = 2,
    Blue    = 4,
    Yellow  = 8,
  }

  public enum PenColors
  {
    White   = 0,
    Red     = 1,
    Green   = 2,
    Blue    = 4,
    Yellow  = 8,
  }
  
  [HierarchyRoot]
  public class Preference : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public PenColors FavoritePanColorses { get; set; }
  }

  public class PreferenceLong : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public PenColorsLong FavoritePanColorses { get; set; }
  }

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
      using (Domain.OpenSession()) {
        using (var transaction = Session.Current.OpenTransaction()) {
          for(int i = 0; i < 9; i++) {
            new Preference {FavoritePanColorses = (PenColors) i};
          }
          new Pen { Color = PenColors.Red };
          new Pen { Color = PenColors.Blue };
          new Pen { Color = PenColors.Red|PenColors.Blue };
          new Pen { Color = PenColors.Red|PenColors.Green };
          transaction.Complete();
        }
      }
    }

    [Test]
    public void WhereSimplePenColorIntermediate()
    {
      using (Domain.OpenSession()) {
        using(var transaction = Session.Current.OpenTransaction()) {

          var expected = PenColors.Blue;

          var result = from a in Query.All<Preference>()
            let v = PenColors.Blue
            where a.FavoritePanColorses.HasFlag(v)
            select a;

          Assert.That(result.First(),Is.EqualTo(expected));
        }
      }
    }

    [Test]
    public void WhereSimplePenColorDirectly()
    {
      using (Domain.OpenSession()) {
        using (var transaction = Session.Current.OpenTransaction()) {

          var expected = PenColors.Blue;

          var result = from a in Query.All<Preference>()
                       where a.FavoritePanColorses.HasFlag(PenColors.Blue)
                       select a;

          Assert.That(result.First().FavoritePanColorses, Is.EqualTo(expected));
        }
      }
    }

    [Test]
    public void WhenComplexPenColorIntermediate()
    {
      using (Domain.OpenSession()) {
        using (var transaction = Session.Current.OpenTransaction()) {

          var expected = PenColors.Blue|PenColors.Green|PenColors.Red;

          var result = from a in Query.All<Preference>()
                       let v = PenColors.Blue | PenColors.Green | PenColors.Red
                       where a.FavoritePanColorses.HasFlag(v)
                       select a;

          Assert.That(result.First().FavoritePanColorses, Is.EqualTo(expected));
        }
      }
    }

    [Test]
    public void WhenComplexPenColorDirectly()
    {
      using (Domain.OpenSession()) {
        using (var transaction = Session.Current.OpenTransaction()) {

          var expected = PenColors.Blue | PenColors.Green | PenColors.Red;

          var result = from a in Query.All<Preference>()
                       where a.FavoritePanColorses.HasFlag(PenColors.Blue | PenColors.Green | PenColors.Red)
                       select a;

          Assert.That(result.First().FavoritePanColorses, Is.EqualTo(expected));
        }
      }
    }

    [Test]
    public void WhereSimplePenColorLongDirectly()
    {
      using(Domain.OpenSession()) {
        using (var transaction = Session.Current.OpenTransaction()) {

          var expected = PenColorsLong.White;

          var result = from a in Query.All<PreferenceLong>()
            where a.FavoritePanColorses.HasFlag(PenColorsLong.White)
            select a;
          
          Assert.That(result.First().FavoritePanColorses,Is.EqualTo(expected));
        }
      }
    }

    [Test]
    public void WhereSimplePenColorLongIntermediate()
    {
      using (Domain.OpenSession())
      {
        using (var transaction = Session.Current.OpenTransaction())
        {

          var expected = PenColorsLong.White;

          var result = from a in Query.All<PreferenceLong>()
                       let v = PenColorsLong.White
                       where a.FavoritePanColorses.HasFlag(v)
                       select a;

          Assert.That(result.First().FavoritePanColorses, Is.EqualTo(expected));
        }
      }
    }

    [Test]
    public void WhereComplexPenColorLongDirectly()
    {
      using (Domain.OpenSession())
      {
        using (var transaction = Session.Current.OpenTransaction())
        {

          var expected = PenColorsLong.White|PenColorsLong.Blue;

          var result = from a in Query.All<PreferenceLong>()
                       where a.FavoritePanColorses.HasFlag(PenColorsLong.White|PenColorsLong.Blue)
                       select a;

          Assert.That(result.First().FavoritePanColorses, Is.EqualTo(expected));
        }
      }
    }

    [Test]
    public void WhereComplexPenColorLongIntermediate()
    {
      using (Domain.OpenSession())
      {
        using (var transaction = Session.Current.OpenTransaction())
        {

          var expected = PenColorsLong.White|PenColorsLong.Blue;

          var result = from a in Query.All<PreferenceLong>()
                       let v = PenColorsLong.White|PenColorsLong.Blue
                       where a.FavoritePanColorses.HasFlag(v)
                       select a;

          Assert.That(result.First().FavoritePanColorses, Is.EqualTo(expected));
        }
      }
    }
  }
}
#endif