using Pronia.Models;
using System.Collections.Generic;

namespace Pronia.ViewModel
{
    public class HomeVM
    {
        public List<Slider> Slider { get; set; }
        public List<Category> Categories { get; set; }
        public List<Product> Products { get; set; }
    }
}
