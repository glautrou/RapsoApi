using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using RapsoApi.Model;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Security.Authentication;

namespace RapsoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OperationsController : ControllerBase
    {
        private readonly IMongoCollection<Operation> _operations;

        public OperationsController(IRapsoDatabaseSettings config)
        {
            // var client = new MongoClient(settings.ConnectionString);
            // var database = client.GetDatabase(settings.DatabaseName);

            // _operations = database.GetCollection<Operation>(settings.OperationsCollectionName);


            MongoClientSettings settings = MongoClientSettings.FromUrl(
                new MongoUrl(config.ConnectionString)
            );
            settings.SslSettings = new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            var mongoClient = new MongoClient(settings);
            var database = mongoClient.GetDatabase(config.DatabaseName);

            _operations = database.GetCollection<Operation>(config.OperationsCollectionName);
        }

        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        public Operation GetFirstByYear(string id) =>
            _operations.Find<Operation>(i => i.id_mutation == id).FirstOrDefault();

        public Operation Create(Operation operation)
        {
            _operations.InsertOne(operation);
            return operation;
        }

        [HttpPost("import/{year:int}")]
        public async Task<ActionResult<string>> Import(int year)
        {
            //check if already imported
            var a = GetFirstByYear("");


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
                        var created = Create(operation);
                        System.Diagnostics.Debug.WriteLine("Row: " + ++nbImported);
                    }
                }
            }

            var b = GetFirstByYear("");

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
