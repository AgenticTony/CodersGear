using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodersGear.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string sortColumn = "CategoryId", string sortOrder = "asc", string search = "", int page = 1, int pageSize = 10)
        {
            // Get all categories
            IEnumerable<Category> objCategoryList = _unitOfWork.Category.GetAll();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                objCategoryList = objCategoryList.Where(c =>
                    (c.Name != null && c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    c.DisplayOrder.ToString().Contains(search));
            }

            // Store total count before pagination
            int totalCount = objCategoryList.Count();

            // Apply sorting
            objCategoryList = sortColumn switch
            {
                "Name" => sortOrder == "asc" ? objCategoryList.OrderBy(c => c.Name) : objCategoryList.OrderByDescending(c => c.Name),
                "DisplayOrder" => sortOrder == "asc" ? objCategoryList.OrderBy(c => c.DisplayOrder) : objCategoryList.OrderByDescending(c => c.DisplayOrder),
                _ => sortOrder == "asc" ? objCategoryList.OrderBy(c => c.CategoryId) : objCategoryList.OrderByDescending(c => c.CategoryId)
            };

            // Apply pagination
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            page = Math.Max(1, Math.Min(page, totalPages)); // Ensure page is within valid range

            var paginatedList = objCategoryList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display Order cannot exactly match the Name.");
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? categoryFromDb = _unitOfWork.Category.Get(u=>u.CategoryId==id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Category.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }

            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? categoryFromDb = _unitOfWork.Category.Get(u => u.CategoryId == id);

            if (categoryFromDb == null)
            {
                return NotFound();
            }

            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Category? obj = _unitOfWork.Category.Get(u => u.CategoryId == id);

            if (obj == null)
            {
                return NotFound();
            }

            _unitOfWork.Category.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
