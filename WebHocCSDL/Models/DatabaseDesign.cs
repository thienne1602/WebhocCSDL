namespace WebHocCSDL.Models
{
    public class DatabaseDesign
    {
        public int Id { get; set; }
        public string? RequirementDescription { get; set; }
        public string? ERD { get; set; }
        public string? LogicalDesign { get; set; }
        public string? PhysicalDesign { get; set; }
        public string? ConceptualDesign { get; set; }
        public List<Entity>? Entities { get; set; } = new List<Entity>();
        public List<Relationship>? Relationships { get; set; } = new List<Relationship>();
        // Remove this if present: public string? RelationshipsJson { get; set; }
    }


}