using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ChudoObuv.Models;
using Microsoft.EntityFrameworkCore;

namespace ChudoObuv
{
    internal static class CatalogService
    {
        public static List<CatalogItem> Load()
        {
            using var db = new AppDbContext();

            // прошлый календарный месяц
            var today = DateOnly.FromDateTime(DateTime.Today);
            var thisMonth = new DateOnly(today.Year, today.Month, 1);
            var prevMonth = thisMonth.AddMonths(-1);

            // товары, которые заказывали в прошлом месяце (скидки на них нет)
            var soldIds = db.OrderItems
                .Where(oi => oi.Order.OrderDate >= prevMonth && oi.Order.OrderDate < thisMonth)
                .Select(oi => oi.StockItem.ProductID)
                .Distinct()
                .ToHashSet();

            var products = db.Products
                .Include(p => p.Manufacturer)
                .Include(p => p.Subcategory).ThenInclude(s => s.Category)
                .Include(p => p.StockItems)
                .ToList();

            string imageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

            return products.Select(p =>
            {
                bool discount = !soldIds.Contains(p.ProductID);
                return new CatalogItem
                {
                    ProductId = p.ProductID,
                    Name = p.Name,
                    Manufacturer = p.Manufacturer?.Name ?? "",
                    Category = p.Subcategory?.Category?.Name ?? "",
                    Composition = p.Composition ?? "",
                    Description = p.Description ?? "",
                    Image = GetImagePath(imageDir, p.Image),
                    Stock = p.StockItems.Sum(s => s.Quantity),
                    Price = p.Price,
                    HasDiscount = discount,
                    PriceWithDiscount = discount ? Math.Round(p.Price * 0.75m, 2) : p.Price
                };
            }).ToList();
        }

        // если файла с картинкой нет, показываем заглушку из ресурсов
        private static string GetImagePath(string imageDir, string? image)
        {
            if (!string.IsNullOrWhiteSpace(image))
            {
                string path = Path.Combine(imageDir, image.TrimStart('/', '\\'));
                if (File.Exists(path)) return path;
            }
            return "pack://application:,,,/picture.png";
        }
    }
}
