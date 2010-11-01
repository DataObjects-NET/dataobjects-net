// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.23

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PostSharp.Aspects;
using Xtensive.IoC;
using Xtensive.Reflection;
using Xtensive.Orm;
using Xtensive.Orm.Validation;
using Xtensive.Testing;
using AggregateException = Xtensive.Core.AggregateException;
using ConstraintViolationException = Xtensive.Orm.Validation.ConstraintViolationException;

namespace Xtensive.Orm.Tests.Integrity
{
  [TestFixture]
  public class ConstraintsTest
  {
    internal static ValidationContext context = new ValidationContext();

    internal class ValidatableObject : IValidationAware
    {
      public void OnValidate()
      {
        this.CheckConstraints();
      }

      ValidationContext IContextBound<ValidationContext>.Context
      {
        get { return context; }
      }
    }

    #region LogMethodFastAspect

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true, Inherited = false)]
    [Serializable]
    internal class LogMethodFastAspect : OnMethodBoundaryAspect
    {
      public MethodBase Method { get; private set; }

      public override void OnEntry(MethodExecutionArgs args)
      {
        Log.Info("OnEntry called on {0}.", Method.GetShortName(true));
        args.MethodExecutionTag = "OnEntry";
      }

      public override void OnExit(MethodExecutionArgs args)
      {
        Log.Info("OnExit called.");
        Log.Info(string.Format("OnEntry result: {0}", args.MethodExecutionTag));
      }

      public override void OnSuccess(MethodExecutionArgs args)
      {
        Log.Info("OnSuccess called.");
      }

      public override void OnException(MethodExecutionArgs args)
      {
        Log.Error(args.Exception);
        args.FlowBehavior = FlowBehavior.RethrowException;
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
      [NotNullOrEmptyConstraint]
      [LengthConstraint(Max = 20, Mode = ConstrainMode.OnSetValue)]
      public string Name { get; set;}

      [RangeConstraint(Min = 0, Message = "Incorrect age ({value}), age can not be less than {Min}.")]
      public int Age { get; set;}

      [PastConstraint]
      public DateTime RegistrationDate { get; set;}

      [RegexConstraint(Pattern = @"^(\(\d+\))?[-\d ]+$", 
        MessageResourceType = typeof(TestResources) , MessageResourceName = "IncorrectPhoneFormat")]
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

      using (var region = context.DisableValidation()) {
        Person person = new Person();

        Assert.AreEqual(
          "Name can not be null or empty.", 
          person.GetPropertyValidationError("Name").Message);

        person.Name = "Alex Kofman";
        person.Age = -26;

        Assert.AreEqual(
          "Incorrect age (-26), age can not be less than 0.", 
          person.GetPropertyValidationError("Age").Message);

        Assert.AreEqual(
          null, 
          person.GetPropertyValidationError("Name"));
      }
    }

    [Test]
    public void PersonTest()
    {
      context = new ValidationContext();

      try {
        using (var region = context.DisableValidation()) {
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
        var errors = exception.GetFlatExceptions().Cast<Orm.Validation.ConstraintViolationException>();
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

      using (var region = context.DisableValidation()) {
        Person person = new Person();
        
        AssertEx.Throws<Validation.ConstraintViolationException>(() =>
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

      using (var region = context.DisableValidation()) {
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

      var region = context.DisableValidation();

      var person = new Person();
      AssertEx.Throws<AggregateException>(person.CheckConstraints);

      var advancedPerson = new AdvancedPerson();
      AssertEx.Throws<AggregateException>(advancedPerson.CheckConstraints);

      region.Dispose();
    }
  }
}