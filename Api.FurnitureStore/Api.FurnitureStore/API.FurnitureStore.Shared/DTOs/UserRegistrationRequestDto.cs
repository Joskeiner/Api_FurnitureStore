using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.FurnitureStore.Shared.DTOs
{
    // este va hacer el req  
    // lo que se va apedir a todos aquellos que usen el endpoint de registro
    public  class UserRegistrationRequestDto
    {
        [Required]
        public string? Name { get; set; }

        [Required]

        public string? EmailAddress { get; set; }

        [Required]
        public string? Password { get; set; } 

    }
}
