// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.23

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PostSharp.Laos;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Core.Testing;
using Xtensive.Integrity.Aspects.Constraints;
using Xtensive.Integrity.Validation;

namespace Xtensive.Integrity.Tests
{
  [TestFixture]
  public class ConstraintsTest
  {
    internal class ValidationContext : ValidationContextBase
    {
      public new void Reset()
      {
        base.Reset();
      }
    }

    internal static ValidationContext context = new ValidationContext();

    internal class ValidatableObject : IValidationAware
    {
      public void OnValidate()
      {
        this.CheckConstraints();
      }

      ValidationContextBase IContextBound<ValidationContextBase>.Context
      {
        get { return context; }
      }
    }

    #region LogMethodFastAspect

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true, Inherited = false)]
    [Serializable]
    internal class LogMethodFastAspect : ReprocessMethodBoundaryAspect, ILaosWeavableAspect
    {
      public MethodBase Method { get; private set; }

      public int AspectPriority
      {
        get { return -200000; }
      }

      public override object OnEntry(object instance)
      {
        Log.Info("OnEntry called on {0}.", Method.GetShortName(true));
        return "OnEntry";
      }

      public override void OnExit(object instance, object onEntryResult)
      {
        Log.Info("OnExit called.");
        Log.Info(string.Format("OnEntry result: {0}", onEntryResult));
      }

      public override void OnSuccess(object instance, object onEntryResult)
      {
        Log.Info("OnSuccess called.");
      }

      public override ErrorFlowBehavior OnError(object instance, Exception e)
      {
        Log.Error(e);
        return ErrorFlowBehavior.Rethrow;
      }

      public override void RuntimeInitialize(MethodBase method)
      {
        Method = method;
        Log.Info("RuntimeInitialize for {0}.", method.GetShortName(true));
      }
    }

    #endregion

    internal class Person : ValidatableObject
    {
      [Trace]
      [NotNullOrEmptyConstraint]
      [LengthConstraint(Max = 20, Mode = ConstrainMode.OnSetValue)]
      public string Name { [LogMethodFastAspect] get; [LogMethodFastAspect] set;}

      [RangeConstraint(Min = 0, Message = "Incorrect age ({value}), age can not be less than {Min}.")]
      public int Age { get; set;}

      [PastConstraint]
      public DateTime RegistrationDate { get; set;}

      [RegexConstraint(Pattern = @"^(\(\d+\))?[-\d ]+$", Message = "Incorrect phone format '{value}'")]
      public string Phone { get; set;}

      [EmailConstraint]
      public string Email { get; set;}

      [RangeConstraint(Min = 1, Max = 2.13)]
      public double Height { get; set; }
    }

    internal class AdvancedPerson : Person
    {
    }

    [Test]
    public void CustomErrorsTest()
    {
      context = new ValidationContext();

      using (var region = context.OpenInconsistentRegion()) {
        Person person = new Person();

        Assert.AreEqual(
          "Name can not be null or empty.", 
          person.GetPropertyError("Name"));

        person.Name = "Alex Kofman";
        person.Age = -26;

        Assert.AreEqual(
          "Incorrect age (-26), age can not be less than 0.", 
          person.GetPropertyError("Age"));

        Assert.AreEqual(
          string.Empty, 
          person.GetPropertyError("Name"));
      }
    }

    [Test]
    public void PersonTest()
    {
      context = new ValidationContext();

      try {
        using (var region = context.OpenInconsistentRegion()) {
          Person person = new Person();
          
          person.Age = -1;
          person.RegistrationDate = new DateTime(2100, 1, 1);
          person.Phone = "one-one-two-four-seven-one";
          person.Email = "my name@my domain";
          person.Height = 2.15;

          person.Validate();
          region.Complete();
        }
      }
      catch (AggregateException exception) {
        var errors = exception.GetFlatExceptions().Cast<ConstraintViolationException>();
        Assert.AreEqual(6, errors.Count());
        Assert.IsTrue(errors.Any(error => error.TargetType==typeof(Person)));

        var nameExcepthion = errors.Where(error => error.TargetProperty.Name=="Name").Single();
        Assert.AreEqual("Name can not be null or empty.", nameExcepthion.Message);

        var ageExcepthion = errors.Where(error => error.TargetProperty.Name=="Age").Single();
        Assert.AreEqual("Incorrect age (-1), age can not be less than 0.", ageExcepthion.Message);

        var phoneError = errors.Where(error => error.TargetProperty.Name=="Phone").Single();
        Assert.AreEqual("Incorrect phone format 'one-one-two-four-seven-one'", phoneError.Message);

        var registrationDateError = errors.Where(error => error.TargetProperty.Name=="RegistrationDate").Single();
        Assert.AreEqual("RegistrationDate must be in the past.", registrationDateError.Message);

        var emailError = errors.Where(error => error.TargetProperty.Name=="Email").Single();
        Assert.AreEqual("Email format is incorrect.", emailError.Message);

        var heightError = errors.Where(error => error.TargetProperty.Name=="Height").Single();
        Assert.AreEqual(string.Format("Height can not be less than 1 or greater than {0}.", 2.13), heightError.Message);
      }

      context.Reset();

      using (var region = context.OpenInconsistentRegion()) {
        Person person = new Person();
        
        AssertEx.Throws<ConstraintViolationException>(() =>
          person.Name = "Bla-Bla-Bla longer than 20 characters.");

        person.Name = "Alex Kofman";
        person.Age = 26;
        person.RegistrationDate = new DateTime(2007, 1, 02);
        person.Phone = "(343) 111-22-33";
        person.Email = "alex.kofman@x-tensive.com";
        person.Height = 1.67;
        person.Validate();

        region.Complete();
      }

      using (var region = context.OpenInconsistentRegion()) {
        Person person = new Person();

        person.Name = "Mr. Unknown";
        person.Age = 100;
        person.RegistrationDate = DateTime.Now;
        person.Phone = null;
        person.Email = "";
        person.Height = 2;

        person.Validate();

        region.Complete();
      }
    }

    [Test]
    public void InheritanceTest()
    {
      context = new ValidationContext();

      var region = context.OpenInconsistentRegion();

      var person = new Person();
      AssertEx.Throws<AggregateException>(person.CheckConstraints);

      var advancedPerson = new AdvancedPerson();
      AssertEx.Throws<AggregateException>(advancedPerson.CheckConstraints);

      region.Dispose();
    }
  }
}