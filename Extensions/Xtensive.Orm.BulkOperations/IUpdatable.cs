using Xtensive.Orm.BulkOperations;

namespace Xtensive.Orm
{
  /// <summary>
  /// Marker interface for correct work of <c>BulkExtensions.Update</c> methods.
  /// </summary>
  /// <typeparam name="T">Type of the entity.</typeparam>
  public interface IUpdatable<T> where T : IEntity
  {
  }
}