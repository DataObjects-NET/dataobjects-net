using NUnit.Framework;

namespace Xtensive.Orm.Tests.Core.DotNetFramework
{
  [TestFixture]
  public class ConstraintsTest
  {  
    public interface IConstraint
    {
      bool IsCorrect(object value);
    }

    public abstract class Constraint : IConstraint
    {
      public abstract bool IsCorrect(object value);
    }    
   
    public class IntConstraint : Constraint
    {
      public override bool IsCorrect(object value)
      {
        return (int)value < 100;
      }
    }

    public class StringConstraint : Constraint
    {           
      public override bool IsCorrect(object value)
      {
        return string.IsNullOrEmpty((string) value);
      }
    }

    public interface IConstraint<T>
    {
      bool IsCorrect(T value);
    }

    public abstract class Constraint<T> : IConstraint<T>
    {
      public abstract bool IsCorrect(T value);      
    }

    public class GenericIntConstraint : Constraint<int>
    {
      public override bool IsCorrect(int value)
      {
        return value < 100;
      }
    }

    public class GenericStringConstraint : IConstraint<string>
    {
      public virtual bool IsCorrect(string value)
      {
        return string.IsNullOrEmpty(value);
      }
    }

    public void TestBoxingInt(IConstraint constraint, int count)
    {
      for (int j = 0; j<count; j++) {
        object i = j;
        constraint.IsCorrect(i);
        constraint.IsCorrect(i);
        constraint.IsCorrect(i);
        constraint.IsCorrect(i);
        constraint.IsCorrect(i);
        constraint.IsCorrect(i);
        constraint.IsCorrect(i);
        constraint.IsCorrect(i);
        constraint.IsCorrect(i);
        constraint.IsCorrect(i);        
      }
    }
  
    const string s = "bla-bla-bla";

    public void TestBoxingString(IConstraint constraint, int count)
    {      
      for (int i = 0; i<count; i++) {
        constraint.IsCorrect(s);
        constraint.IsCorrect(s);
        constraint.IsCorrect(s);
        constraint.IsCorrect(s);
        constraint.IsCorrect(s);
        constraint.IsCorrect(s);
        constraint.IsCorrect(s);
        constraint.IsCorrect(s);
        constraint.IsCorrect(s);
        constraint.IsCorrect(s);
      }
    }

    public void TestGenericInt(object constraint, int count)
    {
      for (int i = 0; i<count; i++) {
        ((IConstraint<int>) constraint).IsCorrect(i);
        ((IConstraint<int>) constraint).IsCorrect(i);
        ((IConstraint<int>) constraint).IsCorrect(i);
        ((IConstraint<int>) constraint).IsCorrect(i);
        ((IConstraint<int>) constraint).IsCorrect(i);
        ((IConstraint<int>) constraint).IsCorrect(i);
        ((IConstraint<int>) constraint).IsCorrect(i);
        ((IConstraint<int>) constraint).IsCorrect(i);
        ((IConstraint<int>) constraint).IsCorrect(i);
        ((IConstraint<int>) constraint).IsCorrect(i);        
      }
    }

    public void TestGenericString(object constraint, int count)
    {
      for (int i = 0; i<count; i++) {
        ((IConstraint<string>) constraint).IsCorrect(s);
        ((IConstraint<string>) constraint).IsCorrect(s);
        ((IConstraint<string>) constraint).IsCorrect(s);
        ((IConstraint<string>) constraint).IsCorrect(s);
        ((IConstraint<string>) constraint).IsCorrect(s);
        ((IConstraint<string>) constraint).IsCorrect(s);
        ((IConstraint<string>) constraint).IsCorrect(s);
        ((IConstraint<string>) constraint).IsCorrect(s);
        ((IConstraint<string>) constraint).IsCorrect(s);
        ((IConstraint<string>) constraint).IsCorrect(s);
      }
    }     

    [Test]
    [Explicit]  
    [Category("Performance")]
    public void Test()
    {   
      const int iterations = 100000;

      using (new Measurement("Boxing int ", MeasurementOptions.Log, iterations * 10)) {

        Constraint constraint = new IntConstraint();
        TestBoxingInt(constraint, iterations);        
      }

      using (new Measurement("Generic int", MeasurementOptions.Log, iterations * 10)) {
        
        object constraint = new GenericIntConstraint();
        TestGenericInt(constraint, iterations);
      }                  

      using (new Measurement("Boxing string ", MeasurementOptions.Log, iterations * 10)) {

        Constraint constraint = new StringConstraint();
        TestBoxingString(constraint, iterations);         
      } 

      using (new Measurement("Generic string", MeasurementOptions.Log, iterations * 10  )) {

        object constraint = new GenericStringConstraint();
        TestGenericString(constraint, iterations);
      }
    }
  }
}
