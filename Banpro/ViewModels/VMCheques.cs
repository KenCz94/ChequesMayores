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

        [Required]
        [Display(Name = "Archivo")]
        public List<IFormFile> FormFiles { get; set; }
        public List<ImagenCheque> Imagenes { get; set; }
        public ImagenCheque Imagen { get; set; }
    }
}
