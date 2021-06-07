using Banpro.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Banpro.ViewModels
{
    public class VMCheques
    {
        public List<IFormFile> FormFiles { get; set; }
        public List<ImagenCheque> Imagenes { get; set; }
    }
}
