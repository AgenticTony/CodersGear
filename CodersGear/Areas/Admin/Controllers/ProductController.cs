using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using CodersGear.Models.ViewModels;
using CodersGear.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CodersGear.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment hostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostEnvironment = hostEnvironment;
        }

        /// <summary>
        /// GET: Display list of all products with search, sorting, and pagination
        /// </summary>
        public IActionResult Index(string sortColumn = "ProductId", string sortOrder = "asc", string search = "", int page = 1, int pageSize = 20)
        {
            // Get all products with Category
            IEnumerable<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category");

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                objProductList = objProductList.Where(p =>
                    (p.ProductName != null && p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (p.UPC != null && p.UPC.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Category != null && p.Category.Name != null && p.Category.Name.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            // Store total count before pagination
            int totalCount = objProductList.Count();

            // Apply sorting
            objProductList = sortColumn switch
            {
                "ProductName" => sortOrder == "asc" ? objProductList.OrderBy(p => p.ProductName) : objProductList.OrderByDescending(p => p.ProductName),
                "UPC" => sortOrder == "asc" ? objProductList.OrderBy(p => p.UPC) : objProductList.OrderByDescending(p => p.UPC),
                "Price" => sortOrder == "asc" ? objProductList.OrderBy(p => p.Price) : objProductList.OrderByDescending(p => p.Price),
                "Category" => sortOrder == "asc" ? objProductList.OrderBy(p => p.Category!.Name) : objProductList.OrderByDescending(p => p.Category!.Name),
                _ => sortOrder == "asc" ? objProductList.OrderBy(p => p.ProductId) : objProductList.OrderByDescending(p => p.ProductId)
            };

            // Apply pagination
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages)); // Ensure page is within valid range

            var paginatedList = objProductList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Pass data to view
            ViewData["CurrentSortColumn"] = sortColumn;
            ViewData["CurrentSortOrder"] = sortOrder;
            ViewData["CurrentSearch"] = search;
            ViewData["CurrentPage"] = page;
            ViewData["PageSize"] = pageSize;
            ViewData["TotalPages"] = totalPages == 0 ? 1 : totalPages;
            ViewData["TotalCount"] = totalCount;

            return View(paginatedList);
        }

        /// <summary>
        /// GET: Display form to create a new product
        /// </summary>
        public IActionResult Upsert(int? id)
        {
            // Create the ViewModel with an empty Product and populated categories
            ProductViewModel viewModel = new ProductViewModel
            {
                Product = new Product(),
                CategoryDictionary = _unitOfWork.Category.GetAll()
                    .Where(u => u.Name != null)
                    .ToDictionary(u => u.CategoryId, u => u.Name!)
            };
            if (id == null || id == 0)
            {
                // Create
                return View(viewModel);
            }
            else
            {
                // Update
                viewModel.Product = _unitOfWork.Product.Get(u => u.ProductId == id);
                if (viewModel.Product == null)
                {
                    return NotFound();
                }
                return View(viewModel);
            }
        }

        /// <summary>
        /// POST: Handle form submission to create or update a product
        /// </summary>
        [HttpPost]
        public IActionResult Upsert(ProductViewModel viewModel, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;

                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\product");
                    var extension = Path.GetExtension(file.FileName);

                    // Delete old image if updating and old image exists
                    if (viewModel.Product!.ProductId != 0 && !string.IsNullOrEmpty(viewModel.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, viewModel.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Save new image
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    viewModel.Product.ImageUrl = @"\images\product\" + fileName + extension;
                }

                if (viewModel.Product!.ProductId == 0)
                {
                    // Create new product
                    _unitOfWork.Product.Add(viewModel.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    // Update existing product
                    _unitOfWork.Product.Update(viewModel.Product);
                    TempData["success"] = "Product updated successfully";
                }
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }

            // If validation fails, repopulate categories and return to the form
            viewModel.CategoryDictionary = _unitOfWork.Category.GetAll()
                .Where(u => u.Name != null)
                .ToDictionary(u => u.CategoryId, u => u.Name!);

            return View(viewModel);
        }

        /// <summary>
        /// GET: Display form to edit an existing product
        /// </summary>
       
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? productFromDb = _unitOfWork.Product.Get(u => u.ProductId == id);

            if (productFromDb == null)
            {
                return NotFound();
            }

            return View(productFromDb);
        }

        /// <summary>
        /// POST: Handle the actual deletion of a product
        /// </summary>
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? obj = _unitOfWork.Product.Get(u => u.ProductId == id);

            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.Product.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
