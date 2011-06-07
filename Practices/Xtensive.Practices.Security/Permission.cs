
using System;
using System.Linq;
using Xtensive.Orm;

namespace Xtensive.Practices.Security
{
  public abstract class Permission
  {
    public Type Type { get; private set; }

    public bool CanRead { get; private set; }

    public bool CanWrite { get; private set; }

    public Func<ImpersonationContext, QueryEndpoint, IQueryable<IEntity>> Query { get; protected set; }

    #region GetHashcode & Equals members

    public bool Equals(Permission other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Equals(other.Type, Type) && other.CanRead.Equals(CanRead) && other.CanWrite.Equals(CanWrite);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != typeof (Permission)) return false;
      return Equals((Permission) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        int result = (Type != null ? Type.GetHashCode() : 0);
        result = (result*397) ^ CanRead.GetHashCode();
        result = (result*397) ^ CanWrite.GetHashCode();
        return result;
      }
    }

    #endregion

    protected Permission(Type type, bool canWrite)
    {
      Type = type;
      CanRead = true;
      CanWrite = canWrite;
    }
  }

  public class Permission<T> : Permission where T : class, IEntity
  {
    #region GetHashcode & Equals members

    public bool Equals(Permission<T> other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return base.Equals(other) && Equals(other.Query, Query);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      return Equals(obj as Permission<T>);
    }

    public override int GetHashCode()
    {
      unchecked {
        int result = base.GetHashCode();
        result = (result*397) ^ (Query != null ? Query.GetHashCode() : 0);
        return result;
      }
    }

    #endregion

    public Permission()
      : this(false)
    {
    }

    public Permission(bool canWrite)
      : base(typeof(T), canWrite)
    {
    }

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
