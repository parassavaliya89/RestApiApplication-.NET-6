using System.ComponentModel.DataAnnotations;

namespace RestApi.Models
{
    public class Book
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        [Range(0,100)]
        public int Price { get; set; }
    }
}
