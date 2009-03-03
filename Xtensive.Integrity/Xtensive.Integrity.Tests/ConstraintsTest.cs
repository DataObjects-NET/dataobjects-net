// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.23

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

    #region Nested types: ValidatableObject, AgedObject, NamedObject, WebObject

    internal class ValidatableObject : IValidationAware
    {
      public void OnValidate()
      {
        this.CheckConstraints();
      }

      ValidationContextBase IContextBound<ValidationContextBase>.Context
      {
        get { return Context; }
      }
    }

    internal class AgedObject : ValidatableObject
    {      
      [RangeConstraint(0, 100)]
      public int Age { get; set;}

//      [RangeConstraint(10)]
//      public AgedObject Wrong { get; set; }
    }

    internal class NamedObject : ValidatableObject
    {
      [RegexConstraint(@"^[^1-9]*$", Mode = ValidationMode.Immediate)]
      [RequiredConstraint(true)]
      public string Name { get; set;}      
    }

    internal class WebObject : ValidatableObject
    {      
      [RegexConstraint(@"^www\.")]
      public string Url { get; set;}
    }

    #endregion

    [Test]
    public void AgedObjectTest()
    {
      AssertEx.Throws<ConstraintViolationException>(() => {
        var a = new AgedObject {Age = (-1)};
      });
      AssertEx.Throws<ConstraintViolationException>(() => {
        var a = new AgedObject {Age = 101};
      });
      using (Context.OpenInconsistentRegion()) {
        var a = new AgedObject();
        a.Age = 101;
        a.Age = 100;
      }
      {
        var a = new AgedObject {Age = 100};
      }
    }

    [Test]
    public void NamedObjectTest()
    {
      AssertEx.Throws<ConstraintViolationException>(() => {
        var c = new NamedObject {Name = null};
      });
      AssertEx.Throws<ConstraintViolationException>(() => {
        var c = new NamedObject {Name = string.Empty};
      });
      AssertEx.Throws<AggregateException>(() => {
        using (Context.OpenInconsistentRegion()) {
          var c = new NamedObject();
          c.Name = "E1.ru"; // Throws CVE, since Name is validated in immediate mode
          c.Name = "Xtensive";
        } // Throws AE, since Name value is required
      });
      {
        var c = new NamedObject {Name = "Xtensive"};
      }
    }

    [Test]
    public void WebObjectTest()
    {
      using (Context.OpenInconsistentRegion()) {
        var w = new WebObject();
        w.Url = "x-tensive.com";
        w.Url = "www.x-tensive.com";
      }
      {
        var w = new WebObject {Url = "www.x-tensive.com"};
      }
      AssertEx.Throws<ConstraintViolationException>(() => {
        var w = new WebObject {Url = "x-tensive.com"};
      });
    }
  }
}