using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TrainingsDashboard.Models;

namespace TrainingsDashboard.Controllers
{
    [Authorize]
    [NoCache]
    public class EntrenadoresController : Controller
    {
        private readonly AppDbContext _context;
        public EntrenadoresController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
