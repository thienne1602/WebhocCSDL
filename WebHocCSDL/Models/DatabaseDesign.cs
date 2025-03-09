using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace WebHocCSDL.Models
{
    public class DatabaseDesign
    {
        public int Id { get; set; }
        public string RequirementDescription { get; set; }
        public string ConceptualDesign { get; set; }
        public string ERD { get; set; }
        public string LogicalDesign { get; set; }
        public string PhysicalDesign { get; set; }

        [NotMapped]
        public List<Entity> Entities { get; set; }

        public string EntitiesJson
        {
            get => JsonSerializer.Serialize(Entities);
            set => Entities = JsonSerializer.Deserialize<List<Entity>>(value);
        }

        [NotMapped]
        public List<Relationship> Relationships { get; set; }

        public string RelationshipsJson
        {
            get => JsonSerializer.Serialize(Relationships);
            set => Relationships = JsonSerializer.Deserialize<List<Relationship>>(value);
        }
    }
}