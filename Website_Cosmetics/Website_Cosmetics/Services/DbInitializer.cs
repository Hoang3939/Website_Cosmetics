using Website_Cosmetics.Models;

namespace Website_Cosmetics.Services;

public class DbInitializer
{
    public static void Initialize(WebsiteCosmeticContext context)
    {
        // Clear existing data to reseed with English data
        if (context.Products.Any())
        {
            // Remove all existing data
            context.ProductImages.RemoveRange(context.ProductImages);
            context.Products.RemoveRange(context.Products);
            context.Categories.RemoveRange(context.Categories);
            context.Brands.RemoveRange(context.Brands);
            context.SaveChanges();
        }

        var now = DateTime.Now;

        // Add sample brands
        var brands = new Brand[]
        {
            new Brand { BrandId = Guid.NewGuid(), Name = "L'Oréal", Country = "France", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Brand { BrandId = Guid.NewGuid(), Name = "Maybelline", Country = "USA", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Brand { BrandId = Guid.NewGuid(), Name = "MAC Cosmetics", Country = "Canada", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Brand { BrandId = Guid.NewGuid(), Name = "NARS", Country = "France", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Brand { BrandId = Guid.NewGuid(), Name = "Fenty Beauty", Country = "USA", IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        context.Brands.AddRange(brands);
        context.SaveChanges();

        // Add sample categories
        var categories = new Category[]
        {
            new Category { CategoryId = Guid.NewGuid(), Name = "Lipstick", Description = "Lipstick products in various finishes", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Category { CategoryId = Guid.NewGuid(), Name = "Eye Shadow", Description = "Eye shadow palettes and singles", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Category { CategoryId = Guid.NewGuid(), Name = "Blush", Description = "Powder and cream blush products", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Category { CategoryId = Guid.NewGuid(), Name = "Foundation", Description = "Foundation for all skin types", IsActive = true, CreatedAt = now, UpdatedAt = now },
            new Category { CategoryId = Guid.NewGuid(), Name = "Powder", Description = "Setting and finishing powders", IsActive = true, CreatedAt = now, UpdatedAt = now }
        };

        context.Categories.AddRange(categories);
        context.SaveChanges();

        // Add sample products
        var products = new Product[]
        {
            // Most Loved Products (8 products)
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Maybelline SuperStay Matte Ink",
                Slug = "maybelline-superstay-matte-ink",
                Price = 15.00m,
                Description = "Long-lasting matte lipstick with up to 16 hours wear.",
                Ingredients = "Isododecane, Dimethicone, Silica.",
                IsActive = true,
                BrandId = brands[1].BrandId,
                CategoryId = categories[0].CategoryId,
                CreatedAt = now.AddDays(-30),
                UpdatedAt = now
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "L'Oréal Rouge Signature Liquid Lipstick",
                Slug = "loreal-rouge-signature",
                Price = 12.00m,
                Description = "Lightweight matte liquid lipstick with comfortable wear.",
                Ingredients = "Dimethicone, Isododecane, Silica.",
                IsActive = true,
                BrandId = brands[0].BrandId,
                CategoryId = categories[0].CategoryId,
                CreatedAt = now.AddDays(-28),
                UpdatedAt = now
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "NARS Afterglow Lip Balm",
                Slug = "nars-afterglow-lip-balm",
                Price = 28.00m,
                Description = "Luxurious tinted lip balm with natural finish.",
                Ingredients = "Natural oils, Vitamin E, Pigments.",
                IsActive = true,
                BrandId = brands[3].BrandId,
                CategoryId = categories[0].CategoryId,
                CreatedAt = now.AddDays(-25),
                UpdatedAt = now
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "MAC Powder Kiss Lipstick",
                Slug = "mac-powder-kiss",
                Price = 22.00m,
                Description = "Soft matte lipstick with powder-like finish.",
                Ingredients = "Silica, Dimethicone, Color pigments.",
                IsActive = true,
                BrandId = brands[2].BrandId,
                CategoryId = categories[0].CategoryId,
                CreatedAt = now.AddDays(-22),
                UpdatedAt = now
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "L'Oréal Paradise Enchanted Palette",
                Slug = "loreal-paradise-palette",
                Price = 18.00m,
                Description = "12-shade eyeshadow palette with warm tones.",
                Ingredients = "Talc, Mica, Dimethicone.",
                IsActive = true,
                BrandId = brands[0].BrandId,
                CategoryId = categories[1].CategoryId,
                CreatedAt = now.AddDays(-20),
                UpdatedAt = now
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "NARS Orgasm Blush",
                Slug = "nars-orgasm-blush",
                Price = 32.00m,
                Description = "Iconic peachy-pink blush with golden shimmer.",
                Ingredients = "Talc, Nylon-12, Dimethicone.",
                IsActive = true,
                BrandId = brands[3].BrandId,
                CategoryId = categories[2].CategoryId,
                CreatedAt = now.AddDays(-18),
                UpdatedAt = now
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Fenty Cheeks Out Cream Blush",
                Slug = "fenty-cheeks-out",
                Price = 26.00m,
                Description = "Cream blush stick with natural, blendable finish.",
                Ingredients = "Dimethicone, Silica, Color pigments.",
                IsActive = true,
                BrandId = brands[4].BrandId,
                CategoryId = categories[2].CategoryId,
                CreatedAt = now.AddDays(-15),
                UpdatedAt = now
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Maybelline Lash Sensational Mascara",
                Slug = "maybelline-lash-sensational",
                Price = 9.00m,
                Description = "Volumizing mascara for longer, fuller lashes.",
                Ingredients = "Paraffin, Beeswax, Carnauba wax.",
                IsActive = true,
                BrandId = brands[1].BrandId,
                CategoryId = categories[1].CategoryId,
                CreatedAt = now.AddDays(-12),
                UpdatedAt = now
            },
            
            // Special Deals Products (8 products)
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "MAC Studio Fix Fluid SPF15",
                Slug = "mac-studio-fix-fluid",
                Price = 35.00m,
                Description = "Full coverage foundation with oil control.",
                Ingredients = "Water, Cyclopentasiloxane, Talc.",
                IsActive = true,
                BrandId = brands[2].BrandId,
                CategoryId = categories[3].CategoryId,
                CreatedAt = now.AddDays(-10),
                UpdatedAt = now.AddDays(-1)
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Fenty Beauty Pro Filt'r Powder",
                Slug = "fenty-pro-filtr-powder",
                Price = 36.00m,
                Description = "Soft matte longwear setting powder.",
                Ingredients = "Silica, Dimethicone, Mica.",
                IsActive = true,
                BrandId = brands[4].BrandId,
                CategoryId = categories[4].CategoryId,
                CreatedAt = now.AddDays(-8),
                UpdatedAt = now.AddDays(-1)
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "L'Oréal Infallible 24H Foundation",
                Slug = "loreal-infallible-24h",
                Price = 16.00m,
                Description = "24-hour wear foundation with full coverage.",
                Ingredients = "Cyclopentasiloxane, Dimethicone, Titanium Dioxide.",
                IsActive = true,
                BrandId = brands[0].BrandId,
                CategoryId = categories[3].CategoryId,
                CreatedAt = now.AddDays(-6),
                UpdatedAt = now.AddDays(-2)
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Maybelline Baby Skin Primer",
                Slug = "maybelline-baby-skin",
                Price = 8.00m,
                Description = "Pore-minimizing primer for smooth skin.",
                Ingredients = "Dimethicone, Silica, Polymethylsilsesquioxane.",
                IsActive = true,
                BrandId = brands[1].BrandId,
                CategoryId = categories[3].CategoryId,
                CreatedAt = now.AddDays(-5),
                UpdatedAt = now.AddDays(-1)
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "MAC Studio Face & Body Foundation",
                Slug = "mac-studio-face-body",
                Price = 38.00m,
                Description = "Lightweight, natural-looking foundation.",
                Ingredients = "Water, Glycerin, Dimethicone.",
                IsActive = true,
                BrandId = brands[2].BrandId,
                CategoryId = categories[3].CategoryId,
                CreatedAt = now.AddDays(-4),
                UpdatedAt = now.AddDays(-1)
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "NARS Radiant Creamy Concealer",
                Slug = "nars-radiant-concealer",
                Price = 31.00m,
                Description = "Multi-action concealer with high coverage.",
                Ingredients = "Water, Dimethicone, Glycerin.",
                IsActive = true,
                BrandId = brands[3].BrandId,
                CategoryId = categories[3].CategoryId,
                CreatedAt = now.AddDays(-3),
                UpdatedAt = now.AddDays(-1)
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "L'Oréal Infallible Setting Powder",
                Slug = "loreal-infallible-powder",
                Price = 11.00m,
                Description = "Long-lasting oil-control setting powder.",
                Ingredients = "Talc, Silica, Mica.",
                IsActive = true,
                BrandId = brands[0].BrandId,
                CategoryId = categories[4].CategoryId,
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now
            },
            new Product
            {
                ProductId = Guid.NewGuid(),
                Name = "Maybelline Fit Me Powder",
                Slug = "maybelline-fit-me-powder",
                Price = 7.00m,
                Description = "Natural, oil-controlling finishing powder.",
                Ingredients = "Talc, Dimethicone, Nylon-12.",
                IsActive = true,
                BrandId = brands[1].BrandId,
                CategoryId = categories[4].CategoryId,
                CreatedAt = now.AddDays(-1),
                UpdatedAt = now
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();

        // Add product images
        var productImages = new List<ProductImage>();
        
        for (int i = 0; i < products.Length; i++)
        {
            // Products 0-7: Most Loved (lipstick, eyeshadow, blush) - use img-cart-lip
            // Products 8-15: Special Deals (foundation, powder) - use img-cart-brush  
            string imageUrl = i < 8 
                ? "/public/images/products/img-cart-lip.png" 
                : "/public/images/products/img-cart-brush.png";
                
            productImages.Add(new ProductImage
            {
                ProductImageId = Guid.NewGuid(),
                ProductId = products[i].ProductId,
                Url = imageUrl,
                IsCover = true,
                CreatedAt = now
            });
        }

        context.ProductImages.AddRange(productImages);
        context.SaveChanges();
    }
}

