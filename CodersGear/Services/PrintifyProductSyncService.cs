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

        // Category mapping: CategoryId → Keywords/Patterns (ordered by priority - most specific first)
        // IMPORTANT: Order matters - more specific categories should be checked first
        private static readonly Dictionary<int, string[]> CategoryKeywords = new()
        {
            { 2, new[] { "hoodie", "sweatshirt", "pullover", "crewneck", "zip-up", "fleece" } },               // Hoodies
            { 3, new[] { "mug", "cup", "tumbler", "stein", "ceramic mug", "travel mug" } },                    // Mugs (removed generic "coffee")
            { 4, new[] { "hat", "cap", "beanie", "bag", "tote", "sticker", "phone case", "case for" } },       // Accessories
            { 1, new[] { "t-shirt", "tshirt", " t-shirt", " tee", "shirt", "jersey", "tank top", "long sleeve", "short sleeve", "sleeve" } }  // T-Shirts (check last)
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
            _logger.LogInformation("=== Printify Product Sync Started ===");
            _logger.LogInformation($"ShopId: '{_settings.ShopId}'");
            _logger.LogInformation($"ApiUrl: '{_settings.ApiUrl}'");

            if (string.IsNullOrEmpty(_settings.ShopId))
            {
                _logger.LogWarning("Printify ShopId is not configured. Skipping product sync.");
                return;
            }

            try
            {
                _logger.LogInformation("Fetching products from Printify API...");

                var printifyProducts = await _printifyService.GetProductsAsync(_settings.ShopId);

                _logger.LogInformation($"Received {printifyProducts.Count} products from Printify API");

                // Get all Printify product IDs from API
                var printifyProductIds = printifyProducts.Select(p => p.Id).ToHashSet();

                // Get all Printify products from local database
                var localPrintifyProducts = _unitOfWork.Product.GetAll(p => p.IsPrintifyProduct).ToList();
                var localPrintifyProductIds = localPrintifyProducts.Select(p => p.PrintifyProductId).ToHashSet();

                // 1. Hide products that no longer exist in Printify (deleted)
                var deletedProductIds = localPrintifyProductIds.Except(printifyProductIds);
                foreach (var deletedId in deletedProductIds)
                {
                    var productToHide = localPrintifyProducts.FirstOrDefault(p => p.PrintifyProductId == deletedId);
                    if (productToHide != null && productToHide.Visible)
                    {
                        productToHide.Visible = false;
                        _unitOfWork.Product.Update(productToHide);
                        _logger.LogInformation($"Hid deleted product: {productToHide.ProductName} (Printify ID: {deletedId}) - no longer in Printify");
                    }
                }

                // 2. Hide products that are invisible in Printify
                var invisiblePrintifyProducts = printifyProducts.Where(p => !p.Visible);
                foreach (var invisibleProduct in invisiblePrintifyProducts)
                {
                    var localProduct = localPrintifyProducts.FirstOrDefault(p => p.PrintifyProductId == invisibleProduct.Id);
                    if (localProduct != null && localProduct.Visible)
                    {
                        localProduct.Visible = false;
                        _unitOfWork.Product.Update(localProduct);
                        _logger.LogInformation($"Hid invisible product: {localProduct.ProductName} (Printify ID: {invisibleProduct.Id}) - marked invisible in Printify");
                    }
                }

                // 3. Sync visible products (update existing or add new)
                foreach (var printifyProduct in printifyProducts.Where(p => p.Visible))
                {
                    _logger.LogInformation($"Syncing product: {printifyProduct.Title} (ID: {printifyProduct.Id})");
                    await SyncSingleProductAsync(printifyProduct.Id);
                }

                _unitOfWork.Save();

                _logger.LogInformation($"=== Printify product sync completed. Processed {printifyProducts.Count} products, hid {deletedProductIds.Count()} deleted, hid {invisiblePrintifyProducts.Count()} invisible. ===");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during Printify product sync: {ex.GetType().Name} - {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
                throw;
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

                // Log what we received from Printify for debugging
                _logger.LogInformation($"Product '{printifyProduct.Title}' - Images: {printifyProduct.Images.Count}, Options: {printifyProduct.Options.Count}, Variants: {printifyProduct.Variants.Count}");
                if (printifyProduct.Images.Any())
                {
                    _logger.LogInformation($"  First image: {printifyProduct.Images.FirstOrDefault()?.Src}");
                }
                if (printifyProduct.Options.Any())
                {
                    foreach (var opt in printifyProduct.Options)
                    {
                        _logger.LogInformation($"  Option: {opt.Name} with {opt.Values.Count} values");
                    }
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

                    // Store ALL images as JSON array (including first one for thumbnail gallery)
                    var allImages = printifyProduct.Images.Select(i => i.Src).ToList();
                    existingProduct.AdditionalImages = allImages.Any() ? JsonSerializer.Serialize(allImages) : null;

                    // Store options (Size, Color, etc.)
                    existingProduct.PrintifyOptionsData = printifyProduct.Options.Any() ? JsonSerializer.Serialize(printifyProduct.Options) : null;

                    existingProduct.PrintifyVariantData = JsonSerializer.Serialize(printifyProduct.Variants);
                    existingProduct.LastSyncedAt = DateTime.UtcNow;

                    // Update category (in case it changed)
                    existingProduct.CategoryId = categoryId;

                    // Always keep Printify products visible (locked status just means publishing in progress)
                    existingProduct.Visible = true;

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
                    _logger.LogInformation($"Updated existing product: {existingProduct.ProductName} (ID: {existingProduct.ProductId}, CategoryId: {categoryId}, Visible: {existingProduct.Visible})");
                }
                else
                {
                    // Create new product
                    var firstVariant = printifyProduct.Variants.FirstOrDefault(v => v.IsEnabled);

                    // Store ALL images as JSON array (including first one for thumbnail gallery)
                    var allImages = printifyProduct.Images.Select(i => i.Src).ToList();

                    var newProduct = new Product
                    {
                        ProductName = printifyProduct.Title,
                        Description = printifyProduct.Description,
                        ImageUrl = printifyProduct.Images.FirstOrDefault()?.Src,
                        AdditionalImages = allImages.Any() ? JsonSerializer.Serialize(allImages) : null,
                        PrintifyOptionsData = printifyProduct.Options.Any() ? JsonSerializer.Serialize(printifyProduct.Options) : null,
                        IsPrintifyProduct = true,
                        PrintifyProductId = printifyProduct.Id,
                        PrintifyShopId = _settings.ShopId,
                        PrintifyVariantData = JsonSerializer.Serialize(printifyProduct.Variants),
                        LastSyncedAt = DateTime.UtcNow,
                        CategoryId = categoryId,
                        UPC = $"PRINTIFY-{printifyProduct.Id}",
                        Visible = true  // Always visible for Printify products
                    };

                    if (firstVariant != null)
                    {
                        newProduct.Price = firstVariant.Price / 100m;
                        newProduct.Price50 = firstVariant.Price / 100m;
                        newProduct.Price100 = firstVariant.Price / 100m;
                        newProduct.ListPrice = firstVariant.Price / 100m * 1.2m;
                    }

                    _unitOfWork.Product.Add(newProduct);
                    _logger.LogInformation($"Created new product: {newProduct.ProductName} (CategoryId: {categoryId}, Visible: {newProduct.Visible})");
                }

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error syncing Printify product {printifyProductId}: {ex.GetType().Name} - {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Intelligently maps a Printify product to the appropriate category based on title, description, and blueprint
        /// Uses priority-based matching where more specific product types override generic keywords
        /// </summary>
        private int MapProductToCategory(Utility.PrintifyProduct product)
        {
            // Combine title and description for matching
            string searchText = $"{product.Title} {product.Description}".ToLower();

            // Priority-based category checking (highest priority first)
            // More specific keywords are checked first to avoid false positives

            // Priority 1: Hoodies - check FIRST as "hoodie" is very specific
            if (ContainsAnyKeyword(searchText, new[] { "hoodie", "pullover hoodie", "zip hoodie", "fleece hoodie" }))
            {
                _logger.LogInformation($"Product '{product.Title}' categorized as Hoodie (2) - 'hoodie' keyword match");
                return 2;
            }

            // Priority 2: T-Shirts - use specific t-shirt patterns
            if (ContainsAnyKeyword(searchText, new[] { "t-shirt", "tshirt", "classic tee", "premium tee" }))
            {
                _logger.LogInformation($"Product '{product.Title}' categorized as T-Shirt (1) - specific t-shirt keyword match");
                return 1;
            }

            // Priority 3: Sweatshirts (without "hoodie" explicitly mentioned) - can include crewnecks
            if (ContainsAnyKeyword(searchText, new[] { "sweatshirt", "crewneck", "pullover" }) &&
                !ContainsAnyKeyword(searchText, new[] { "hoodie" }))
            {
                _logger.LogInformation($"Product '{product.Title}' categorized as Hoodie (2) - sweatshirt/crewneck match");
                return 2;
            }

            // Priority 4: Mugs - only if not already matched as apparel
            if (ContainsAnyKeyword(searchText, new[] { "mug", "coffee mug", "ceramic mug", "travel mug", "steamer" }))
            {
                _logger.LogInformation($"Product '{product.Title}' categorized as Mug (3) - specific mug keyword match");
                return 3;
            }

            // Priority 5: Accessories - check last as they have many generic keywords
            if (ContainsAnyKeyword(searchText, new[] { "hat", "cap", "beanie", "tote bag", "phone case", "sticker", "sticker sheet" }))
            {
                _logger.LogInformation($"Product '{product.Title}' categorized as Accessory (4) - accessory keyword match");
                return 4;
            }

            // Fallback: Use the original scoring system for edge cases
            var categoryScores = new Dictionary<int, int>();
            foreach (var categoryPair in CategoryKeywords)
            {
                int categoryId = categoryPair.Key;
                string[] keywords = categoryPair.Value;
                int matchCount = 0;

                foreach (string keyword in keywords)
                {
                    if (searchText.Contains(keyword))
                    {
                        matchCount++;
                        _logger.LogDebug($"Product '{product.Title}' matched category {categoryId} with keyword '{keyword}'");
                    }
                }

                if (matchCount > 0)
                {
                    categoryScores[categoryId] = matchCount;
                }
            }

            // Return category with highest score (most keyword matches)
            if (categoryScores.Any())
            {
                int bestCategoryId = categoryScores.OrderByDescending(x => x.Value).First().Key;
                _logger.LogInformation($"Product '{product.Title}' categorized as {bestCategoryId} with {categoryScores[bestCategoryId]} keyword matches (fallback)");
                return bestCategoryId;
            }

            // Default: return first category
            int defaultCategoryId = _unitOfWork.Category.GetAll().OrderBy(c => c.CategoryId).FirstOrDefault()?.CategoryId ?? 1;
            _logger.LogWarning($"No category match found for product '{product.Title}'. Defaulting to category {defaultCategoryId}");
            return defaultCategoryId;
        }

        /// <summary>
        /// Helper method to check if search text contains any of the specified keywords
        /// </summary>
        private bool ContainsAnyKeyword(string searchText, string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                if (searchText.Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
