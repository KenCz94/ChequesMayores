using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Banpro.Models
{
    public class ImagenCheque
    {
        public string NombreFrenteCompleto { get; set; }
        public string NombreFrente { get; set; }
        public string NombreReverso { get; set; }
        public string Ruta { get; set; }
        public string Cuenta { get; set; }
        public string NumCheque { get; set; }
        public string Moneda { get; set; }
        public decimal Monto { get; set; }

    }
}
