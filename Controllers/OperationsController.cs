using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using RapsoApi.Model;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            //var year = 2019;
            //check if already imported

            //download file
            var url = $"https://cadastre.data.gouv.fr/data/etalab-dvf/latest/csv/{year}/full.csv.gz";
            var stream = await new HttpClient().GetStreamAsync(url);
            var file = new MemoryStream();
            using(var decompressionStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(file);
            }

            using (var reader = new StreamReader(file))
            using (var csv = new CsvReader(reader))
            {
                var operations = csv.GetRecords<Operation>();
            }

            return Ok($"Fichier {year} importé");
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
