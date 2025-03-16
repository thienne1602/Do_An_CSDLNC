using System.Collections.Generic;

namespace WebCSDL.Models
{
    public class HomeViewModel
    {
        public List<Entity> Entities { get; set; } = new List<Entity>();
        public List<Relationship> Relationships { get; set; } = new List<Relationship>();
    }
}