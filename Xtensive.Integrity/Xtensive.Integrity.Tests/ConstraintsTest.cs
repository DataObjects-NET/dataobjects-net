// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.23

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Integrity.Aspects.Constraints;
using Xtensive.Integrity.Validation;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Tests
{
  [TestFixture]
  public class ConstraintsTest
  {
    internal class ValidationContext : ValidationContextBase {}

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

    internal class Person : ValidatableObject
    {
      [NotNullOrEmptyConstraint]
      [LengthConstraint(Max = 20, Mode = ValidationMode.Immediate)]
      public string Name { get; set;}

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

    [Test]
    public void PersonTest()
    {
      
      try {
        using (context.OpenInconsistentRegion()) {
          Person person = new Person();
          
          person.Age = -1;
          person.RegistrationDate = new DateTime(2100, 1, 1);
          person.Phone = "one-one-two-four-seven-one";
          person.Email = "my name@my domain";
          person.Height = 2.15;

          person.Validate();
        }
      }
      catch(AggregateException exception) {
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
        Assert.AreEqual("Height can not be less than 1 or greater than 2,13.", heightError.Message);
      }

      using (context.OpenInconsistentRegion()) {
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
      }

      using (context.OpenInconsistentRegion()) {
        Person person = new Person();

        person.Name = "Mr. Unknown";
        person.Age = 100;
        person.RegistrationDate = DateTime.Now;
        person.Phone = null;
        person.Email = "";
        person.Height = 2;

        person.Validate();
      }
    }
  }
}