using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.FurnitureStore.Shared.DTOs
{
    public  class TokenRequst
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToeken { get; set; }
    }
}
