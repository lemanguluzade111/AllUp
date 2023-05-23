using AllUp.DAL;
using AllUp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace AllUp.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        public HomeController(AppDbContext db)
        {
            _db = db;
        }



        public async Task<IActionResult> Index()
        {
            return View(await _db.Categories.Where(x=>x.IsMain).ToListAsync());
        }

        
        
        public IActionResult Error()
        {
            return View();
        }

        public async Task<IActionResult> Subscribe(string email)
        {
            if(email==null)
            {
                return Content("Email bos ola bilmez");
            }
            
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);
            if (!match.Success)
            {
                return Content("Email deyil");
            }
            else
            {
                Subscribe subscribe = new Subscribe
                {
                    Email = email,

                };
                bool isExist= await _db.Subscribes.AnyAsync(x=>x.Email==email);
                if (isExist)
                {
                    return Content("Email artiq movcuddur");
                }
                await _db.Subscribes.AddAsync(subscribe);
                await _db.SaveChangesAsync();
            }
            return Content("Ok");
        }
    }
}