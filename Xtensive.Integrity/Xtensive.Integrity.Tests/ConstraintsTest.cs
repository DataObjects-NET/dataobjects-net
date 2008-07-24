// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.23

using System;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Integrity.Aspects.Constraints;
using Xtensive.Integrity.Validation;
using Xtensive.Integrity.Validation.Interfaces;

namespace Xtensive.Integrity.Tests
{
  [TestFixture]
  public class ConstraintsTest
  {
    public class ConstraintsValidationContext : ValidationContextBase {}    

    public static ConstraintsValidationContext Context = new ConstraintsValidationContext();

    public class Company : IValidationAware
    {
      public string Name { get; set;}


      [RegexConstraint("^www.", Mode = ValidationMode.Immediate)]
      public string WebSite { get; set;}

      public void OnValidate()
      {        
      }

      public bool IsCompatibleWith(ValidationContextBase context)
      {
        return context is ConstraintsValidationContext;
      }

      public ValidationContextBase Context
      {
        get { return ConstraintsTest.Context; }
      }
    }
      
    [Test]
    public void Test()
    {
      using (new ValidationScope(Context)) {
        Company xtensive = new Company();

        AssertEx.Throws<Exception>(delegate { xtensive.WebSite = "x-tensive.com"; });
        xtensive.WebSite = "www.x-tensive.com";
      }     
    }

  }
}