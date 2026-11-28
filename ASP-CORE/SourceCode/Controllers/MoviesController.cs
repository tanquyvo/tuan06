
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using _31241023037_VoTanQuy.Data;
using _31241023037_VoTanQuy.Models;
using _31241023037_VoTanQuy.Extension;
using System.Text;

namespace _31241023037_VoTanQuy.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        // 7.1(a): Constructor
        public MoviesController(
            ApplicationDbContext context,
            IWebHostEnvironment env,
            IConfiguration config)
        {
            _context = context;
            _env = env;
            _config = config;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            // Display the five supplied poster titles in the same order as the user's images.
            var approvedOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["Love Destiny: The Movie"] = 1,
                ["Addicted"] = 2,
                ["La La Land"] = 3,
                ["The Dark Knight"] = 4,
                ["Interstellar"] = 5
            };

            var movies = await _context.Movies
                .Include(m => m.GenreObj)
                .ToListAsync();

            var orderedMovies = movies
                .OrderBy(movie => approvedOrder.TryGetValue(movie.Title, out var order) ? order : int.MaxValue)
                .ThenBy(movie => movie.Title)
                .ToList();

            return View(orderedMovies);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.GenreObj)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // GET: Movies/Create
        // 7.1(b)
        public IActionResult Create()
        {
            PopulateGenres();
            return View();
        }

        // POST: Movies/Create
        // 7.1(c)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                // Lưu ảnh upload, nếu không có ảnh thì dùng ảnh mặc định
                movie.PicturePath = await SaveImageAsync(movie.PictureUpload)
                    ?? "/images/no-image.jpg";

                _context.Add(movie);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            PopulateGenres(movie.GenreID);
            return View(movie);
        }

        // GET: Movies/Edit/5
        // 7.1(b)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            PopulateGenres(movie.GenreID);
            return View(movie);
        }

        // POST: Movies/Edit/5
        // 7.1(d)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie)
        {
            if (id != movie.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Lưu ảnh mới nếu người dùng upload
                    var newPath = await SaveImageAsync(movie.PictureUpload);

                    if (newPath != null)
                    {
                        movie.PicturePath = newPath;
                    }
                    else if (string.IsNullOrEmpty(movie.PicturePath))
                    {
                        movie.PicturePath = "/images/no-image.jpg";
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateGenres(movie.GenreID);
            return View(movie);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .Include(m => m.GenreObj)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);

            if (movie != null)
            {
                _context.Movies.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Kiểm tra Movie có tồn tại hay không
        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.ID == id);
        }

        // 7.1(a): Lưu ảnh vào wwwroot/images
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Chỉ cho phép một số định dạng ảnh
            if (!new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" }.Contains(ext))
            {
                return null;
            }

            var folder = Path.Combine(_env.WebRootPath, "images");

            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid().ToString("N") + ext;

            using var stream = System.IO.File.Create(
                Path.Combine(folder, fileName)
            );

            await file.CopyToAsync(stream);

            return "/images/" + fileName;
        }

        // 7.1(b): Tạo dropdown Genre hiển thị Name
        private void PopulateGenres(int? selectedGenreId = null)
        {
            ViewData["GenreID"] = new SelectList(
                _context.Genres,
                "ID",
                "Name",
                selectedGenreId
            );
        }
                public ActionResult AddToCart(int id)
        {
            // Kiểm tra Id movie tồn tại hay không
            var movie = _context.Movies.Where(x => x.ID == id).FirstOrDefault();

            if (movie == null)
            {
                return RedirectToAction("Index");
            }

            var hoaDon = HttpContext.Session.Get<HoaDon>("HoaDon");

            // Kiểm tra sản phẩm đã có trong giỏ chưa
            var chiTietHoaDon = hoaDon.ChiTietHoaDons
                .Where(x => x.MaMovie == id)
                .FirstOrDefault();

            if (chiTietHoaDon == null)
            {
                chiTietHoaDon = new ChiTietHoaDon();
                chiTietHoaDon.MaMovie = id;
                chiTietHoaDon.MovieObj = movie;
                chiTietHoaDon.SoLuong = 1;

                hoaDon.ChiTietHoaDons.Add(chiTietHoaDon);
            }
            else
            {
                chiTietHoaDon.SoLuong++;
            }

            hoaDon.TongTien = hoaDon.ChiTietHoaDons
                .Sum(x => x.SoLuong * x.MovieObj.Price);

            HttpContext.Session.Set<HoaDon>("HoaDon", hoaDon);

            return View(hoaDon);
        }
            public ActionResult RemoveFromCart(int maMovies)
        {
            var hoaDon = HttpContext.Session.Get<HoaDon>("HoaDon");

            var chiTietHoaDon = hoaDon.ChiTietHoaDons
                .Where(x => x.MaMovie == maMovies)
                .FirstOrDefault();

            if (chiTietHoaDon != null)
            {
                hoaDon.ChiTietHoaDons.Remove(chiTietHoaDon);
            }

            hoaDon.TongTien = hoaDon.ChiTietHoaDons
                .Sum(x => x.SoLuong * x.MovieObj.Price);

            HttpContext.Session.Set<HoaDon>("HoaDon", hoaDon);

            return View("AddToCart", hoaDon);
        }
            public ActionResult Checkout()
        {
            return View(new ShippingDetail());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Checkout(ShippingDetail detail)
        {
            var hoaDon = HttpContext.Session.Get<HoaDon>("HoaDon");

            if (hoaDon == null || hoaDon.ChiTietHoaDons.Count == 0)
            {
                ModelState.AddModelError("", "Sorry, your cart is empty!");
            }

            if (ModelState.IsValid)
            {
                // Lưu đơn hàng vào CSDL trên Somee
                var order = new HoaDon
                {
                    NgayLap = DateTime.Now,
                    TongTien = hoaDon.TongTien
                };

                foreach (var ct in hoaDon.ChiTietHoaDons)
                {
                    order.ChiTietHoaDons.Add(new ChiTietHoaDon
                    {
                        MaMovie = ct.MaMovie,
                        SoLuong = ct.SoLuong
                    });
                }

                _context.HoaDons.Add(order);
                _context.ShippingDetails.Add(detail);

                await _context.SaveChangesAsync();

                // Soạn nội dung email
                Func<string, string> enc = System.Net.WebUtility.HtmlEncode;

                StringBuilder body = new StringBuilder()
                    .AppendLine("<p>A new order has been submitted</p><hr />")
                    .AppendLine("<p>Items:</p><ul>");

                foreach (var ct in hoaDon.ChiTietHoaDons)
                {
                    var subtotal = ct.MovieObj.Price * ct.SoLuong;

                    body.AppendLine(
                        $"<li>{ct.SoLuong} x {enc(ct.MovieObj.Title)} (subtotal: {subtotal:N0})</li>");
                }

                body.AppendLine("</ul>")
                    .AppendLine($"<p>Total order value: {hoaDon.TongTien:N0}</p><hr />")
                    .AppendLine(
                        $"<p>Ship to:<br/>{enc(detail.Name)}<br/>{enc(detail.Address)}<br/>{enc(detail.Mobile)}</p>");

                var gmail = _config["Email:Address"];
                var appPassword = _config["Email:AppPassword"];

                await EmailServiceNew.SendMailGoogleSmtp(
                    gmail,
                    detail.Email,
                    "New order submitted!",
                    body.ToString(),
                    gmail,
                    appPassword);

                HttpContext.Session.Remove("HoaDon");

                return View("CheckoutCompleted");
            }

            return View(detail);
        }
    }
}
