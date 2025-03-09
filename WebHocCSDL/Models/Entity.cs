namespace WebHocCSDL.Models
{
    public class Entity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<string>? Attributes { get; set; } = new List<string>();
        public int? DatabaseDesignId { get; set; }
        public DatabaseDesign? DatabaseDesign { get; set; }
    }
}


