using System.ComponentModel.DataAnnotations.Schema;

namespace FilmBookmarkService.Core
{
    public class Film
    {
        // http://www.asp.net/mvc/tutorials/getting-started-with-ef-using-mvc/creating-an-entity-framework-data-model-for-an-asp-net-mvc-application

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public string Link { get; set; }
        
        public string Stream { get; set; }
        
        public string Season { get; set; }
        
        public string Episode { get; set; } 
    }
}