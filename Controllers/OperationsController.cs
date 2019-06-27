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

        [HttpPost("import/{year:int}")]
        public async Task<ActionResult<string>> Import(int year)
        {
            var nbImported = 0;

            try
            {
                //var documents = await _operations.Find(_ => true).ToListAsync();

                //check if already imported
                var isAlreadyImported = _operations
                    .Find(i => i.id_mutation.StartsWith(year.ToString()))
                    .Any();
                if (isAlreadyImported)
                    throw new Exception($"The year {year} has already been imported.");

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
                        csv.Configuration.Delimiter = ",";
                        var operations = csv.GetRecords<Operation>();

                        foreach (var operation in operations)
                        {
                            //Save item
                            operation.Id = ObjectId.GenerateNewId();
                            _operations.InsertOne(operation);
                            System.Diagnostics.Debug.WriteLine("Row: " + ++nbImported);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return Ok($"Fichier {year} importé: {nbImported} items");
        }
    }
}
