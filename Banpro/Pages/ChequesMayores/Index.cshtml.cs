using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Banpro.Models;
using Banpro.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Web.Helpers;

namespace Banpro.Pages.ChequesMayores
{
    public class IndexModel : PageModel
    {

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public IndexModel(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult OnGet()
        {

            try
            {
                string Fecha = DateTime.Now.ToString("ddMMyyyy");
                string PathUploads = ("\\Uploads");
                string PathDestinoServer = ("/Uploads" + @"/vmCheques_Mayores_" + Fecha);
                string PathChequesServidor = "vmCheques_Mayores_" + Fecha;
                string PathDestinoServerFinal = ("/Uploads" + @"/vmCheques_Mayores_" + Fecha + @"/");
                string NombreArchivo = @"\vmCheques_Mayores_" + Fecha + ".zip";
                string NombreArchivoDelete = "vmCheques_Mayores_" + Fecha + ".zip";
                string RutaArchivoInicial = PathDestinoServer + NombreArchivo;
                string RutaCompartida = _configuration.GetValue<string>("RutaCompartida");
                DirectoryInfo PathServidor = new DirectoryInfo(RutaCompartida);
                DirectoryInfo PathAppUploads = new DirectoryInfo(_webHostEnvironment.ContentRootPath + PathUploads);


                if (Directory.Exists(RutaCompartida))
                {

                    foreach (FileInfo fi in PathServidor.GetFiles())
                    {
                        fi.Delete();
                    }

                    foreach (DirectoryInfo di in PathServidor.GetDirectories())
                    {
                        foreach (FileInfo f in di.GetFiles().Where(x => x.Name != NombreArchivoDelete))
                        {
                            f.Delete();
                        }
                        di.Delete();
                    }
                }

                if (Directory.Exists(_webHostEnvironment.ContentRootPath + PathUploads))
                {
                    foreach (FileInfo fiu in PathAppUploads.GetFiles())
                    {
                        fiu.Delete();
                    }

                    foreach (DirectoryInfo diu in PathAppUploads.GetDirectories())
                    {
                        foreach (FileInfo fiu in diu.GetFiles().Where(x => x.Name != NombreArchivoDelete))
                        {
                            fiu.Delete();
                        }

                    }

                    foreach (DirectoryInfo diu2 in PathAppUploads.GetDirectories().Where(x => x.Name != PathChequesServidor))
                    {
                        diu2.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ("Ocurrio un error al preparar la aplicación, Error: " + ex.Message);               

            }
            return Page();
        }

        [BindProperty]
        public VMCheques VMCheques { get; set; }

        public IActionResult OnPost(VMCheques vmCheques)
        {
            try
            {
                if (!vmCheques.FormFiles.Any())
                {
                    ViewData["Error"] = "No se cargo ningun archivo para procesar";
                    return Page();
                }

                List<ImagenCheque> ListImagenes = new List<ImagenCheque>();

                foreach (IFormFile item in vmCheques.FormFiles)
                {
                    if (item != null)
                    {

                        string Fecha = DateTime.Now.ToString("ddMMyyyy");
                        string PathDestinoServer = ("/Uploads" + @"/vmCheques_Mayores_" + Fecha);
                        string PathChequesServidor = "vmCheques_Mayores_" + Fecha;
                        string PathDestinoServerFinal = ("/Uploads" + @"/vmCheques_Mayores_" + Fecha + @"/");
                        string NombreArchivo = @"\vmCheques_Mayores_" + Fecha + ".zip";
                        string RutaArchivoInicial = PathDestinoServer + NombreArchivo;
                        string RutaCompartida = _configuration.GetValue<string>("RutaCompartida");
                        string RutaFisica = _configuration.GetValue<string>("RutaFisica");
                        string RutaArchivoFinal = RutaFisica + NombreArchivo;
                        string RutaCompartidaFinal = RutaCompartida + NombreArchivo;

                        using (StreamReader Lector = new StreamReader(item.OpenReadStream()))
                        {
                            while (Lector.Peek() > -1)
                            {
                                string LineaUnica = Lector.ReadLine();

                                if (!String.IsNullOrEmpty(LineaUnica))
                                {
                                    string[] ResultadoLinea = LineaUnica.Split('|');

                                    if (!String.IsNullOrEmpty(ResultadoLinea[0].ToString()) && !String.IsNullOrEmpty(ResultadoLinea[1].ToString()))
                                    {
                                        string CkFrenteCompleto = ResultadoLinea[0].ToString();
                                        string CkFrente = CkFrenteCompleto.Length == 74 ? CkFrenteCompleto.Substring(38, 36) :
                                                          CkFrenteCompleto.Length == 73 ? CkFrenteCompleto.Substring(38, 35) : "";

                                        string CkReverso = ResultadoLinea[1].ToString();
                                        string CkFrenteRuta = CkFrenteCompleto.Substring(0, 9);
                                        string CkFrenteCuenta = CkFrenteCompleto.Substring(9, 10);
                                        string CkFrenteNumCheque = CkFrenteCompleto.Substring(19, 7);
                                        string MontoCompleto = CkFrenteCompleto.Substring(26, 8);
                                        string Decimales = CkFrenteCompleto.Substring(34, 4);
                                        string MontoFinal = MontoCompleto + "." + Decimales.Substring(0, 2);
                                        decimal CkFrenteMonto = decimal.Parse(MontoFinal);
                                        string CkFrenteMoneda = CkFrenteCompleto.Substring(47, 1);

                                        ImagenCheque ImagenUnica = new ImagenCheque()
                                        {
                                            NombreFrenteCompleto = CkFrenteCompleto,
                                            NombreFrente = CkFrente,
                                            NombreReverso = CkReverso,
                                            Ruta = CkFrenteRuta,
                                            Cuenta = CkFrenteCuenta,
                                            NumCheque = CkFrenteNumCheque,
                                            Monto = CkFrenteMonto,
                                            Moneda = CkFrenteMoneda
                                        };

                                        if (ImagenUnica != null && (!String.IsNullOrEmpty(ImagenUnica.NombreFrente)) &&
                                                                   (!String.IsNullOrEmpty(ImagenUnica.NombreReverso)) &&
                                                                   (!String.IsNullOrEmpty(ImagenUnica.Ruta)) &&
                                                                   (!String.IsNullOrEmpty(ImagenUnica.Cuenta)) &&
                                                                   (!String.IsNullOrEmpty(ImagenUnica.NumCheque)) &&
                                                                   (!String.IsNullOrEmpty(ImagenUnica.Moneda)))
                                        {

                                            bool ImagenExiste = ListImagenes.Any(x => x.NombreFrenteCompleto == ImagenUnica.NombreFrenteCompleto);

                                            if (!ImagenExiste)
                                            {

                                                if (ImagenUnica.Moneda.Equals("1") && ImagenUnica.Monto >= Int32.Parse(_configuration["rangoNIO"]))
                                                {
                                                    ListImagenes.Add(ImagenUnica);
                                                }

                                                if (ImagenUnica.Moneda.Equals("2") && ImagenUnica.Monto >= Int32.Parse(_configuration["rangoUSD"]))
                                                {
                                                    ListImagenes.Add(ImagenUnica);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }



                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ("Ocurrio un error al procesar los archivos, Error: " + ex.Message);
            }

            return RedirectToPage("./Index");
        }
    }


}
