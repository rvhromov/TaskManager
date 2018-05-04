using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TaskManager.Models
{
    // Set up user's profile
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public virtual ICollection<TaskModel> Tasks { get; set; }

        public ApplicationUser() { }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            return userIdentity;
        }
    }

    // Configure DB context
    public class ApplicationContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationContext() : base("TaskmgrDb") { }
        public DbSet<TaskModel> Tasks { get; set; }

        public static ApplicationContext Create()
        {
            return new ApplicationContext();
        }
    }
}