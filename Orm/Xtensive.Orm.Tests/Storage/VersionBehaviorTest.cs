// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.05

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm.Tests.Storage.VersionBehavior.Model;
using Version = Xtensive.Orm.Tests.Storage.VersionBehavior.Model.Version;

namespace Xtensive.Orm.Tests.Storage
{
  namespace VersionBehavior.Model
  {
    public abstract class Base : Entity
    {
      [Field]
      public string Tag { get; set; }

      [Field(LazyLoad = true)]
      public string Content { get; set; }
    }

    [HierarchyRoot]
    public class Default : Base
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public int Version { get; set; }
    }

    public class DefaultInheritor : Default
    {
      [Field]
      public int SubVersion { get; set; }
    }

    [HierarchyRoot]
    public class Manual : Base
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Version(VersionMode.Manual)]
      public int Version { get; set; }
    }

    [HierarchyRoot]
    public class AnotherManual : Base
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Version(VersionMode.Manual)]
      public int Version { get; set; }

      [Field]
      [Version(VersionMode.Manual)]
      public long SubVersion { get; set; }
    }

    public class ManualInheritor : Manual
    {
      [Field]
      public string Name { get; set; }
    }

    public class AnotherManualInheritor : Manual
    {
      [Field]
      public string Name { get; set; }
      [Field]
      [Version(VersionMode.Manual)]
      public int SubVersion { get; set; }
    }

    [HierarchyRoot]
    public class Auto : Base
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Version(VersionMode.Auto)]
      public DateTime Date { get; set; }
    }

    public class AutoInheritor : Auto
    {
      [Field]
      [Version(VersionMode.Auto)]
      public Guid Uid { get; set; }
    }

    [HierarchyRoot]
    public class Skip : Base
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Version(VersionMode.Skip)]
      public string NotVersion { get; set; }

      [Field]
      public string Description { get; set; }
    }

    public class Version : Structure
    {
      [Field]
      [Version(VersionMode.Manual)]
      public int Major { get; set; }
      [Field]
      [Version(VersionMode.Auto)]
      public int Minor { get; set; }
      [Field]
      [Version(VersionMode.Skip)]
      public int Meta { get; set; }
    }

    [HierarchyRoot]
    public class HasVersion : Base
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public Version Version { get; set; }
    }

    [HierarchyRoot]
    public class HasSkipVersion : Base
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Version(VersionMode.Skip)]
      public Version Version { get; set; }
    }

    [HierarchyRoot]
    public class HasManualVersion : Base // Should throw an exception during model build
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Version(VersionMode.Manual)]
      public Version Version { get; set; }
    }

    [HierarchyRoot]
    public class HasAutoVersion : Base // Should throw an exception during model build
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Version(VersionMode.Auto)]
      public Version Version { get; set; }
    }
  }

  [TestFixture]
  public class VersionBehaviorTest
  {
    [Test]
    public void DefaultTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (Base));
      config.Types.Register(typeof (Default));
      config.Types.Register(typeof (DefaultInheritor));
      var domain = Domain.Build(config);
      var defaultTypeInfo = domain.Model.Types[typeof(Default)];
      var defaultInheritorTypeInfo = domain.Model.Types[typeof(DefaultInheritor)];
      Assert.AreEqual(3, defaultTypeInfo.GetVersionColumns().Count);
      Assert.AreEqual(4, defaultInheritorTypeInfo.GetVersionColumns().Count);
      using (var session = domain.OpenSession()) {
        var versions = new VersionSet();
        var updatedVersions = new VersionSet();
        Default @default;
        using (VersionCapturer.Attach(session, versions))
        using (var t = session.OpenTransaction()) {
          @default = new Default() { Name = "Name", Tag = "Tag", Version = 1};
          t.Complete();
        }
        using (VersionCapturer.Attach(session, updatedVersions))
        using (VersionValidator.Attach(session, versions))
        using (var t = session.OpenTransaction()) {
          @default.Version = 2;
          @default.Name = "AnotherName";
          @default.Name = "AnotherNameCorrect";
          t.Complete();
        }
        AssertEx.Throws<VersionConflictException>(() => {
          using (VersionValidator.Attach(session, versions))
          using (var t = session.OpenTransaction()) {
            @default.Tag = "AnotherTag";
            t.Complete();
          }});

        using (VersionValidator.Attach(session, updatedVersions))
        using (var t = session.OpenTransaction()) {
          @default.Tag = "AnotherTag";
          t.Complete();
        }
      }
      var allVersions = new VersionSet();
      using (var session = domain.OpenSession())
      using (VersionCapturer.Attach(session, allVersions))
      using (VersionValidator.Attach(session, allVersions)) {
        Default @default;
        using (var t = session.OpenTransaction()) {
          @default = new Default() { Name = "Name", Tag = "Tag", Version = 1};
          t.Complete();
        }
        
        using (var t = session.OpenTransaction()) {
          @default.Version = 2;
          @default.Name = "AnotherName";
          @default.Name = "AnotherNameCorrect";
          t.Complete();
        }
        
        using (var t = session.OpenTransaction()) {
          @default.Tag = "AnotherTag";
          t.Complete();
        }
      }
    }

    [Test]
    public void ManualTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (Base));
      config.Types.Register(typeof (Manual));
      config.Types.Register(typeof (AnotherManual));
      config.Types.Register(typeof (ManualInheritor));
      config.Types.Register(typeof (AnotherManualInheritor));
      var domain = Domain.Build(config);
      var manualTypeInfo = domain.Model.Types[typeof(Manual)];
      var anotherManualTypeInfo = domain.Model.Types[typeof(AnotherManual)];
      var manualInheritorTypeInfo = domain.Model.Types[typeof(ManualInheritor)];
      var anotherManualInheritorTypeInfo = domain.Model.Types[typeof(AnotherManualInheritor)];
      Assert.AreEqual(1, manualTypeInfo.GetVersionColumns().Count);
      Assert.AreEqual(2, anotherManualTypeInfo.GetVersionColumns().Count);
      Assert.AreEqual(1, manualInheritorTypeInfo.GetVersionColumns().Count);
      Assert.AreEqual(2, anotherManualInheritorTypeInfo.GetVersionColumns().Count);
      using (var session = domain.OpenSession()) {
        var versions = new VersionSet();
        var updatedVersions = new VersionSet();
        Manual manual;
        ManualInheritor manualInheritor;
        AnotherManual anotherManual;
        AnotherManualInheritor anotherManualInheritor;
        using (VersionCapturer.Attach(session, versions))
        using (var t = session.OpenTransaction()) {
          manual = new Manual() { Tag = "Tag", Content = "Content", Version = 1};
          manualInheritor = new ManualInheritor() { Tag = "Tag", Content = "Content", Version = 1, Name = "Name"};
          anotherManual = new AnotherManual() {Tag = "Tag", Content = "Content", Version = 1, SubVersion = 100};
          anotherManualInheritor = new AnotherManualInheritor() {Tag = "Tag", Content = "Content", Version = 1, SubVersion = 100, Name = "Name"};
          t.Complete();
        }
        using (VersionCapturer.Attach(session, updatedVersions))
        using (VersionValidator.Attach(session, versions))
        using (var t = session.OpenTransaction()) {
          manual.Version = 2;
          manual.Tag = "AnotherTag";
          manual.Tag = "AnotherTagCorrect";
          manualInheritor.Name = "AnotherName";
          manualInheritor.Version = 2;
          anotherManual.Tag = "AnotherTag";
          anotherManual.SubVersion = 200;
          anotherManualInheritor.Name = "AnotherName";
          anotherManualInheritor.SubVersion = 200;
          t.Complete();
        }
        AssertEx.Throws<VersionConflictException>(() => {
          using (VersionValidator.Attach(session, versions))
          using (var t = session.OpenTransaction()) {
            manual.Tag = "YetAnotherTag";
            anotherManual.Tag = "YetAnotherTag";
            anotherManual.Version = 2;
            manualInheritor.Name = "YetAnotherName";
            anotherManualInheritor.Name = "YetAnotherName";
            t.Complete();
          }});

        using (VersionValidator.Attach(session, updatedVersions))
        using (var t = session.OpenTransaction()) {
          manual.Tag = "YetAnotherTag";
          anotherManual.Tag = "YetAnotherTag";
          anotherManual.Version = 2;
          manualInheritor.Name = "YetAnotherName";
          anotherManualInheritor.Name = "YetAnotherName";
          t.Complete();
        }
      }
    }

    [Test]
    public void AutoTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (Base));
      config.Types.Register(typeof (Auto));
      config.Types.Register(typeof (AutoInheritor));
      var domain = Domain.Build(config);
      var autoTypeInfo = domain.Model.Types[typeof(Auto)];
      var autoInheritorTypeInfo = domain.Model.Types[typeof(AutoInheritor)];
      Assert.AreEqual(1, autoTypeInfo.GetVersionColumns().Count);
      Assert.AreEqual(2, autoInheritorTypeInfo.GetVersionColumns().Count);
      using (var session = domain.OpenSession()) {
        var versions = new VersionSet();
        var updatedVersions = new VersionSet();
        Auto auto;
        AutoInheritor autoInheritor;
        using (VersionCapturer.Attach(session, versions))
        using (var t = session.OpenTransaction()) {
          auto = new Auto() { Content = "Content", Tag = "Tag"};
          autoInheritor = new AutoInheritor() { Content = "Content", Tag = "Tag"};
          t.Complete();
        }
        using (VersionCapturer.Attach(session, updatedVersions))
        using (VersionValidator.Attach(session, versions))
        using (var t = session.OpenTransaction()) {
          auto.Content = "AnotherContent";
          auto.Content = "AnotherContetnCorrect";
          autoInheritor.Content = "AnotherContent";
          autoInheritor.Content = "AnotherContetnCorrect";
          t.Complete();
        }
        AssertEx.Throws<VersionConflictException>(() => {
          using (VersionValidator.Attach(session, versions))
          using (var t = session.OpenTransaction()) {
            auto.Tag = "AnotherTag";
            autoInheritor.Tag = "AnotherTag";
            t.Complete();
          }});

        using (VersionValidator.Attach(session, updatedVersions))
        using (var t = session.OpenTransaction()) {
          auto.Tag = "AnotherTag";
          autoInheritor.Tag = "AnotherTag";
          t.Complete();
        }
      }
      var allVersions = new VersionSet();
      using (var session = domain.OpenSession())
      using (VersionCapturer.Attach(session, allVersions))
      using (VersionValidator.Attach(session, allVersions)) {
        Auto auto;
        AutoInheritor autoInheritor;
        using (var t = session.OpenTransaction()) {
          auto = new Auto() { Content = "Content", Tag = "Tag"};
          autoInheritor = new AutoInheritor() { Content = "Content", Tag = "Tag"};
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          auto.Content = "AnotherContent";
          auto.Content = "AnotherContetnCorrect";
          autoInheritor.Content = "AnotherContent";
          autoInheritor.Content = "AnotherContetnCorrect";
          t.Complete();
        }
       
        using (var t = session.OpenTransaction()) {
          auto.Tag = "AnotherTag";
          autoInheritor.Tag = "AnotherTag";
          t.Complete();
        }
      }

    }

    [Test]
    public void SkipTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof (Base));
      config.Types.Register(typeof (Skip));
      config.Types.Register(typeof (VersionBehavior.Model.Version));
      config.Types.Register(typeof (HasVersion));
      config.Types.Register(typeof (HasSkipVersion));
      var domain = Domain.Build(config);
      var skipTypeInfo = domain.Model.Types[typeof(Skip)];
      var hasVersionTypeInfo = domain.Model.Types[typeof(HasVersion)];
      var hasSkipVersionTypeInfo = domain.Model.Types[typeof(HasSkipVersion)];
      Assert.AreEqual(2, skipTypeInfo.GetVersionColumns().Count);
      Assert.AreEqual(2, hasVersionTypeInfo.GetVersionColumns().Count);
      Assert.AreEqual(2, hasSkipVersionTypeInfo.GetVersionColumns().Count);
      using (var session = domain.OpenSession()) {
        var versions = new VersionSet();
        var updatedVersions = new VersionSet();
        Skip skip;
        HasVersion hasVersion;
        using (VersionCapturer.Attach(session, versions))
        using (var t = session.OpenTransaction()) {
          skip = new Skip() { Content = "Content", Tag = "Tag", Description = "Desription", NotVersion = "NotVersion"};
          hasVersion = new HasVersion { 
            Content = "Content",
            Tag = "Tag",
            Version = {Major = 10, Minor = 100, Meta = 1000 }};
          t.Complete();
        }
        using (VersionCapturer.Attach(session, updatedVersions))
        using (VersionValidator.Attach(session, versions))
        using (var t = session.OpenTransaction()) {
          skip.Content = "AnotherContent";
          skip.Content = "AnotherContetnCorrect";
          hasVersion.Content = "AnotherContent";
          hasVersion.Content = "AnotherContetnCorrect";
          t.Complete();
        }
        AssertEx.Throws<VersionConflictException>(() => {
          using (VersionValidator.Attach(session, versions))
          using (var t = session.OpenTransaction()) {
            skip.Tag = "AnotherTag";
            hasVersion.Tag = "AnotherTag";
            t.Complete();
          }});

        using (VersionValidator.Attach(session, updatedVersions))
        using (var t = session.OpenTransaction()) {
          skip.Tag = "AnotherTag";
          hasVersion.Tag = "AnotherTag";
          t.Complete();
        }
      }
      var allVersions = new VersionSet();
      using (var session = domain.OpenSession())
      using (VersionCapturer.Attach(session, allVersions))
      using (VersionValidator.Attach(session, allVersions)) {
        Skip skip;
        HasVersion hasVersion;
        using (var t = session.OpenTransaction()) {
          skip = new Skip() { Content = "Content", Tag = "Tag", Description = "Desription", NotVersion = "NotVersion"};
          hasVersion = new HasVersion { 
            Content = "Content",
            Tag = "Tag",
            Version = {Major = 10, Minor = 100, Meta = 1000 }};
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          skip.Content = "AnotherContent";
          skip.Content = "AnotherContetnCorrect";
          hasVersion.Content = "AnotherContent";
          hasVersion.Content = "AnotherContetnCorrect";
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          skip.Tag = "AnotherTag";
          hasVersion.Tag = "AnotherTag";
          t.Complete();
        }
      }
    }

    [Test]
    [ExpectedException(typeof(DomainBuilderException))]
    public void HasManualVersionTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Base));
      config.Types.Register(typeof(VersionBehavior.Model.Version));
      config.Types.Register(typeof(HasManualVersion));
      Domain.Build(config);
    }

    [Test]
    [ExpectedException(typeof(DomainBuilderException))]
    public void HasAutoVersionTest()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(Base));
      config.Types.Register(typeof(VersionBehavior.Model.Version));
      config.Types.Register(typeof(HasAutoVersion));
      Domain.Build(config);
    }
  }
}