// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.07.02

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Testing;

namespace Xtensive.Orm.Tests.Linq
{
  [TestFixture]
  public class QueryPreprocessorSortingTest
  {
    public class PreprocessorBase : IQueryPreprocessor
    {
      private static bool Enabled;

      private static readonly List<PreprocessorBase> CallSequence = new List<PreprocessorBase>();

      public virtual Expression Apply(Expression query)
      {
        if (Enabled)
          CallSequence.Add(this);

        return query;
      }

      public virtual bool IsDependentOn(IQueryPreprocessor other)
      {
        return false;
      }

      public static List<PreprocessorBase> GetCallSequence()
      {
        return CallSequence.ToList();
      }

      public static IDisposable EnableTracking()
      {
        CallSequence.Clear();

        Enabled = true;

        return new Disposable(_ => Enabled = false);
      }
    }

    [Service(typeof (IQueryPreprocessor))]
    public class Preprocessor1 : PreprocessorBase
    {
    }

    [Service(typeof (IQueryPreprocessor))]
    public class Preprocessor2 : PreprocessorBase
    {
      public override bool IsDependentOn(IQueryPreprocessor other)
      {
        return other is Preprocessor1;
      }
    }

    [Service(typeof (IQueryPreprocessor))]
    public class PreprocessorA : PreprocessorBase
    {
      public override bool IsDependentOn(IQueryPreprocessor other)
      {
        return other is PreprocessorC;
      }
    }

    [Service(typeof (IQueryPreprocessor))]
    public class PreprocessorB : PreprocessorBase
    {
      public override bool IsDependentOn(IQueryPreprocessor other)
      {
        return other is PreprocessorA;
      }
    }

    [Service(typeof (IQueryPreprocessor))]
    public class PreprocessorC : PreprocessorBase
    {
      public override bool IsDependentOn(IQueryPreprocessor other)
      {
        return other is PreprocessorB;
      }
    }

    [Test]
    public void SuccessTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (Preprocessor1));
      configuration.Types.Register(typeof (Preprocessor2));
      
      using (var domain = Domain.Build(configuration))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction())
      using (PreprocessorBase.EnableTracking()) {
        var q = session.Query.All<Metadata.Assembly>().ToList();
      }

      var callSequence = PreprocessorBase.GetCallSequence();
      Assert.That(callSequence.Count, Is.EqualTo(2));
      Assert.That(callSequence[0] is Preprocessor1);
      Assert.That(callSequence[1] is Preprocessor2);
    }

    [Test]
    public void FailureTest()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (PreprocessorA));
      configuration.Types.Register(typeof (PreprocessorB));
      configuration.Types.Register(typeof (PreprocessorC));

      AssertEx.ThrowsInvalidOperationException(() => Domain.Build(configuration));
    }
  }
}