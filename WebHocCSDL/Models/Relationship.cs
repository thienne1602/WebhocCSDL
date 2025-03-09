namespace WebHocCSDL.Models
{
    public class Relationship
    {
        public int Id { get; set; }
        public string? Entity1 { get; set; }
        public string? Entity2 { get; set; }
        public string? Type { get; set; }
        public int? DatabaseDesignId { get; set; }
        public DatabaseDesign? DatabaseDesign { get; set; }
    }
}
