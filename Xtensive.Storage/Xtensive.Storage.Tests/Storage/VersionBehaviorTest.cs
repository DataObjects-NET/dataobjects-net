// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.08.05

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.Storage.VersionBehavior.Model;

namespace Xtensive.Storage.Tests.Storage
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

    [HierarchyRoot]
    public class Manual : Base
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      [Version]
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
      [Version]
      public int SubVersion { get; set; }
    }

    [HierarchyRoot]
    public class Auto : Base
    {
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
      [Field]
      [Version(VersionMode.Skip)]
      public string NotVersion { get; set; }

      [Field]
      public string Description { get; set; }
    }

    public class Version : Structure
    {
      [Field]
      [Version]
      public int Major { get; set; }
      [Field]
      [Version(VersionMode.Auto)]
      public int Minor { get; set; }
      [Field]
      [Version(VersionMode.Skip)]
      public int Meta { get; set; }
    }

    [HierarchyRoot]
    public class HasSkipVersion : Base
    {
      [Field]
      [Version(VersionMode.Skip)]
      public Version Version { get; set; }
    }

    [HierarchyRoot]
    public class HasManualVersion : Base
    {
      [Field]
      [Version(VersionMode.Manual)]
      public Version Version { get; set; }
    }

    [HierarchyRoot]
    public class HasAutoVersion : Base // Should throw an exception during model build
    {
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
      var domain = Domain.Build(config);
      using (var session = Session.Open(domain)) {
        var versions = new VersionSet();
        var updatedVersions = new VersionSet();
        Default @default;
        using (VersionCapturer.Attach(versions))
        using (var t = Transaction.Open()) {
          @default = new Default() { Name = "Name", Tag = "Tag", Version = 1};
          t.Complete();
        }
        using (VersionCapturer.Attach(updatedVersions))
        using (VersionValidator.Attach(versions))
        using (var t = Transaction.Open()) {
          @default.Version = 2;
          @default.Name = "AnotherName";
          @default.Name = "AnotherNameCorrect";
          t.Complete();
        }
        AssertEx.Throws<VersionConflictException>(() => {
          using (VersionValidator.Attach(versions))
          using (var t = Transaction.Open()) {
            @default.Tag = "AnotherTag";
            t.Complete();
          }});

        using (VersionValidator.Attach(updatedVersions))
        using (var t = Transaction.Open()) {
          @default.Tag = "AnotherTag";
          t.Complete();
        }
      }
      var allVersions = new VersionSet();
      using (var session = Session.Open(domain))
      using (VersionCapturer.Attach(allVersions))
      using (VersionValidator.Attach(allVersions)) {
        Default @default;
        using (var t = Transaction.Open()) {
          @default = new Default() { Name = "Name", Tag = "Tag", Version = 1};
          t.Complete();
        }
        
        using (var t = Transaction.Open()) {
          @default.Version = 2;
          @default.Name = "AnotherName";
          @default.Name = "AnotherNameCorrect";
          t.Complete();
        }
        
        using (var t = Transaction.Open()) {
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
      using (var session = Session.Open(domain)) {
        var versions = new VersionSet();
        var updatedVersions = new VersionSet();
        Manual manual;
        ManualInheritor manualInheritor;
        AnotherManual anotherManual;
        AnotherManualInheritor anotherManualInheritor;
        using (VersionCapturer.Attach(versions))
        using (var t = Transaction.Open()) {
          manual = new Manual() { Tag = "Tag", Content = "Content", Version = 1};
          manualInheritor = new ManualInheritor() { Tag = "Tag", Content = "Content", Version = 1, Name = "Name"};
          anotherManual = new AnotherManual() {Tag = "Tag", Content = "Content", Version = 1, SubVersion = 100};
          anotherManualInheritor = new AnotherManualInheritor() {Tag = "Tag", Content = "Content", Version = 1, SubVersion = 100, Name = "Name"};
          t.Complete();
        }
        using (VersionCapturer.Attach(updatedVersions))
        using (VersionValidator.Attach(versions))
        using (var t = Transaction.Open()) {
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
          using (VersionValidator.Attach(versions))
          using (var t = Transaction.Open()) {
            manual.Tag = "YetAnotherTag";
            anotherManual.Tag = "YetAnotherTag";
            anotherManual.Version = 2;
            manualInheritor.Name = "YetAnotherName";
            anotherManualInheritor.Name = "YetAnotherName";
            t.Complete();
          }});

        using (VersionValidator.Attach(updatedVersions))
        using (var t = Transaction.Open()) {
          manual.Tag = "YetAnotherTag";
          anotherManual.Tag = "YetAnotherTag";
          anotherManual.Version = 2;
          manualInheritor.Name = "YetAnotherName";
          anotherManualInheritor.Name = "YetAnotherName";
          t.Complete();
        }
      }
    }
  }
}