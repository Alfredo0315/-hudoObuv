using System;
using System.Collections.Generic;

namespace ChudoObuv
{
    internal static class Session
    {
        public static int UserId;
        public static string UserFio = "";
        public static string RoleName = "";
        public static bool IsGuest;
        public static List<CartItem> Cart = new List<CartItem>();

        public static bool IsAdmin => RoleName.Contains("админ", StringComparison.OrdinalIgnoreCase);
        public static bool IsManager => RoleName.Contains("менедж", StringComparison.OrdinalIgnoreCase);
        public static bool CanManageOrders => IsAdmin || IsManager;

        public static void Reset()
        {
            UserId = 0;
            UserFio = "";
            RoleName = "";
            IsGuest = false;
        }
    }

    internal class CartItem
    {
        public int StockItemId;
        public string ProductName = "";
        public string Size = "";
        public int Quantity;
        public decimal UnitPrice;   // цена уже со скидкой (если она есть)
    }
}