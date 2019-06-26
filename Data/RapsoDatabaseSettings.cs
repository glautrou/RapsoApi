namespace RapsoApi.Model
{
    public class RapsoDatabaseSettings : IRapsoDatabaseSettings
    {
        public string OperationsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IRapsoDatabaseSettings
    {
        string OperationsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}