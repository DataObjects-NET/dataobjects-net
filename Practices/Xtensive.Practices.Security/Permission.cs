
using System;
using System.Linq;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public abstract class Permission : ICloneable
  {
    public Type Type { get; private set; }

    public bool CanRead { get; private set; }

    public bool CanWrite { get; private set; }

    public object Clone()
    {
      return MemberwiseClone();
    }

    protected Permission(Type type, bool canWrite)
    {
      Type = type;
      CanRead = true;
      CanWrite = canWrite;
    }
  }

  public class Permission<T> : Permission where T : class, IEntity
  {
    public Func<ImpersonationContext, QueryEndpoint, IQueryable<T>> Query { get; protected set; }

    public Permission()
      : this(false)
    {
    }

    public Permission(bool canWrite)
      : base(typeof(T), canWrite)
    {
      SetDefaultQuery();
    }

    private void SetDefaultQuery()
    {
      IsDefaultQuery = true;
      Query = (context, query) => query.All<T>();
    }

    public bool IsDefaultQuery { get; private set; }

    public Permission(Func<ImpersonationContext, QueryEndpoint, IQueryable<T>> query)
      : this(false, query)
    {
    }

    public Permission(bool canWrite, Func<ImpersonationContext, QueryEndpoint, IQueryable<T>> query)
      : base(typeof(T), canWrite)
    {
      Query = query;
    }
  }
}
