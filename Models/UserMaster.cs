using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace MarketAnalytics.Models
{
    public class UserMaster
    {
        [Key]
        public int USER_MASTER_ID { get; set; }

        [Required]
        [DisplayName("User Id")]
        public string USER_ID { get; set; }
        [Required]
        [DisplayName("Password")]
        public string USER_PWD { get; set; }
        [Required]
        [DisplayName("User Type")]
        public int USER_TYPE { get; set; }
        public ICollection<UserMaster> portfolioCollection { get; set; }

    }
}
