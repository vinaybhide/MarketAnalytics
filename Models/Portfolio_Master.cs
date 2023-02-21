﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace MarketAnalytics.Models
{
    public class Portfolio_Master
    {
        [Key]
        [DisplayName("Portfolio ID")]
        public int PORTFOLIO_MASTER_ID { get; set; }

        [Required]
        [DisplayName("Name")]
        
        public string PORTFOLIO_NAME { get; set; } = string.Empty;

        public string Id { get; set; } = string.Empty;
        
        public IList<PORTFOLIOTXN> collectionTxn { get; set; }

    }
}
