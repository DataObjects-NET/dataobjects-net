// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.20

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Testing;
using Xtensive.Integrity.Atomicity;
using Xtensive.Integrity.Atomicity.OperationLogs;
using Xtensive.Core.Diagnostics;
using Xtensive.Integrity.Validation;

namespace Xtensive.Integrity.Tests
{
  [TestFixture]
  public class CombinedTest
  {
    Session fastSession = new Session();
    Session session1 = new Session(AtomicityContextOptions.Full);
    Session session2 = new Session(AtomicityContextOptions.Full);
    private Person person1;
    private Person person2;

    [SetUp]
    public void Setup()
    {
      using (Log.InfoRegion("Setup"))
      using (session1.Activate()) {
        person1 = new Person("Alex", 28);
        using (session2.Activate()) {
          person2 = new Person("Andrey", 24);
        }
      }
    }

    [Test]
    public void CustomErrorsTest()
    {
      using (var region = person1.Context.OpenInconsistentRegion()) {
        person1.SetAll("26", 26);

        Assert.AreEqual(
          "Name==Age.ToString()",
          person1.GetObjectError());

        person1.SetAll("Alex", 26);

        Assert.AreEqual(
          string.Empty,
          person1.GetObjectError());

        region.Complete();
      }
    }

    [Test]
    public void BaseContextTest()
    {
      Assert.AreEqual(session1, person1.Session);
      Assert.AreEqual(session2, person2.Session);
    }

    [Test]
    public void GroupUndoTest()
    {
      Log.Info("Person: {0}", person1);
      string oldName = person1.Name;
      int oldAge = person1.Age;
      try {
        person1.SetAll("Sergey", 300);
      }
      catch (ArgumentOutOfRangeException) {
        person1.Session.ValidationContext.Reset();
      }
      Log.Info("Person: {0}", person1);
      Assert.AreEqual(oldName, person1.Name);
      Assert.AreEqual(oldAge, person1.Age);
    }

    [Test]
    public void ContextActivationTest()
    {
      person1.SetAll("+Alex", 29, "1", "OVD1");
      person2.SetAll("+Andrey", 25, "2", "OVD2");
      Assert.AreEqual(session1, person1.Passport.Session);
      Assert.AreEqual(session2, person2.Passport.Session);
    }

    [Test]
    public void PropertyPairSyncTest()
    {
      person1.SetAll("Alex", 28, "1", "OVD1");
      Passport passport1 = person1.Passport;
      Assert.AreSame(person1, passport1.Person);
      Assert.AreSame(passport1, person1.Passport);

      using (person1.Session.Activate())
        person1.Passport = new Passport("1", "OVD2");
      Passport passport2 = person1.Passport;
      Assert.AreSame(person1, passport2.Person);
      Assert.AreSame(passport2, person1.Passport);
      Assert.IsNull(passport1.Person);

      person1.Passport = null;
      Assert.IsNull(person1.Passport);
      Assert.IsNull(passport1.Person);
    }

    [Test]
    [ExpectedException(typeof(AggregateException))]
    public void ValidationTest()
    {     
      try {
        person1.SetAll("1", 1);
      }
      finally {
        person1.Session.ValidationContext.Reset();
      }
    }

    [Test]
    public void UndoRedoTest()
    {
      Log.Info("Cloning...");
      var operationLog1 = (OperationLogBase) session1.AtomicityContext.OperationLog;
      var initialOperations1 = new List<IRedoDescriptor>(operationLog1);
      Person initialPerson1;
      using (person1.Session.Activate()) {
        initialPerson1 = (Person) LegacyBinarySerializer.Instance.Clone(person1);
      }
      Log.Info("Person: {0}", person1);
      Assert.AreEqual(initialPerson1, person1);
      
      Log.Info("Modifying...");
      person1.SetAll("-Alex", 27);
      person1.Age++;
      person1.Name += "-";
      using (person1.Session.Activate()) {
        person1.Name += "+";
        Passport passport1 = new Passport("3", "OVD3");
        person1.Passport = passport1;
      }
      Log.Info("Person: {0}", person1);
      List<IRedoDescriptor> operations1 = new List<IRedoDescriptor>(operationLog1);
      Assert.AreNotEqual(initialPerson1, person1);
      Person finalPerson1;
      using (person1.Session.Activate()) {
        finalPerson1 = (Person) LegacyBinarySerializer.Instance.Clone(person1);
      }
      Assert.AreEqual(finalPerson1, person1);
      
      Log.Info("Undoing...");
      using (person1.Session.Activate()) {
        for (int i = operations1.Count-1; i>=initialOperations1.Count; i--)
          operations1[i].OppositeDescriptor.Invoke();
      }
      Log.Info("Person: {0}", person1);
      Assert.AreEqual(initialPerson1, person1);
      
      Log.Info("Redoing...");
      using (person1.Session.Activate()) {
        for (int i = initialOperations1.Count; i < operations1.Count; i++)
          operations1[i].Invoke();
      }
      Log.Info("Person: {0}", person1);
      Assert.AreEqual(finalPerson1, person1);
    }

    [Test]
    public void PerformanceTest()
    {
      using (fastSession.Activate()) {
        AtomicBase target = new Person("Alex", 28);
        Log.Info("Warmup...");
        using (new LogIndentScope()) {
          MeasureAll(target, 1000);
        }
        Log.Info("Test...");
        using (new LogIndentScope()) {
          MeasureAll(target, 100000);
        }
      }
    }

    [Test]
    public void InconsistentRegionsTest()
    {
      ValidationContext context = new ValidationContext(session1);

      using (var r1 = context.OpenInconsistentRegion()) {
        using (var r2 = context.OpenInconsistentRegion()) {
          r2.Complete();
        }
//        Assert.IsTrue(context.IsValid);
        Assert.IsFalse(context.IsConsistent);
        r1.Complete();
      }
//      Assert.IsTrue(context.IsValid);
      Assert.IsTrue(context.IsConsistent);

      using (context.OpenInconsistentRegion()) { }

//      Assert.IsFalse(context.IsValid);

//      AssertEx.ThrowsInvalidOperationException(() => 
//        context.OpenInconsistentRegion());
    }

    private void MeasureAll(AtomicBase target, int count)
    {
      MeasureDummyUndoableAtomic(target, count);
      MeasureDummyAtomicToUndoable(target, count);
      MeasureDummyAtomicToAtomicToUndoable3(target, count);
      MeasureDummyAtomicToAtomicToUndoable1(target, count);
    }

    private void MeasureDummyAtomicToAtomicToUndoable1(AtomicBase target, int count)
    {
      using (new Measurement("Invoking AtomicBase.DummyAtomicToAtomicToUndoable()", count)) {
        for (int i = 0; i<count; i++)
          target.DummyAtomicToAtomicToUndoable(1,1);
      }
    }

    private void MeasureDummyAtomicToAtomicToUndoable3(AtomicBase target, int count)
    {
      using (new Measurement("Invoking AtomicBase.DummyAtomicToAtomicToUndoable()", count)) {
        count = count/9;
        for (int i = 0; i<count; i++)
          target.DummyAtomicToAtomicToUndoable(3,3);
      }
    }

    private void MeasureDummyAtomicToUndoable(AtomicBase target, int count)
    {
      using (new Measurement("Invoking AtomicBase.DummyAtomicToUndoable()", count)) {
        count = count/3;
        for (int i = 0; i<count; i++)
          target.DummyAtomicToUndoable(3);
      }
    }

    private void MeasureDummyUndoableAtomic(AtomicBase target, int count)
    {
      using (new Measurement("Invoking AtomicBase.DummyUndoableAtomic()", count)) {
        for (int i = 0; i<count; i++)
          target.DummyUndoableAtomic();
      }
    }
  }
}