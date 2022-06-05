using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilies.File;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Pronia.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class ProductController : Controller
    {
        public AppDbContext _context { get; }
        private IWebHostEnvironment _env { get; }
        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            List<Product> products = await _context.Products.Include(p => p.Category).Include(p => p.ProductImages).ToListAsync();
            return View(products);
        }
        public IActionResult Create()
        {
            ViewBag.Catagories = _context.Categories.ToList();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View();
            ViewBag.Categories = _context.Categories.ToList();
            if (_context.Products.Any(p => p.Name == product.Name))
            {
                ModelState.AddModelError("Name", "Bu adda Product sistemde movcuddur");
                return View();
            }
            if (product.MainPhoto != null)
            {
                if (!product.MainPhoto.CheckFileFormat("image/"))
                {
                    ModelState.AddModelError("ManinPhoto", "Yalniz image formatinda file yukleyin");
                    return View();
                }
                if (!product.MainPhoto.CheckFileSize(1))
                {
                    ModelState.AddModelError("MainPhoto", "File olcusu 1 mb cox olmamalidir");
                    return View();
                }
                product.ProductImages = new List<ProductImage>();
                product.ProductImages.Add(new ProductImage
                {
                    Image = await product.MainPhoto.SaveFileAsync(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                    IsMain = true,
                    Product = product,
                });
            }
            if (product.Photos != null)
            {

                if (product.Photos.PhotoIsOk() != "")
                {
                    ModelState.AddModelError("Photos", product.Photos.PhotoIsOk());
                    return View();
                }
                foreach (var photo in product.Photos)
                {
                    ProductImage productImage = new ProductImage()
                    {
                        Image = await photo.SaveFileAsync(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                        IsMain = false,
                        Product = product,
                    };
                    product.ProductImages.Add(productImage);
                }
            }
            product.IsDeleted = false;
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(int id)
        {
            Product product = _context.Products.Include(x => x.Category).Include(x => x.ProductImages).SingleOrDefault(x => x.Id == id);
            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            List<ProductImage> productImages = _context.ProductImages.Where(x => x.ProductId == id).ToList();
            Product changedProduct = _context.Products.SingleOrDefault(x => x.Id == id);
            if (!ModelState.IsValid) return View();
            if (product.MainPhoto != null)
            {
                if (changedProduct.MainPhoto == null)
                {
                    ModelState.AddModelError("MainPhoto", "Esas sekil duzgun qeyd olunmayib");
                    return RedirectToAction(nameof(Edit));
                }
                productImages.Remove(productImages.FirstOrDefault(x => x.ProductId == product.Id && x.IsMain == true));
                changedProduct.ProductImages.Add(new ProductImage
                {
                    Image = await product.MainPhoto.SaveFileAsync(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                    IsMain = true,
                    Product = product,
                });
            }
            else
            {
                if (!product.MainPhoto.CheckFileFormat("image/"))
                {
                    ModelState.AddModelError("ManinPhoto", "Yalniz image formatinda file yukleyin");
                    return RedirectToAction(nameof(Edit));
                }
                if (!product.MainPhoto.CheckFileSize(1))
                {
                    ModelState.AddModelError("MainPhoto", "File olcusu 1 mb cox olmamalidir");
                    return RedirectToAction(nameof(Edit));
                }
                product.ProductImages = new List<ProductImage>();
                product.ProductImages.Add(new ProductImage
                {
                    Image = await product.MainPhoto.SaveFileAsync(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                    IsMain = true,
                    Product = product,
                });
            }
            if (product.Photos != null)
            {
                if (changedProduct.Photos == null)
                {
                    ModelState.AddModelError("Photos", "Sekiller duzgun qeyd olunmayib");
                    return RedirectToAction(nameof(Edit));
                }
                productImages.Remove(productImages.FirstOrDefault(x => x.ProductId == product.Id && x.IsMain == false));
                foreach (var photo in productImages.FindAll(x => x.ProductId == product.Id))
                {
                    FileExtension.DeleteFile(Path.Combine(_env.WebRootPath, "assets", "images", "product", photo.Image));
                }
                foreach (var photo in changedProduct.Photos)
                {
                    ProductImage productImage = new ProductImage()
                    {
                        Image = await photo.SaveFileAsync(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                        IsMain = false,
                        Product = product,
                    };
                    product.ProductImages.Add(productImage);
                }
            }
            else
            {
                if (product.Photos.PhotoIsOk() != "")
                {
                    ModelState.AddModelError("Photos", product.Photos.PhotoIsOk());
                    return View();
                }
                List<int> deleteImgIds = new List<int>();
                foreach (var item in productImages)
                {
                    bool isEqual = false;
                    foreach (var imgId in product.ProductImageIds)
                    {
                        if (item.Id == imgId)
                        {
                            isEqual = true;
                            break;
                        }
                    }
                    if (isEqual == false)
                    {
                        deleteImgIds.Add(item.Id);
                    }
                }
                foreach (var imageId in deleteImgIds)
                {
                    FileExtension.DeleteFile(Path.Combine(_env.WebRootPath, "assets", "images", "product", productImages.Find(x => x.Id == imageId).Image));
                    productImages.Remove(productImages.Find(x => x.Id == imageId));
                }
                foreach (var photo in product.Photos)
                {
                    ProductImage productImage = new ProductImage()
                    {
                        Image = await photo.SaveFileAsync(Path.Combine(_env.WebRootPath, "assets", "images", "product")),
                        IsMain = false,
                        Product = product,
                    };
                    product.ProductImages.Add(productImage);
                }
            }
            changedProduct.Name = product.Name;
            changedProduct.Description = product.Description;
            changedProduct.Price = product.Price;
            changedProduct.Raiting = product.Raiting;
            changedProduct.StockCount = product.StockCount;
            changedProduct.ProductImages = product.ProductImages;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SoftDelete(int id)
        {
            Product product = _context.Products.Find(id);
            if (product == null) return NotFound();
            product.IsDeleted = true;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HardDelete(int id)
        {
            Product product = _context.Products.Find(id);
            if (product == null) return NotFound();
            List<ProductImage> productImages = _context.ProductImages.Where(x => x.ProductId == id).ToList();
            foreach (var image in productImages)
            {
                FileExtension.DeleteFile(Path.Combine(_env.WebRootPath, "assets/images/product", image.Image));
                product.ProductImages.Remove(image);
            }
            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Active(int id)
        {
            Product product = _context.Products.Find(id);
            if (product == null) return NotFound();
            product.IsDeleted = false;
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
