﻿using AllUp.DAL;
using AllUp.Helpers;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllUp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public CategoriesController(AppDbContext db, IWebHostEnvironment env)
        {
            _db= db;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            
            List<Category>categories=await _db.Categories.OrderByDescending(x=>x.IsMain).Include(x=>x.Children).Include(x => x.Parent).ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.MainCategories = await _db.Categories.Where(x=>x.IsMain).ToListAsync();
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category,int? mainCatId)
        {
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            if (category.IsMain) 
            {
                bool isExist = await _db.Categories.AnyAsync(x=>x.Name==category.Name);
                if(isExist) 
                {
                    ModelState.AddModelError("Name", "This category is already exist");
                    return View();
                }
                #region SAVE IMAGE
                if (category.Photo == null)
                {
                    ModelState.AddModelError("Photo", "Image cannot be null");
                    return View();
                }
                if (!category.Photo.IsImage())
                {
                    ModelState.AddModelError("Photo", "Please select image type");
                    return View();
                }
                if (category.Photo.IsOlder1Mb())
                {
                    ModelState.AddModelError("Photo", "MAX 1 Mb");
                    return View();
                }
                string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                category.Image = await category.Photo.SaveFileAsync(folder);
                #endregion
            }
            else
            {
                category.ParentId=mainCatId;
            }
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();

            List<Subscribe> subscribes = await _db.Subscribes.ToListAsync();
            string message = "salam";
            string title = "test";
            foreach (Subscribe sub in subscribes)
            {
                await Helper.SendMailAsync(title, message,sub.Email);
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Activity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Category? dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id); 
            if (dbCategory == null)
                return BadRequest();
            if (dbCategory.IsDeactive == true)
            {
                dbCategory.IsDeactive = false;
            }
            else
            {
                dbCategory.IsDeactive = true;
            }
            return RedirectToAction("Index");
        }
    

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null)
                return NotFound();
            Category? dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if(dbCategory == null)
            {
                return BadRequest();
            }
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            return View(dbCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id,Category category,int? mainCatId)
        {
            if (id == null)
                return NotFound();
            Category? dbCategory = await _db.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (dbCategory == null)
            {
                return BadRequest();
            }
            ViewBag.MainCategories = await _db.Categories.Where(x => x.IsMain).ToListAsync();
            if(dbCategory.IsMain)
            {
                #region Exist
                bool isExist = await _db.Categories.AnyAsync(x => x.Name == category.Name && x.Id != id);
                if (isExist)
                {
                    ModelState.AddModelError("Name", "This category is already exist");
                    return View();
                }
                #endregion

                #region SAVE IMAGE
                if (category.Photo != null)
                {
                    if (!category.Photo.IsImage())
                    {
                        ModelState.AddModelError("Photo", "Please select image type");
                        return View();
                    }
                    if (category.Photo.IsOlder1Mb())
                    {
                        ModelState.AddModelError("Photo", "MAX 1 Mb");
                        return View();
                    }
                    string folder = Path.Combine(_env.WebRootPath, "assets", "images");
                    dbCategory.Image = await category.Photo.SaveFileAsync(folder);
                }
                
                #endregion
            }
            else
            {
                dbCategory.ParentId = mainCatId;
            }
            dbCategory.Name = category.Name;
            await _db.SaveChangesAsync();
            return RedirectToAction("Index"); 
        }
    }
}
