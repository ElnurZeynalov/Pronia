using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pronia.DAL;
using Pronia.Models;
using Pronia.Utilies.File;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pronia.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class SliderController : Controller
    {
        private AppDbContext _context { get; }
        private IWebHostEnvironment _env { get; }
        public SliderController(AppDbContext context,IWebHostEnvironment env)
        {
            _context=context;
            _env=env;
        }
        public async Task<IActionResult> Index()
        {
            List<Slider> slider = await _context.Sliders.ToListAsync();
            return View(slider);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Slider slider)
        {
            if(ModelState.IsValid) return View();
            bool isExist = _context.Sliders.Any(x => x.Title.ToLower().Trim() == slider.Title.ToLower().Trim());
            if (isExist) return View();
            if(!slider.imageFile.CheckFileFormat("image/"))
            {
                ModelState.AddModelError("fileImage", "Yalniz image formatinda file yukleyin");
                return View();
            }
            if (!slider.imageFile.CheckFileSize(5))
            {
                ModelState.AddModelError("fileImage", "Image olcusu 5 mb cox olamamalidir");
                return View();
            }
            slider.Image = await slider.imageFile.SaveFileAsync(Path.Combine(_env.WebRootPath, "assets", "images", "sliderImg"));
            await _context.Sliders.AddAsync(slider);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(int id)
        {
            Slider slider = _context.Sliders.Find(id);
            if (slider == null) return NotFound();
            return View(slider);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Slider slider)
        {
            if (ModelState.IsValid) return View();
            Slider sliderItem = _context.Sliders.Find(slider.Id);
            if (sliderItem == null) return NotFound();
            if (!slider.imageFile.CheckFileFormat("image/"))
            {
                ModelState.AddModelError("fileImage", "Yalniz image formatinda file yukleyin");
                return View();
            }
            if (!slider.imageFile.CheckFileSize(5))
            {
                ModelState.AddModelError("fileImage", "Image olcusu 5 mb cox olamamalidir");
                return View();
            }
            sliderItem.Title = slider.Title;
            sliderItem.Description = slider.Description;
            slider.Image = await slider.imageFile.SaveFileAsync(Path.Combine(_env.WebRootPath, "assets", "images", "sliderImg"));
            sliderItem.Image = slider.Image;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            Slider slider = _context.Sliders.Find(id);
            if(slider == null) return NotFound();
            FileExtension.DeleteFile(Path.Combine(_env.WebRootPath, "assets", "images", "sliderImg" + slider.Image));
            _context.Sliders.Remove(slider);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
