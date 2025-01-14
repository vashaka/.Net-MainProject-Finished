using Microsoft.AspNetCore.Identity;

namespace MvcProject.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? WalletId { get; set; }
    }
}
