namespace SampleApi.Models
{
    public class Document
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

    }
}
