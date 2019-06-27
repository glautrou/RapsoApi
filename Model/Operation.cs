using CsvHelper.Configuration.Attributes;
using MongoDB.Bson;

namespace RapsoApi.Model
{
    public class Operation
    {
        [Ignore]
        public ObjectId Id { get; set; }
        public string id_mutation { get; set; }
        public string date_mutation { get; set; }
        public string numero_disposition { get; set; }
        public string valeur_fonciere { get; set; }
        public string adresse_numero { get; set; }
        public string adresse_suffixe { get; set; }
        public string adresse_code_voie { get; set; }
        public string adresse_nom_voie { get; set; }
        public string code_postal { get; set; }
        public string code_commune { get; set; }
        public string nom_commune { get; set; }
        public string ancien_code_commune { get; set; }
        public string ancien_nom_commune { get; set; }
        public string code_departement { get; set; }
        public string id_parcelle { get; set; }
        public string ancien_id_parcelle { get; set; }
        public string numero_volume { get; set; }
        public string lot1_numero { get; set; }
        public string lot1_surface_carrez { get; set; }
        public string lot2_numero { get; set; }
        public string lot2_surface_carrez { get; set; }
        public string lot3_numero { get; set; }
        public string lot3_surface_carrez { get; set; }
        public string lot4_numero { get; set; }
        public string lot4_surface_carrez { get; set; }
        public string lot5_numero { get; set; }
        public string lot5_surface_carrez { get; set; }
        public string nombre_lots { get; set; }
        public string code_type_local { get; set; }
        public string type_local { get; set; }
        public string surface_reelle_bati { get; set; }
        public string nombre_pieces_principales { get; set; }
        public string code_nature_culture { get; set; }
        public string nature_culture { get; set; }
        public string code_nature_culture_speciale { get; set; }
        public string nature_culture_speciale { get; set; }
        public string surface_terrain { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
    }
}