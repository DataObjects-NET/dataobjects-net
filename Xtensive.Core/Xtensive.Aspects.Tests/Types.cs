namespace Xtensive.Aspects.Tests
{
  public class Argument
  {

  }

  [ImplementConstructor(typeof(string))]
  [ImplementConstructor(typeof(Argument))]
  public class Base
  {
    public string Message { get; private set; }

    protected Base()
    { }

    protected Base(string message)
    {
      Message = message;
    }

    protected Base(Argument argument)
    {

    }
  }

  public class Generic<T> : Base
  {
    public Generic()
    { }

    protected Generic(string message, bool ignore)
      : base(message)
    { }
  }

  public class GenericDescendant<T> : Generic<T>
  {
    public GenericDescendant()
    { }

    protected GenericDescendant(string message, bool ignore)
      : base(message, ignore)
    { }
  }

  public class Descendant : Generic<string>
  {
  }

  public class ManualDescendant : Generic<string>
  {
    protected ManualDescendant(string message)
      : base(message, true)
    { }
  }

  public class ManualDescendantInheritor : ManualDescendant
  {
    protected ManualDescendantInheritor(string message)
      : base(message)
    { }
  }
}