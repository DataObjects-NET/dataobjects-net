using System;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Issues.IssueJira0116_InterfacesCastAndIndexesModel
{
    public enum RecordState
    {
        Deleted = -1,
        Draft = 0,
        Normal = 1
    }

    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public abstract class Record : Entity, IRecord
    {
        /// <summary>
        /// To create an empty record
        /// </summary>
        protected Record(IParty createdBy)
        {
            CreatedBy = createdBy;
            DateCreated = DateTime.UtcNow;
            Status = RecordState.Draft;
        }

        /// <summary>
        /// key of table
        /// </summary>
        [Field, Key]
        public long Id { get; private set; }

        [Field]
        public RecordState Status { get; private set; }

        /// <summary>
        /// Username of user who created Record
        /// </summary>
        [Field]
        public IParty CreatedBy { get; set; }

        /// <summary>
        /// Date when Record was created
        /// </summary>
        [Field]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Date when record was last Modified
        /// </summary>
        [Field]
        public DateTime? DateModified { get; set; }
        
        /// <summary>
        /// method override from dataObjects .. called when a a value is set in a field on screen
        /// used for auditing
        /// Other features to add here: 
        /// -set fields that don't need tracking
        /// -set information for auditing
        /// -calculations/population of other values based on the value entered 
        /// </summary>
        /// <param name="field">field information</param>
        /// <param name="oldValue">previous value of field</param>
        /// <param name="newValue">current submitted value of field</param>
        protected override void OnSetFieldValue(FieldInfo field, object oldValue, object newValue)
        {
            base.OnSetFieldValue(field, oldValue, newValue);

            if (field.Name != "DateModified")
                DateModified = DateTime.UtcNow;
        }

        public void Delete()
        {
            Status = RecordState.Deleted;
        }

        //todo: find a better name for this method?
        public void SetStateToNormal()
        {
            Status = RecordState.Normal;
        }
    }
}
