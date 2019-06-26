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
using System;
using MongoDB.Bson;
using CsvHelper.Configuration;

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
            var nbImported = 0;

            try
            {
                var documents = await _operations.Find(_ => true).ToListAsync();

                //check if already imported
                var a = GetFirstByYear("");

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

                    //using (var fileStream = System.IO.File.Create("file.csv"))
                    //{
                    //    file.Seek(0, SeekOrigin.Begin);
                    //    file.CopyTo(fileStream);
                    //}
                    //file.Seek(0, SeekOrigin.Begin);
                    //file.Flush();

                    //Read CSV
                    using (var reader = new StreamReader(file))
                    using (var csv = new CsvReader(reader))
                    {
                        var operations = csv.GetRecords<Operation>();
                        foreach (var operation in operations)
                        {
                            //Save item
                            operation.Id = ObjectId.GenerateNewId();
                            var created = Create(operation);
                            System.Diagnostics.Debug.WriteLine("Row: " + ++nbImported);
                        }
                    }
                }

                var b = GetFirstByYear("");
            }
            catch (Exception ex)
            {
                throw;
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
