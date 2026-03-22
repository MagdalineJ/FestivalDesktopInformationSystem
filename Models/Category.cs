namespace FestivalDesktopInformationSystem.Models
{
    // Represents a vendor product category entity.
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public Category()
        {
            CategoryName = string.Empty;
        }

        public Category(int categoryId, string categoryName)
        {
            CategoryId = categoryId;
            CategoryName = categoryName;
        }

        public override string ToString()
        {
            return $"{CategoryId} - {CategoryName}";
        }
    }
}