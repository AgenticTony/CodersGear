using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CodersGear.DataAccess.Repository.IRepository;
using CodersGear.Models;
using IPrintifyService = CodersGear.Utility.IPrintifyService;

namespace CodersGear.Services
{
    public interface IPrintifyProductSyncService
    {
        Task SyncProductsAsync();
        Task SyncSingleProductAsync(string printifyProductId);
    }

    public class PrintifyProductSyncService : IPrintifyProductSyncService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPrintifyService _printifyService;
        private readonly Utility.PrintifySettings _settings;
        private readonly ILogger<PrintifyProductSyncService> _logger;

        // Category mapping: CategoryId → Keywords/Patterns
        private static readonly Dictionary<int, string[]> CategoryKeywords = new()
        {
            { 1, new[] { "t-shirt", "tee", "tshirt", "jersey", "tank", "long sleeve", "short sleeve" } },  // T-Shirts
            { 2, new[] { "hoodie", "sweatshirt", "pullover", "crewneck", "zip", "fleece" } },              // Hoodies
            { 3, new[] { "mug", "cup", "coffee", "steamer", "travel", "ceramic" } },                     // Mugs
            { 4, new[] { "hat", "cap", "beanie", "bag", "tote", "sticker", "phone", "case", "accessory" } } // Accessories
        };

        public PrintifyProductSyncService(
            IUnitOfWork unitOfWork,
            IPrintifyService printifyService,
            IOptions<Utility.PrintifySettings> settings,
            ILogger<PrintifyProductSyncService> logger)
        {
            _unitOfWork = unitOfWork;
            _printifyService = printifyService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SyncProductsAsync()
        {
            if (string.IsNullOrEmpty(_settings.ShopId))
            {
                _logger.LogWarning("Printify ShopId is not configured. Skipping product sync.");
                return;
            }

            try
            {
                _logger.LogInformation("Starting Printify product sync...");

                var printifyProducts = await _printifyService.GetProductsAsync(_settings.ShopId);

                foreach (var printifyProduct in printifyProducts.Where(p => p.Visible))
                {
                    await SyncSingleProductAsync(printifyProduct.Id);
                }

                _logger.LogInformation($"Printify product sync completed. Processed {printifyProducts.Count} products.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Printify product sync: {ex.Message}");
            }
        }

        public async Task SyncSingleProductAsync(string printifyProductId)
        {
            if (string.IsNullOrEmpty(_settings.ShopId))
            {
                _logger.LogWarning("Printify ShopId is not configured. Skipping product sync.");
                return;
            }

            try
            {
                var printifyProduct = await _printifyService.GetProductAsync(_settings.ShopId, printifyProductId);

                if (printifyProduct == null)
                {
                    _logger.LogWarning($"Printify product {printifyProductId} not found.");
                    return;
                }

                // Determine the appropriate category
                int categoryId = MapProductToCategory(printifyProduct);

                // Check if product already exists in local database
                var existingProduct = _unitOfWork.Product.Get(p => p.PrintifyProductId == printifyProductId);

                if (existingProduct != null)
                {
                    // Update existing product
                    existingProduct.ProductName = printifyProduct.Title;
                    existingProduct.Description = printifyProduct.Description;
                    existingProduct.ImageUrl = printifyProduct.Images.FirstOrDefault()?.Src;
                    existingProduct.PrintifyVariantData = JsonSerializer.Serialize(printifyProduct.Variants);
                    existingProduct.LastSyncedAt = DateTime.UtcNow;

                    // Update category (in case it changed)
                    existingProduct.CategoryId = categoryId;

                    // Update prices from the first enabled variant
                    var firstVariant = printifyProduct.Variants.FirstOrDefault(v => v.IsEnabled);
                    if (firstVariant != null)
                    {
                        existingProduct.Price = firstVariant.Price / 100m; // Convert from cents to dollars
                        existingProduct.Price50 = firstVariant.Price / 100m;
                        existingProduct.Price100 = firstVariant.Price / 100m;
                        existingProduct.ListPrice = firstVariant.Price / 100m * 1.2m; // Add 20% margin
                    }

                    _unitOfWork.Product.Update(existingProduct);
                    _logger.LogInformation($"Updated existing product: {existingProduct.ProductName} (ID: {existingProduct.ProductId}, CategoryId: {categoryId})");
                }
                else
                {
                    // Create new product
                    var firstVariant = printifyProduct.Variants.FirstOrDefault(v => v.IsEnabled);

                    var newProduct = new Product
                    {
                        ProductName = printifyProduct.Title,
                        Description = printifyProduct.Description,
                        ImageUrl = printifyProduct.Images.FirstOrDefault()?.Src,
                        IsPrintifyProduct = true,
                        PrintifyProductId = printifyProduct.Id,
                        PrintifyShopId = _settings.ShopId,
                        PrintifyVariantData = JsonSerializer.Serialize(printifyProduct.Variants),
                        LastSyncedAt = DateTime.UtcNow,
                        CategoryId = categoryId,
                        UPC = $"PRINTIFY-{printifyProduct.Id}"
                    };

                    if (firstVariant != null)
                    {
                        newProduct.Price = firstVariant.Price / 100m;
                        newProduct.Price50 = firstVariant.Price / 100m;
                        newProduct.Price100 = firstVariant.Price / 100m;
                        newProduct.ListPrice = firstVariant.Price / 100m * 1.2m;
                    }

                    _unitOfWork.Product.Add(newProduct);
                    _logger.LogInformation($"Created new product: {newProduct.ProductName} (CategoryId: {categoryId})");
                }

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error syncing Printify product {printifyProductId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Intelligently maps a Printify product to the appropriate category based on title, description, and blueprint
        /// </summary>
        private int MapProductToCategory(Utility.PrintifyProduct product)
        {
            // Combine title and description for matching
            string searchText = $"{product.Title} {product.Description}".ToLower();

            // Try to match against category keywords
            foreach (var categoryPair in CategoryKeywords)
            {
                int categoryId = categoryPair.Key;
                string[] keywords = categoryPair.Value;

                foreach (string keyword in keywords)
                {
                    if (searchText.Contains(keyword))
                    {
                        _logger.LogDebug($"Product '{product.Title}' matched category {categoryId} using keyword '{keyword}'");
                        return categoryId;
                    }
                }
            }

            // If no match found, check blueprint ID for known patterns
            // (You can add specific blueprint IDs here if needed)
            // For example, some Printify providers use specific blueprint IDs for certain product types

            // Default: return first category
            int defaultCategoryId = _unitOfWork.Category.GetAll().OrderBy(c => c.CategoryId).FirstOrDefault()?.CategoryId ?? 1;
            _logger.LogWarning($"No category match found for product '{product.Title}'. Defaulting to category {defaultCategoryId}");
            return defaultCategoryId;
        }
    }
}
