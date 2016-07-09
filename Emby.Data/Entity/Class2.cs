using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Linq;
using System.ComponentModel;

namespace Emby.Data.Entity
{
    public enum EntityType { User, Group, Media, Device}

    [Table("entities")]
    public class BaseEntity
    {
        [Column("uid"), Key, ForeignKey("uid")]
        public Guid Id { get; protected set; }

        [Column("last_modified")]
        public DateTime LastModified { get; protected set; }

        [Column("last_saved")]
        public DateTime LastSaved { get; protected set; }

        [Column("created")]
        public DateTime Created { get; protected set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("type")]
        public EntityType Type { get; protected set; }

        [Column("picture_path")]
        public string PicturePath { get; set; }

    }

    [Table("users_data")]
    public class UserData
    {
        [Column("uid"), Key, ForeignKey("uid")]
        public Guid Id { get; protected set;}

        [Column("login_name")]
        public string LoginName { get; set; }

        [Column("password",TypeName ="CHAR[40]")]
        public string Password { get; set; }   

        [Column("salt")]
        public string Salt { get; set; }

        [Column("pin")]
        public string Pin { get; set; }

        [Column("last_activity")]
        public string LastActivity { get; set; }

    }

    [Table("external_entities")]
    public class ExternalEntities
    {

        [Column("uid"), ForeignKey("uid")]
        public Guid Id { get; protected set; }

        [Column("external_uid",Order = 1), Key]
        public string ExternalUid { get; protected set; }

        [Column("fqdn", Order = 0), Key]
        public string FQDN { get; protected set; }

        [Column("cn")]
        public string CommonName { get; set; }

        [Column("rdn")]
        public string RDN { get; set; }
    }

    [Table("policy_holders")]
    public class PolicyHolder
    {
        [Column("uid"), Key, ForeignKey("uid")]
        public Guid Id { get; protected set; }

        [Column("memberOf")]
        public ICollection<PolicyHolder> MemberOf { get; set; }

        [Column("policy",TypeName ="BLOB")]
        protected byte[] policy { get; set; }

        public void GetPolicy()
        {
            
        }
        [Column("external_entities")]
        ICollection<ExternalEntities> ExternalEntities { get; set; }
   }
}
