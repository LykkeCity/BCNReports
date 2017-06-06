using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Models
{
    public class AddressTransactionsReportsRequest
    {
        [Required]
        public string BitcoinAddress { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
