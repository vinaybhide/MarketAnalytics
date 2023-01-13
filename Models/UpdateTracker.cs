using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace MarketAnalytics.Models
{
    public class UpdateTracker
    {
        [Key]
        public int UpdateTracker_ID { get; set; }
        public DateTime REF_DATE { get; set; } = default(DateTime);
        public string TYPE { get; set; } = string.Empty;
        public string DATA { get; set; } = string.Empty;
        public int StockMasterID { get; set; }
    }
}
