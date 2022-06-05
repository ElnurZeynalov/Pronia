using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace Pronia.Services
{
    public class LayoutServices
    {
        private AppDbContext _context { get;}
        private IHttpContextAccessor _acessor { get;}
        public LayoutServices(AppDbContext context , IHttpContextAccessor accessor)
        {
            _context=context;
            _acessor=accessor;
        }
        public List<CartVM> GetCartItems()
        {
            List<BasketVM> basketItems = JsonConvert.DeserializeObject<List<BasketVM>>(_acessor.HttpContext.Request.Cookies["Basket"]);
            List<CartVM> cartItems = new List<CartVM>();
            foreach (var item in basketItems)
            {
                Product product = _context.Products.Include(x => x.ProductImages).SingleOrDefault(x => x.Id == item.ProductId);
                if (product == null) continue;
                cartItems.Add(new CartVM
                {
                    ProductId = item.ProductId,
                    Name = product.Name,
                    Count = item.Count,
                    Image = product.ProductImages.FirstOrDefault(x => x.IsMain).Image,
                    Price = product.Price,
                    IsActive = product.StockCount > item.Count ? true : false,
                });
            }
            return cartItems;
        }
    }
}
