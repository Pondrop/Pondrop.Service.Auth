using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Auth.Api.Models;
public class TokenRequest
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public bool IsAdmin { get; set; }
}
