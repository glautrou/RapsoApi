using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using RapsoApi.Model;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RapsoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpPost("import/{year:int}")]
        public async Task<ActionResult<string>> Import(int year)
        {
            //check if already imported

            var nbImported = 0;

            //Download archive
            var url = $"https://cadastre.data.gouv.fr/data/etalab-dvf/latest/csv/{year}/full.csv.gz";
            var stream = await new HttpClient().GetStreamAsync(url);

            using (var file = new MemoryStream())
            {
                //Uncompress data
                using (var decompressionStream = new GZipStream(stream, CompressionMode.Decompress))
                    decompressionStream.CopyTo(file);

                file.Seek(0, SeekOrigin.Begin);
                file.Flush();

                //Read CSV
                using (var reader = new StreamReader(file))
                using (var csv = new CsvReader(reader))
                {
                    foreach (var operation in csv.GetRecords<Operation>())
                    {
                        //Save item

                        System.Diagnostics.Debug.WriteLine("Row: " + ++nbImported);
                    }
                }
            }

            return Ok($"Fichier {year} importé: {nbImported} items");
        }

        public static Stream Decompress(Stream stream)
        {
            var result = new MemoryStream();
            using (GZipStream decompressionStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(result);
            }

            return result;
        }
    }
}
