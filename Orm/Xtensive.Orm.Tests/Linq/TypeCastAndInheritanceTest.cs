// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.04.10

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture, Category("Linq")]
  public class TypeCastAndInheritanceTest : ChinookDOModelTest
  {
    [Test]
    public void InheritanceCountTest()
    {
      var trackCount = Session.Query.All<Track>().Count();
      var videoTrackCount = Session.Query.All<VideoTrack>().Count();
      var audionTrack = Session.Query.All<AudioTrack>().Count();
      Assert.IsTrue(trackCount > 0);
      Assert.IsTrue(videoTrackCount > 0);
      Assert.IsTrue(audionTrack > 0);
      Assert.AreEqual(trackCount, videoTrackCount, audionTrack);
    }

    [Test]
    public void IsSimpleTest()
    {
      var result = Session.Query.All<Track>().Where(t => t is VideoTrack);
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsSameTypeTest()
    {
#pragma warning disable 183
      var result = Session.Query.All<Track>().Where(t => t is Track);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsSubTypeTest()
    {
#pragma warning disable 183
      var result = Session.Query.All<VideoTrack>().Where(t => t is Track);
#pragma warning restore 183
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsIntermediateTest()
    {
      Assert.Throws<QueryTranslationException>( () => {
        Session.Query.All<Track>()
          .Where(t => t is IntermediateTrack)
          .Select(track => (IntermediateTrack) track)
          .Count();
      });
    }

    [Test]
    public void IsCountTest()
    {
      int trackCount = Session.Query.All<Track>().Count();
      int intermediateTrackCount = Session.Query.All<IntermediateTrack>().Count();
      int videoTrackCount = Session.Query.All<VideoTrack>().Count();
      int audionTrackCount = Session.Query.All<AudioTrack>().Count();

      Assert.Greater(trackCount, 0);
      Assert.Greater(intermediateTrackCount, 0);
      Assert.Greater(videoTrackCount, 0);
      Assert.Greater(audionTrackCount, 0);

      Assert.AreEqual(
        trackCount,
        intermediateTrackCount);

      Assert.Throws<QueryTranslationException>(
        () => {
          Assert.AreEqual(
            intermediateTrackCount,
            Session.Query.All<Track>()
              .Where(t => t is IntermediateTrack)
              .Select(track => (IntermediateTrack) track)
              .Count());
        });
      Assert.Throws<QueryTranslationException>(
        () => {
         Assert.AreEqual(
            videoTrackCount,
            Session.Query.All<Track>()
              .Where(t => t is VideoTrack)
              .Select(track => (VideoTrack) track)
              .Count());
        });

      Assert.Throws<QueryTranslationException>(
        () => {
          Assert.AreEqual(
            audionTrackCount,
            Session.Query.All<Track>()
              .Where(t => t is AudioTrack)
              .Select(track => (AudioTrack) track)
              .Count());
        });

#pragma warning disable 183
      Assert.AreEqual(
        trackCount,
        Session.Query.All<Track>()
          .Where(t => t is Track)
          .Count());
#pragma warning restore 183
    }

    [Test]
    public void OfTypeSimpleTest()
    {
      var result = Session.Query.All<Track>().OfType<VideoTrack>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSameTypeTest()
    {
      var result = Session.Query.All<Track>().OfType<Track>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeSubTypeTest()
    {
      var result = Session.Query.All<VideoTrack>().OfType<Track>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeIntermediateTest()
    {
      Session.Query.All<Track>()
        .OfType<IntermediateTrack>()
        .Count();
    }

    [Test]
    public void OfTypeWithFieldAccessTest()
    {
      var result = Session.Query.All<Track>()
        .OfType<AudioTrack>()
        .Select(at => at.Genre);
      QueryDumper.Dump(result);
    }

    [Test]
    public void OfTypeCountTest()
    {
      int trackCount = Session.Query.All<Track>().Count();
      int intermediateTrackCount = Session.Query.All<IntermediateTrack>().Count();
      int videoTrackCount = Session.Query.All<VideoTrack>().Count();
      int audioTrackCount = Session.Query.All<AudioTrack>().Count();

      Assert.Greater(trackCount, 0);
      Assert.Greater(intermediateTrackCount, 0);
      Assert.Greater(videoTrackCount, 0);
      Assert.Greater(audioTrackCount, 0);

      Assert.AreEqual(trackCount, intermediateTrackCount);

      Assert.AreEqual(
        intermediateTrackCount,
        Session.Query.All<Track>()
          .OfType<IntermediateTrack>()
          .Count());

      Assert.AreEqual(
        videoTrackCount,
        Session.Query.All<Track>()
          .OfType<VideoTrack>()
          .Count());

      Assert.AreEqual(
        audioTrackCount,
        Session.Query.All<Track>()
          .OfType<AudioTrack>()
          .Count());

      Assert.AreEqual(
        trackCount,
        Session.Query.All<Track>()
          .OfType<Track>()
          .Count());
    }

    [Test]
    public void CastSimpleTest()
    {
      var videoTracks = Session.Query.All<VideoTrack>().Cast<Track>();
      var list = videoTracks.ToList();
      Assert.Greater(list.Count, 0);
    }


    [Test]
    public void CastCountTest()
    {
      var videoTrackCount1 = Session.Query.All<VideoTrack>().Count();
      var videoTrackCount2 = Session.Query.All<VideoTrack>().Cast<Track>().Where(track => track!=null).Count();
      Assert.AreEqual(videoTrackCount1, videoTrackCount2);

      var audioTrackCount1 = Session.Query.All<AudioTrack>().Count();
      var audioTrackCount2 = Session.Query.All<AudioTrack>().Cast<Track>().Where(track => track!=null).Count();
      Assert.AreEqual(audioTrackCount1, audioTrackCount2);

      var trackCount1 = Session.Query.All<Track>().Count();
      var trackCount2 = Session.Query.All<Track>().Cast<Track>().Count();
      Assert.AreEqual(trackCount1, trackCount2);
    }

    [Test]
    public void OfTypeGetFieldTest()
    {
      var result = Session.Query.All<Track>()
        .OfType<VideoTrack>()
        .Select(vt => vt.Name);
      QueryDumper.Dump(result);
    }

    [Test]
    public void IsGetParentFieldTest()
    {
      var result = Session.Query.All<Track>()
        .Where(t => t is VideoTrack)
        .Select(x => (VideoTrack) x)
        .Select(vt => vt.Name);
      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(result));
    }

    [Test]
    public void IsGetChildFieldTest()
    {
      var result = Session.Query.All<Track>()
        .Where(t => t is VideoTrack)
        .Select(x => (VideoTrack) x)
        .Select(vt => vt.Name);
      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(result));
    }

    [Test]
    public void CastToBaseTest()
    {
      var result = Session.Query.All<VideoTrack>()
        .Select(x => (Track) x);
      QueryDumper.Dump(result);
    }


    [Test]
    public void IsBoolResultTest()
    {
      var result = Session.Query.All<Track>()
        .Select(x => x is VideoTrack
          ? x
          : null);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AnonymousCastTest()
    {
      var result = Session.Query.All<Track>()
        .Select(x => new {VideoTrack = x as VideoTrack});
      QueryDumper.Dump(result);
    }

    [Test]
    public void TwoChildrenCastTest()
    {
      var result = Session.Query.All<Track>()
        .Select(x =>
          new {
            VideoTrack = x as VideoTrack,
            AudioTrack = x as AudioTrack
          });
      QueryDumper.Dump(result);
    }


    [Test]
    public void ComplexIsCastTest()
    {
      var result = Session.Query.All<Track>()
        .Select(x =>
          new {
            VideoTrack = x is VideoTrack
              ? (VideoTrack) x
              : null,
            AudioTrack = x is AudioTrack
              ? (AudioTrack) x
              : null
          })
        .Select(x =>
          new {
            AQ = x.AudioTrack==null
              ? "NULL"
              : x.AudioTrack.Name,
            DQ = x.VideoTrack==null
              ? "NULL"
              : x.VideoTrack.Name
          });

      Assert.Throws<QueryTranslationException>(() => QueryDumper.Dump(result));
    }

    [Test]
    public void ComplexAsCastTest()
    {
      var result = Session.Query.All<Track>()
        .Select(track => 
          new {
            VideoTrack = track as VideoTrack,
            AudioTrack = track as AudioTrack
          })
        .Select(anonymousArgument => new {
            AQ = anonymousArgument.AudioTrack==null
              ? "NULL"
              : anonymousArgument.AudioTrack.Name,
            DQ = anonymousArgument.VideoTrack==null
              ? "NULL"
              : anonymousArgument.VideoTrack.Name
          });
      QueryDumper.Dump(result);
    }

    [Test]
    public void ComplexAsCast2Test()
    {
      var result = Session.Query.All<Track>()
        .Select(track =>
          new {
            VideoTrack = track,
            AudioTrack = track
          })
        .Select(anonymousArgument =>
          new {
            AQ = anonymousArgument.AudioTrack as AudioTrack,
            DQ = anonymousArgument.VideoTrack as VideoTrack
          });
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsDowncastTest()
    {
      var result = Session.Query.All<Track>()
        .Select(track => track as VideoTrack);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsDowncastWithFieldSelectTest()
    {
      var result = Session.Query.All<Track>()
        .Select(track => track as VideoTrack)
        .Select(videoTrack => videoTrack==null ? "NULL" : videoTrack.Name);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsUpcastTest()
    {
      var result = Session.Query.All<VideoTrack>()
        .Select(videoTrack => videoTrack as Track);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsUpcastWithFieldSelectTest()
    {
      var result = Session.Query.All<VideoTrack>()
        .Select(videoTrack => videoTrack as Track)
        .Select(track => track==null ? "NULL" : track.Name);
      QueryDumper.Dump(result);
    }

    [Test]
    public void AsUpcastWithConditionalTest()
    {
      var result = Session.Query.All<VideoTrack>()
        .Select(videoTrack => videoTrack as Track)
        .Select(track => track==null ? null : track);
      QueryDumper.Dump(result);
    }

    [Test]
    public void WrongCastTest()
    {
      var result = Session.Query.All<VideoTrack>()
        .Select(videoTrack => videoTrack as Track)
        .Select(track => track as AudioTrack);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceAsSimpleTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .Select(invoiceLine => invoiceLine.Track as AudioTrack);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceIsSimpleTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .Where(invoiceLine => invoiceLine.Track is AudioTrack);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeSimpleTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .Select(invoiceLine => invoiceLine.Track)
        .OfType<VideoTrack>();
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeWithFieldTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .Select(invoiceLine => invoiceLine.Track)
        .OfType<VideoTrack>()
        .Select(vt => vt.Name);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceAsAnonymousTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .Select(invoiceLine => new {Track = invoiceLine.Track as AudioTrack});
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceIsAnonymousTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .Where(invoiceLine => invoiceLine.Track is AudioTrack);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ReferenceOfTypeAnonymousTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .Select(invoiceLine => new {invoiceLine.Track})
        .Select(t => t.Track)
        .OfType<AudioTrack>();
      QueryDumper.Dump(result);
    }
    
    [Test]
    public void ReferenceOfTypeAnonymousWithFieldAccessTest()
    {
      var result = Session.Query.All<InvoiceLine>()
        .Select(invoiceLine => new {invoiceLine.Track})
        .Select(t => t.Track)
        .OfType<AudioTrack>()
        .Select(ap => ap.Name);
      QueryDumper.Dump(result);
    }

    [Test]
    public void ImplicitNumericTest()
    {
      long value = 0;
      var customer = Session.Query.All<Customer>().Where(c=>c.Company!=null).OrderBy(c => c.Company).First();
      var result = Session.Query.All<Customer>()
        .Where(c => c==customer)
        .Select(c => c.Company.Length + value)
        .First();
      Assert.AreEqual(customer.Company.Length, result);
    }
  }
}