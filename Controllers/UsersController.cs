using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    [Authorize]
    [NoCache]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var users = _context.Users.ToList();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Users user)
        {
            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<Users>();
                user.PasswordHash = hasher.HashPassword(user, user.PasswordHash);

                user.AdmissionDate = DateTime.Now;
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }
    }
}
