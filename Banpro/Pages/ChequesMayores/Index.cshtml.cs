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
using Ionic.Zip;
using System.Web;
using System.IO.Compression;
using ZipFile = Ionic.Zip.ZipFile;

namespace Banpro.Pages.ChequesMayores
{
    public class IndexModel : PageModel
    {

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _fecha = DateTime.Now.ToString("ddMMyyyy");
        private readonly string _pathUploads = DateTime.Now.ToString("ddMMyyyy");
        private readonly string _pathDestinoServer;
        private readonly string _pathChequesServidor;
        private readonly string _pathDestinoServerFinal;
        private readonly string _nombreArchivo;


        public IndexModel(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _pathUploads = ("\\Uploads");
            _pathDestinoServer = ("/Uploads" + @"/Cheques_Mayores_" + _fecha);
            _pathChequesServidor = "Cheques_Mayores_" + _fecha;
            _pathDestinoServerFinal = ("/Uploads" + @"/Cheques_Mayores_" + _fecha + @"/");
            _nombreArchivo = @"\Cheques_Mayores_" + _fecha + ".zip";
        }

        public void OnGet()
        {
            try
            {
                DirectoryInfo PathUploads = new DirectoryInfo(_webHostEnvironment.ContentRootPath + _pathUploads);
                DirectoryInfo PathImagesUploads = new DirectoryInfo(_webHostEnvironment.ContentRootPath + _pathDestinoServerFinal);

                if (Directory.Exists(_webHostEnvironment.ContentRootPath + _pathUploads))
                {

                    foreach (FileInfo fi in PathImagesUploads.GetFiles())
                    {
                        fi.Delete();
                    }

                    foreach (DirectoryInfo di in PathUploads.GetDirectories())
                    {
                        di.Delete();
                    }

                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ("Ocurrio un error al preparar la aplicación, Error: " + ex.Message);
            }
            VMCheques = new VMCheques();

        }

        [BindProperty]
        public VMCheques VMCheques { get; set; }

        public IActionResult OnPost(VMCheques vmCheques)
        {
            List<ImagenCheque> ListaCheques = new List<ImagenCheque>();
            DirectoryInfo PathImagenes = new DirectoryInfo(_configuration.GetValue<string>("RutaImagenes"));
            List<FileInfo> Archivos = new List<FileInfo>();

            try
            {
                if (!vmCheques.FormFiles.Any())
                {
                    ViewData["Error"] = "No se cargo ningun archivo para procesar";
                    return Page();
                }

                foreach (IFormFile item in vmCheques.FormFiles)
                {
                    if (item != null)
                    {
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

                                        ImagenCheque Cheque = new ImagenCheque()
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

                                        if (Cheque != null && (!String.IsNullOrEmpty(Cheque.NombreFrente)) &&
                                                                   (!String.IsNullOrEmpty(Cheque.NombreReverso)) &&
                                                                   (!String.IsNullOrEmpty(Cheque.Ruta)) &&
                                                                   (!String.IsNullOrEmpty(Cheque.Cuenta)) &&
                                                                   (!String.IsNullOrEmpty(Cheque.NumCheque)) &&
                                                                   (!String.IsNullOrEmpty(Cheque.Moneda)))
                                        {

                                            bool ChequeExiste = ListaCheques.Any(x => x.NombreFrenteCompleto == Cheque.NombreFrenteCompleto);

                                            if (!ChequeExiste)
                                            {

                                                if (Cheque.Moneda.Equals("1") && Cheque.Monto >= Int32.Parse(_configuration["rangoNIO"]))
                                                {
                                                    ListaCheques.Add(Cheque);
                                                }

                                                if (Cheque.Moneda.Equals("2") && Cheque.Monto >= Int32.Parse(_configuration["rangoUSD"]))
                                                {
                                                    ListaCheques.Add(Cheque);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                VMCheques.Imagenes = ListaCheques;

                if (!ListaCheques.Any())
                {
                    ViewData["Error"] = ("No existen cheques mayores en los archivos procesados");
                    return Page();
                }

                foreach (DirectoryInfo di in PathImagenes.GetDirectories())
                {
                    foreach (FileInfo f in di.GetFiles())
                    {
                        Archivos.Add(f);
                    }
                }

                if (!Archivos.Any())
                {
                    foreach (FileInfo f in PathImagenes.GetFiles())
                    {
                        Archivos.Add(f);
                    }
                }

                if (!Archivos.Any())
                {
                    ViewData["Error"] = ("Los archivos fueron procesados exitosamente, pero no se encontraron las imagenes de los cheques en la carpeta correspondiente");
                    return Page();
                }

                if (!(Directory.Exists(_webHostEnvironment.ContentRootPath + _pathUploads)))
                {
                    Directory.CreateDirectory(_webHostEnvironment.ContentRootPath + _pathUploads);
                }

                if (!(Directory.Exists(_webHostEnvironment.ContentRootPath + _pathDestinoServer)))
                {
                    Directory.CreateDirectory(_webHostEnvironment.ContentRootPath + _pathDestinoServer);
                }

                List<string> ListaImagenes = new List<string>();

                foreach (ImagenCheque imagen in ListaCheques)
                {
                    if (!String.IsNullOrEmpty(imagen.NombreFrente))
                    {
                        ListaImagenes.Add(imagen.NombreFrente);
                    }

                    if (!String.IsNullOrEmpty(imagen.NombreReverso))
                    {
                        ListaImagenes.Add(imagen.NombreReverso);
                    }
                }

                if (ListaImagenes.Any())
                    CopyImages(ListaImagenes, Archivos, _pathDestinoServerFinal);
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ("Ocurrio un error al procesar los archivos, Error: " + ex.Message);
            }
            return Page();
        }

        private void CopyImages(List<string> listaImagenesCopy, List<FileInfo> archivos, string pathDestinoServerFinal)
        {
            foreach (FileInfo f in archivos)
            {
                if (f != null)
                {

                    string Imagen = listaImagenesCopy.Find(x => x == f.Name);

                    if (Imagen != null)
                    {

                        if (!(System.IO.File.Exists(_webHostEnvironment.ContentRootPath + pathDestinoServerFinal + f.Name)))
                        {

                            if (System.IO.File.Exists(Path.Combine(f.DirectoryName + @"\", f.Name)))
                            {
                                System.IO.File.Copy(Path.Combine(f.DirectoryName + @"\", f.Name), _webHostEnvironment.ContentRootPath + pathDestinoServerFinal + f.Name, true);
                            }
                        }
                    }
                }

            }
            ViewData["SuccessMessage"] = "La carpeta que contiene el consolidado de Cheques Mayores fue generada exitosamente";
            ViewData["DownloadFile"] = true;

        }

        public async Task<IActionResult> OnGetFile(VMCheques vmCheques)
        {
            try
            {
                var botsFolderPath = _webHostEnvironment.ContentRootPath + _pathDestinoServer;
                var botFilePaths = Directory.GetFiles(botsFolderPath);
                var zipFileMemoryStream = new MemoryStream();
                using (ZipArchive archive = new ZipArchive(zipFileMemoryStream, ZipArchiveMode.Update, leaveOpen: true))
                {
                    foreach (var botFilePath in botFilePaths)
                    {
                        var botFileName = Path.GetFileName(botFilePath);
                        var entry = archive.CreateEntry(botFileName);
                        using (var entryStream = entry.Open())
                        using (var fileStream = System.IO.File.OpenRead(botFilePath))
                        {
                            await fileStream.CopyToAsync(entryStream);
                        }
                    }
                }

                zipFileMemoryStream.Seek(0, SeekOrigin.Begin);
                return File(zipFileMemoryStream, "application/octet-stream", _nombreArchivo);

            }
            catch (Exception ex)
            {
                ViewData["Error"] = "Ha ocurrido un error al descargar el archivo Error: " + ex.Message;
                VMCheques = vmCheques;
                return Page();
            }
        }
    }




}
