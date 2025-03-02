using System;
using System.Collections.Generic;

namespace UPPPDGenerator.Managers
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Template> Templates { get; set; } = new List<Template>();
        public virtual ICollection<TemplateAccess> TemplateAccesses { get; set; } = new List<TemplateAccess>();
    }

    public class Template
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PasswordHash { get; set; }

        public virtual User CreatedByUser { get; set; } // Связь с автором
        public virtual ICollection<TemplateAccess> TemplateAccesses { get; set; } = new List<TemplateAccess>();
    }

    public class TemplateAccess
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TemplateId { get; set; }
        public DateTime AccessGrantedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public virtual User User { get; set; }
        public virtual Template Template { get; set; }
    }

    public class VerificationCode
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }

        public virtual User User { get; set; }
    }
}
