using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Auth.Api.Models;
public class ShopperSigninRequest
{
    public string Email { get; set; }


}
