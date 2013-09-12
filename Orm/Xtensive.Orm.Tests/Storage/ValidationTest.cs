// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.31

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Validation;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Tests.Storage.Validation
{ 
  public class ValidationTest : AutoBuildTest
  {
    [ThreadStatic]
    private static int validationCallsCount;

    [Serializable]
    [HierarchyRoot]
    public class Referrer : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field, NotNullConstraint]
      public Reference Reference { get; set; }

      [Field, NotNullOrEmptyConstraint]
      public string Title { get; set; }

      public Referrer(Reference reference, string title)
      {
        Reference = reference;
        Title = title;
      }

      public Referrer()
      {
      }
    }

    [Serializable]
    [HierarchyRoot]
    public class Reference : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field, NotNullOrEmptyConstraint]
      public string Title { get; set; }

      public Reference(string title)
      {
        Title = title;
      }

      public Reference()
      {
      }
    }

    [Serializable]
    [HierarchyRoot]
    public class Mouse : Entity
    {
      [Field, Key]
      public int ID { get; private set; }
        
      [Field]
      public int ButtonCount { get; set; }

      [Field]
      public int ScrollingCount { get; set; }

      [Field]
      public Led Led { get; set; }

      protected override void OnValidate()
      {
        validationCallsCount++;

        base.OnValidate();
        
        if (ButtonCount<1)
          throw new InvalidOperationException("Button count can't be less than one.");

        if (ScrollingCount > ButtonCount)
          throw new InvalidOperationException("Scrolling count can't be greater then button count.");

        if (Led.Brightness > 10)
          throw new InvalidOperationException("Led in the mouse is too bright.");
      }
    }

    [Serializable]
    public class Led : Structure
    {
      [Field]
      public double Brightness { get; set;}

      [Field]
      public double Precision { get; set;}

      protected override void OnValidate()
      {
        base.OnValidate();

        if (Brightness <= 0)
          throw new InvalidOperationException("Led brightness should be greater then 0.");

        if (Precision <= 0)
          throw new InvalidOperationException("Led precision should be greater then 0.");

        if (Precision > Brightness * 10)
          throw new InvalidOperationException("Invalid precision-brightness ratio.");
      }
    }

    [HierarchyRoot]
    public class ValidationTarget : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field, NotNullOrEmptyConstraint(IsImmediate = true)]
      public string Name { get; set; }

      [Field, NotNullOrEmptyConstraint(IsImmediate = true)]
      public string Description { get; set; }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Mouse).Namespace);
      return config;
    }

    [Test]
    public void ModelTest()
    {
      var mouseType = Domain.Model.Types[typeof (Mouse)];
      Assert.That(mouseType.Validators.Count, Is.EqualTo(1));
      Assert.That(mouseType.Validators[0], Is.InstanceOf<EntityValidator>());
      var ledField = mouseType.Fields["Led"];
      Assert.That(ledField.Validators.Count, Is.EqualTo(1));
      Assert.That(ledField.Validators[0], Is.InstanceOf<StructureFieldValidator>());
    }

    [Test]
    public void OnSetValueModeTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var target = new ValidationTarget();
          target.Name = "name";
          target.Description = "blah";
          AssertEx.Throws<ArgumentException>(() => target.Name = null);
          AssertEx.Throws<ArgumentException>(() => target.Name = string.Empty);
          AssertEx.Throws<ArgumentException>(() => target.Description = null);
          AssertEx.Throws<ArgumentException>(() => target.Description = string.Empty);
        }
      }
    }

    [Test]
    public void ValidationCallsCountTest()
    { 
      int mouseId;
      validationCallsCount = 0;
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var mouse = new Mouse {
            ButtonCount = 2,
            ScrollingCount = 1
          };
          mouse.Led.Brightness = 1.5;
          mouse.Led.Precision = 1.5;
          mouseId = mouse.ID;

          session.Validate();
          transactionScope.Complete();
        }
      }
      Assert.AreEqual(1, validationCallsCount);

      validationCallsCount = 0;
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          var mouse = session.Query.All<Mouse>().Where(m => m.ID==mouseId).First();
        }
      }
      // No validation calls on meterialization
      Assert.AreEqual(0, validationCallsCount); 
    }

    [Test]
    public void ValidateAllTest()
    {
      validationCallsCount = 0;

      using (var session = Domain.OpenSession()) {
        Mouse mouse;

        using (var tx = session.OpenTransaction()) {
          mouse = new Mouse {
            ButtonCount = 2,
            ScrollingCount = 1,
            Led = new Led {Brightness = 7.3, Precision = 33}
          };
          mouse.Led.Brightness = 4.3;

          session.Validate();
          Assert.AreEqual(1, validationCallsCount);

          mouse.Led.Brightness = 2.3;

          // Fails here!
          AssertEx.Throws<ValidationFailedException>(session.Validate);

          tx.Complete();
          AssertEx.Throws<ValidationFailedException>(tx.Dispose);
        } // Second .Dispose should do nothing!
      }
    }
    
    [Test]
    public void EntityValidation()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {
          // Created and modified invalid object. (ScrollingCount > ButtonCount)
          AssertEx.Throws<ValidationFailedException>(() => {
              new Mouse {ButtonCount = 2, ScrollingCount = 3, Led = new Led {Brightness = 1, Precision = 1}};
              session.Validate();
            });
        }
        using (session.OpenTransaction()) {
          // Created, but not modified invalid object.
          AssertEx.Throws<ValidationFailedException>(() => {
            new Mouse();
            session.Validate();
          });
        }
        using (session.OpenTransaction()) {
          // Invalid modification of existing object.
          AssertEx.Throws<ValidationFailedException>(() => {
            var m = new Mouse {ButtonCount = 1, ScrollingCount = 1, Led = new Led {Brightness = 1, Precision = 1}};
            m.ScrollingCount = 2;
            session.Validate();
          });
        }
        using (session.OpenTransaction()) {
          Mouse mouse;
          // Valid object - ok.

          mouse = new Mouse {ButtonCount = 5, ScrollingCount = 3};
          mouse.Led.Precision = 1;
          mouse.Led.Brightness = 2;
          session.Validate();

          // Valid modification with invalid intermediate state - ok.

          mouse.ButtonCount = 2;
          mouse.ScrollingCount = 1;
          session.Validate();

          // Invalid object is removed - ok.

          mouse.ScrollingCount = 3;
          mouse.Remove();
          session.Validate();
        }
      }
    }

    [Test]
    public void StructureValidation()
    {
      using (var session = Domain.OpenSession()) {
        Mouse mouse;

        using (var transactionScope = session.OpenTransaction()) {
          // Valid mouse is created.

          mouse = new Mouse {ButtonCount = 2, ScrollingCount = 1};
          mouse.Led = new Led {Brightness = 7.3, Precision = 33};
          session.Validate();
          transactionScope.Complete();
        }

        // Structure become invalid.
        using (var transactionScope = session.OpenTransaction()) {
          AssertEx.Throws<ValidationFailedException>(
            delegate {
              mouse.Led.Brightness = 2.3;
              session.Validate();
            });
        }
          
        // Changed structure make entity invalid.
        using (var transactionScope = session.OpenTransaction()) {
          AssertEx.Throws<ValidationFailedException>(
            delegate {
              mouse.Led.Brightness = 11;
              session.Validate();
            });
        }
      }
    }

    [Test]
    public void ValidationInConstructor()
    {
      using (var session = Domain.OpenSession()) 
      using (session.OpenTransaction()) {
        Reference reference1 = null;
        AssertEx.Throws<ValidationFailedException>(() => {
          reference1 = new Reference();
          session.Validate();
        });
        reference1.Remove();
        var reference2 = new Reference {Title = "Test"};
        session.Validate();

        var reference3 = new Reference("Test");
        Referrer referrer2 = null;
        AssertEx.Throws<ValidationFailedException>(() => {
          referrer2 = new Referrer();
          session.Validate();
        });
        referrer2.Remove();
        var referrer3 = new Referrer {Title = "Test", Reference = reference3};
        session.Validate();
        var referrer1 = new Referrer(reference3, "Test");
      }
    }

    [Test]
    public void GetValidationErrorsTest()
    {
      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var reference = new Reference("hello");
        var errors = session.ValidateAndGetErrors();
        Assert.That(errors, Is.Empty);
        reference.Title = string.Empty;
        errors = session.ValidateAndGetErrors();
        Assert.That(errors.Count, Is.EqualTo(1));
        Assert.That(errors[0].Target, Is.EqualTo(reference));
        Assert.That(errors[0].Errors.Count, Is.EqualTo(1));
      }
    }
  }
}
