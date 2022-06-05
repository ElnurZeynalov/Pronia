using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pronia.DAL;
using Pronia.Models;
using Pronia.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pronia.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext _context { get;}
        public HomeController(AppDbContext context)
        {
            _context=context;
        }
        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                Slider = await _context.Sliders.ToListAsync(),
                Categories = await _context.Categories.ToListAsync(),
                Products = await _context.Products.Include(p => p.Category).Take(8).Include(p => p.ProductImages).ToListAsync(),
        };
            return View(homeVM);
        }
        [HttpGet]
        public IActionResult SelectCategory(int id)
        {
            if(id == 0)
            {
                return PartialView("_HomeProductPartial",_context.Products
                .Where(x=>x.IsDeleted==false)
                .Take(8)
                .Include(p=>p.ProductImages)
                .Include(p=>p.Category));
            }
            else
            {
                return PartialView("_HomeProductPartial", _context.Products
                .Where(x => x.IsDeleted == false && x.CategoryId == id)
                .Take(8)
                .Include(p => p.ProductImages)
                .Include(p => p.Category));
            }
            
        }
        public IActionResult Cart()
        {
            List<BasketVM> basketItems = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["Basket"]);
            List<CartVM> cartItems = new List<CartVM>();
            foreach (var item in basketItems)
            {
                Product product = _context.Products.Include(x=>x.ProductImages).SingleOrDefault(x => x.Id == item.ProductId);
                if (product == null) continue;
                cartItems.Add(new CartVM {
                    ProductId = item.ProductId,
                    Name = product.Name,
                    Count = item.Count,
                    Image =product.ProductImages.FirstOrDefault(x=>x.IsMain).Image,
                    Price = product.Price,
                    IsActive = product.StockCount > item.Count? true:false,
                });
            }
            return View(cartItems);
        }
        public IActionResult Basket(int id)
        {
            Product product = _context.Products.Find(id);
            if(product == null) return NotFound();
            List<BasketVM> basketItems = new List<BasketVM>();
            BasketVM item = new BasketVM { 
                ProductId = product.Id,
                Count = 1
            };
            
            if (Request.Cookies["Basket"] != null)
            {
                basketItems = JsonConvert.DeserializeObject<List<BasketVM>>(Request.Cookies["Basket"]);
            }
            if(basketItems.Find(i=>i.ProductId == product.Id) == null)
            {
                basketItems.Add(item);
            }
            else
            {
                basketItems.Find(i => i.ProductId == product.Id).Count++;
            }
            Response.Cookies.Append("Basket",JsonConvert.SerializeObject(basketItems));
            return RedirectToAction(nameof(Index));
        }
    }
}
