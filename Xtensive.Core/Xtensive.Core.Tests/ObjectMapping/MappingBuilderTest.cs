// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.27

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Xtensive.Core.ObjectMapping;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Testing;
using Xtensive.Core.Tests.ObjectMapping.SourceModel;
using Xtensive.Core.Tests.ObjectMapping.TargetModel;

namespace Xtensive.Core.Tests.ObjectMapping
{
  [TestFixture]
  public sealed class MappingBuilderTest : MapperTestBase
  {
    [Test]
    public void MappingDescriptionReusingTest()
    {
      MappingDescription mapping = GetPetOwnerMapping();
      var mapper0 = new DefaultMapper(mapping);
      var source0 = GetSourcePetOwner(1);
      var target0 = (PetOwnerDto) mapper0.Transform(source0);
      EnsureAreEqual(source0, target0);
      var mapper1 = new DefaultMapper(mapping);
      var source1 = GetSourcePetOwner(2);
      var target1 = (PetOwnerDto) mapper1.Transform(source1);
      EnsureAreEqual(source1, target1);
      var source01 = GetSourcePetOwner(3);
      var target01 = (PetOwnerDto) mapper0.Transform(source01);
      EnsureAreEqual(source01, target01);
      var source11 = GetSourcePetOwner(4);
      var target11 = (PetOwnerDto) mapper1.Transform(source11);
      EnsureAreEqual(source11, target11);
    }

    [Test]
    public void ConcurrentUsingOfMappingDescriptionTest()
    {
      var mapping = GetPetOwnerMapping();
      const int iterationCount = 20000;
      var threadCount = Environment.ProcessorCount * 3;
      var startEvent = new ManualResetEvent(false);
      var finishedTaskCount = 0;
      ParameterizedThreadStart task = state => {
        startEvent.WaitOne();
        var mapper = new DefaultMapper(mapping);
        var rnd = new Random();
        var source = GetSourcePetOwner(rnd.Next());
        for (var i = 0; i < iterationCount; i++) {
          var target = (PetOwnerDto) mapper.Transform(source);
          EnsureAreEqual(source, target);
        }
        ((AutoResetEvent) state).Set();
        Interlocked.Increment(ref finishedTaskCount);
      };
      var finishEvents = new List<AutoResetEvent>();
      for (var i = 0; i < threadCount; i++) {
        var finishEvent = new AutoResetEvent(false);
        var thread = new Thread(task);
        thread.Start(finishEvent);
        finishEvents.Add(finishEvent);
      }
      startEvent.Set();
      WaitHandle.WaitAll(finishEvents.ToArray());
      Assert.AreEqual(threadCount, finishedTaskCount);
    }

    [Test]
    public void ReportFailWhenObjectPropertyFoundTest()
    {
      var mapping = new MappingBuilder()
        .MapType<ObjectContainer, ObjectContainerDto, Guid>(oc => oc.Id, oc => oc.Id)
          .IgnoreProperty(oc => oc.Object)
          .IgnoreProperty(oc => oc.ObjectCollection).Build();
      Assert.IsNotNull(mapping);
      var builder = new MappingBuilder()
        .MapType<ObjectContainer, ObjectContainerDto, Guid>(oc => oc.Id, oc => oc.Id);
      AssertEx.ThrowsInvalidOperationException(() => builder.Build());
      builder = new MappingBuilder()
        .MapType<ObjectContainer, ObjectContainerDto, Guid>(oc => oc.Id, oc => oc.Id)
          .IgnoreProperty(oc => oc.Object);
      AssertEx.ThrowsInvalidOperationException(() => builder.Build());
      builder = new MappingBuilder()
        .MapType<ObjectContainer, ObjectContainerDto, Guid>(oc => oc.Id, oc => oc.Id)
          .IgnoreProperty(oc => oc.ObjectCollection);
      AssertEx.ThrowsInvalidOperationException(() => builder.Build());
      AssertEx.ThrowsInvalidOperationException(
        () => new MappingBuilder()
          .MapType<ObjectContainer, ObjectContainerDto, Guid>(oc => oc.Id, oc => oc.Id)
          .MapStructure<object, AccessRightDto>());
    }

    private static MappingDescription GetPetOwnerMapping()
    {
      return new MappingBuilder()
        .MapType<PetOwner, PetOwnerDto, int>(p => p.Id, p => p.Id)
        .MapType<Animal, AnimalDto, Guid>(a => a.Id, a => a.Id)
        .Build();
    }

    private static void EnsureAreEqual(PetOwner source, PetOwnerDto target)
    {
      Assert.AreEqual(source.Id, target.Id);
      Assert.AreEqual(source.BirthDate, target.BirthDate);
      Assert.AreEqual(source.FirstName, target.FirstName);
      Assert.AreEqual(source.LastName, target.LastName);
      Assert.IsTrue(source.Pets.Select(p => new {p.Id, p.Name})
        .SequenceEqual(target.Pets.Select(p => new {p.Id, p.Name})));
    }
  }
}