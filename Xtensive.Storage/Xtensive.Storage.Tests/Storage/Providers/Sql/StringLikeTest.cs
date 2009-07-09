// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.09

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.DbTypeSupportModel;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  [TestFixture]
  public class StringLikeTest : AutoBuildTest
  {
    private DisposableSet disposableSet;

    protected override void CheckRequirements()
    {
      EnsureIs(StorageProtocols.Sql);
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      disposableSet = new DisposableSet();
      disposableSet.Add(Domain.OpenSession());
      disposableSet.Add(Transaction.Open());

      var testValues = new[]
        {
          "AAA",
          "%_%",
          "AB_",
          "POW",
          "__",
          "^QQ",
          "PQ^",
        };
      
      foreach (var value in testValues)
        new X {FString = value};
    }

    public override void TestFixtureTearDown()
    {
      disposableSet.DisposeSafely();
      base.TestFixtureTearDown();
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (X).Assembly, typeof (X).Namespace);
      return configuration;
    }

    [Test]
    public void StartsWithTest()
    {
      var result = Query<X>.All.Select(x => new
        {
          x.Id,
          String = x.FString,
          StartsWithA = x.FString.StartsWith("A"),
          StartsWithPercent = x.FString.StartsWith("%"),
          StartsWithGround = x.FString.StartsWith("_"),
          StartsWithPrecentGround = x.FString.StartsWith("%_"),
          StartsWithGroundPercent = x.FString.StartsWith("_%"),
          StartsWithEscape = x.FString.StartsWith("^"),
        })
        .ToList();
      foreach (var x in result) {
        Assert.AreEqual(x.String.StartsWith("A"), x.StartsWithA);
        Assert.AreEqual(x.String.StartsWith("%"), x.StartsWithPercent);
        Assert.AreEqual(x.String.StartsWith("_"), x.StartsWithGround);
        Assert.AreEqual(x.String.StartsWith("%_"), x.StartsWithPrecentGround);
        Assert.AreEqual(x.String.StartsWith("_%"), x.StartsWithGroundPercent);
        Assert.AreEqual(x.String.StartsWith("^"), x.StartsWithEscape);
      }
    }

    [Test]
    public void EndsWithTest()
    {
       var result = Query<X>.All.Select(x => new
        {
          x.Id,
          String = x.FString,
          EndsWithA = x.FString.EndsWith("A"),
          EndsWithPercent = x.FString.EndsWith("%"),
          EndsWithGround = x.FString.EndsWith("_"),
          EndsWithPrecentGround = x.FString.EndsWith("%_"),
          EndsWithGroundPercent = x.FString.EndsWith("_%"),
          EndsWithEscape = x.FString.EndsWith("^"),
        })
        .ToList();
      foreach (var x in result) {
        Assert.AreEqual(x.String.EndsWith("A"), x.EndsWithA);
        Assert.AreEqual(x.String.EndsWith("%"), x.EndsWithPercent);
        Assert.AreEqual(x.String.EndsWith("_"), x.EndsWithGround);
        Assert.AreEqual(x.String.EndsWith("%_"), x.EndsWithPrecentGround);
        Assert.AreEqual(x.String.EndsWith("_%"), x.EndsWithGroundPercent);
        Assert.AreEqual(x.String.EndsWith("^"), x.EndsWithEscape);
      }
    }

    [Test]
    public void ContainsTest()
    {
      var result = Query<X>.All.Select(x => new
        {
          x.Id,
          String = x.FString,
          ContainsA = x.FString.Contains("A"),
          ContainsPercent = x.FString.Contains("%"),
          ContainsGround = x.FString.Contains("_"),
          ContainsPrecentGround = x.FString.Contains("%_"),
          ContainsGroundPercent = x.FString.Contains("_%"),
          ContainsEscape = x.FString.Contains("^"),
        })
        .ToList();
      foreach (var x in result) {
        Assert.AreEqual(x.String.Contains("A"), x.ContainsA);
        Assert.AreEqual(x.String.Contains("%"), x.ContainsPercent);
        Assert.AreEqual(x.String.Contains("_"), x.ContainsGround);
        Assert.AreEqual(x.String.Contains("%_"), x.ContainsPrecentGround);
        Assert.AreEqual(x.String.Contains("_%"), x.ContainsGroundPercent);
        Assert.AreEqual(x.String.Contains("^"), x.ContainsEscape);
      }
    }

    [Test]
    public void NotSupportedTest()
    {
      var localVar = ":-)";
      AssertEx.ThrowsNotSupportedException(
        () => Query<X>.All.Select(x => x.FString.StartsWith(localVar)).ToList());
      AssertEx.ThrowsNotSupportedException(
        () => Query<X>.All.Select(x => x.FString.EndsWith(localVar)).ToList());
      AssertEx.ThrowsNotSupportedException(
        () => Query<X>.All.Select(x => x.FString.Contains(localVar)).ToList());
    }
  }
}