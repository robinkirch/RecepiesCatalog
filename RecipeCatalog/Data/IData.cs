namespace RecipeCatalog.Data
{
    public interface IData
    {
        public int Id { get; set; }
        public byte[]? Image { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string[]? Aliases { get; set; }
        public int? GroupId { get; set; }
    }
}
