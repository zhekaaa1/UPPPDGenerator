using System;
using System.Collections.Generic;

namespace UPPPDGenerator.Managers
{
    public partial class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<TemplateAccess> TempAccesses { get; set; } = new List<TemplateAccess>();
        public virtual ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();
    }

    public partial class TemplateAccess
    {
        public int Id { get; set; }
        public DateTime AccessGrantedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsAuthor { get; set; }
        public virtual ICollection<Template> Templates { get; set; } = new List<Template>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }

    public partial class Template
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual ICollection<TemplateAccess> TempAccesses { get; set; } = new List<TemplateAccess>();
    }

    public partial class VerificationCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }    
}
