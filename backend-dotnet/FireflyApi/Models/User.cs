using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FireflyApi.Models
{
    public class User : IdentityUser<int>
    {
        [Required]
        [StringLength(50)]
        public string FullName { get; set; } = string.Empty;

        public bool Disabled { get; set; } = false;
    }
}