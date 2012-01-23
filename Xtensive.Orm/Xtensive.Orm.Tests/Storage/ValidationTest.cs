// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.31

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Validation;
using Xtensive.Testing;
using Xtensive.Orm.Configuration;
using AggregateException = Xtensive.Core.AggregateException;

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

      [Field, NotNullOrEmptyConstraint(Mode = ConstrainMode.OnSetValue)]
      public string Name { get; set; }

      [Field, NotNullOrEmptyConstraint(Mode = ConstrainMode.OnSetValue)]
      public string Description { get; set; }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Mouse).Namespace);
      return config;
    }

    [Test]
    public void OnSetValueModeTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {

          var target = new ValidationTarget();
          target.Name = "name";
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
          using (var region = session.DisableValidation()) {
            var mouse = new Mouse {
              ButtonCount = 2,
              ScrollingCount = 1
            };
            mouse.Led.Brightness = 1.5;
            mouse.Led.Precision = 1.5;
            mouseId = mouse.ID;

            region.Complete();
          }
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
          using (var region = session.DisableValidation()) {
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
            AssertEx.Throws<AggregateException>(session.Validate);

            region.Complete();
            AssertEx.Throws<AggregateException>(region.Dispose);
          } // Second .Dispose should do nothing!

          tx.Complete();
          AssertEx.Throws<InvalidOperationException>(tx.Dispose);
        } // Second .Dispose should do nothing!
      }
    }
    
    [Test]
    public void EntityValidation()
    {
      using (var session = Domain.OpenSession()) {
        using (session.OpenTransaction()) {

          // Created and modified invalid object. (ScrollingCount > ButtonCount)
          AssertEx.Throws<AggregateException>(
            () => {
              using (var region = session.DisableValidation()) {
                new Mouse {ButtonCount = 2, ScrollingCount = 3, Led = new Led {Brightness = 1, Precision = 1}};
                region.Complete();
              }
            });
        }
        using (session.OpenTransaction()) {

          // Created, but not modified invalid object.
          AssertEx.Throws<AggregateException>(() =>
            new Mouse());
        }
        using (session.OpenTransaction()) {

          // Invalid modification of existing object.
          AssertEx.Throws<AggregateException>(
            () => {
              Mouse m;
              using (var region = session.DisableValidation()) {
                m = new Mouse {ButtonCount = 1, ScrollingCount = 1, Led = new Led {Brightness = 1, Precision = 1}};
                region.Complete();
              }
              m.ScrollingCount = 2;
            });
        }
        using (session.OpenTransaction()) {

          Mouse mouse;
          // Valid object - ok.
          using (var region = session.DisableValidation()) {
            mouse = new Mouse {ButtonCount = 5, ScrollingCount = 3};
            mouse.Led.Precision = 1;
            mouse.Led.Brightness = 2;
            region.Complete();
          }

          // Valid modification with invalid intermediate state - ok.
          using (var region = session.DisableValidation()) {
            mouse.ButtonCount = 2;
            mouse.ScrollingCount = 1;
            region.Complete();
          }

          // Invalid object is removed - ok.
          using (var region = session.DisableValidation()) {
            mouse.ScrollingCount = 3;
            mouse.Remove();
            region.Complete();
          }
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
          using (var region = session.DisableValidation()) {
            mouse = new Mouse {ButtonCount = 2, ScrollingCount = 1};
            mouse.Led = new Led {Brightness = 7.3, Precision = 33};
            region.Complete();
          }
          transactionScope.Complete();
        }

        // Structure become invalid.
        using (var transactionScope = session.OpenTransaction()) {
          AssertEx.Throws<AggregateException>(
            delegate {
              mouse.Led.Brightness = 2.3;
              transactionScope.Complete();
            });
        }
          
        // Changed structure make entity invalid.
        using (var transactionScope = session.OpenTransaction()) {
          AssertEx.Throws<AggregateException>(
            delegate {
              mouse.Led.Brightness = 11;
              transactionScope.Complete();
            });
        }
      }
    }

    [Test]
    public void TransactionsValidation()
    {
      using (var session = Domain.OpenSession()) {

        // Inconsistent region can not be opened without transaction.
        AssertEx.ThrowsInvalidOperationException(() => session.DisableValidation());

        // Transaction can not be committed while validation context is in inconsistent state.
          AssertEx.ThrowsInvalidOperationException(() => {
          using (var t = session.OpenTransaction()) {
            session.DisableValidation();
            t.Complete();
          }
        });

        using (var transactionScope = session.OpenTransaction()) {
          try {
            using (var region = session.DisableValidation()) {
              var mouse = new Mouse();
              throw new Exception("Test");
              // region.Complete();
            }
          }
          catch (Exception exception) {
            Assert.AreEqual("Test", exception.Message);
          }
//          Assert.IsFalse(Session.Current.ValidationContext.IsValid);
        }
      }
    }

    [Test]
    public void ValidationInConstructor()
    {
      using (var session = Domain.OpenSession()) 
      using (session.OpenTransaction()) {
        AssertEx.Throws<Exception>(() => {
          var reference1 = new Reference();
        });
        using (var region = session.DisableValidation()) {
          var reference2 = new Reference {
            Title = "Test"
          };
          region.Complete();
        }
        var reference = new Reference("Test");

        AssertEx.Throws<Exception>(() => {
          var referrer1 = new Referrer();
        });
        using (var region = session.DisableValidation()) {
          var referrer2 = new Referrer {
            Title = "Test", 
            Reference = reference
          };
          region.Complete();
        }
        var referrer = new Referrer(reference, "Test");
      }
    }
  }
}
