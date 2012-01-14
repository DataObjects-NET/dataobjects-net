using System;

namespace Xtensive.Orm.Tests.Issues.IssueJira0116_InterfacesCastAndIndexesModel
{
    public interface IRecord : IEntity
    {
        /// <summary>
        /// key of table
        /// </summary>
        [Field]
        long Id { get; }

        [Field]
        RecordState Status { get; }

        /// <summary>
        /// Username of user who created Record
        /// </summary>
        //[Field]
        //IParty CreatedBy { get; set; }

        /// <summary>
        /// Date when Record was created
        /// </summary>
        [Field]
        DateTime DateCreated { get; set; }

        /// <summary>
        /// Date when record was last Modified
        /// </summary>
        [Field]
        DateTime? DateModified { get; set; }

        void Delete();
        void SetStateToNormal();
    }
}

