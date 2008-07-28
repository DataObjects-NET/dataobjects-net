// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using System.Collections.Generic;
using System.Reflection;
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
    internal class ConstraintsValidationContext : ValidationContextBase {}    

    internal static ConstraintsValidationContext Context = new ConstraintsValidationContext();    

    internal class ValidatableObject : IValidatable
    {
      public void OnValidate()
      {
        this.CheckConstraints();        
      }

      public bool IsCompatibleWith(ValidationContextBase context)
      {
        return context == Context;
      }

      public ValidationContextBase Context
      {
        get { return ConstraintsTest.Context; }
      }
    }

    internal class NamedObject : ValidatableObject
    {
      [RegexConstraint("^LLC", Mode = ValidationMode.Immediate)]
      [NotNullOrEmptyConstraint(Mode = ValidationMode.ImmediateOrDelayed)]
      public string Name { get; set;}      
    }

    internal class Company : NamedObject
    {      
      [RegexConstraint("^www\\.", Mode = ValidationMode.ImmediateOrDelayed)]
      public string WebSite { get; set;}
    }

    [Test]
    public void Test()
    {
      using (new ValidationScope(Context)) {
        try { 
          using (Context.InconsistentRegion()) {
            Company xtensive = new Company();
            xtensive.WebSite = "x-tensive.com";
          }
        }
        catch (AggregateException e) {
          List<Exception> errors = e.GetFlattenList();
          Assert.AreEqual(2, errors.Count);
          
          foreach (var exception in errors)
            Assert.AreEqual(typeof (ConstraintViolationException), exception.GetType());

          return; 
        }

        throw new Exception(
          string.Format("{0} was not thrown.", typeof(AggregateException)));
      }
    }
  }
}