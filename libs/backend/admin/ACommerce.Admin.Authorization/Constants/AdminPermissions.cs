namespace ACommerce.Admin.Authorization.Constants;

public static class AdminPermissions
{
    public static class Dashboard
    {
        public const string View = "dashboard.view";
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Edit = "users.edit";
        public const string Delete = "users.delete";
    }

    public static class Listings
    {
        public const string View = "listings.view";
        public const string Approve = "listings.approve";
        public const string Reject = "listings.reject";
        public const string Edit = "listings.edit";
        public const string Delete = "listings.delete";
        public const string Feature = "listings.feature";
    }

    public static class Orders
    {
        public const string View = "orders.view";
        public const string Edit = "orders.edit";
        public const string Refund = "orders.refund";
        public const string Cancel = "orders.cancel";
    }

    public static class Vendors
    {
        public const string View = "vendors.view";
        public const string Approve = "vendors.approve";
        public const string Suspend = "vendors.suspend";
        public const string Edit = "vendors.edit";
    }

    public static class Reports
    {
        public const string View = "reports.view";
        public const string Export = "reports.export";
    }

    public static class Settings
    {
        public const string View = "settings.view";
        public const string Edit = "settings.edit";
    }

    public static class AdminUsers
    {
        public const string View = "admin_users.view";
        public const string Create = "admin_users.create";
        public const string Edit = "admin_users.edit";
        public const string Delete = "admin_users.delete";
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Create = "roles.create";
        public const string Edit = "roles.edit";
        public const string Delete = "roles.delete";
    }

    public static IEnumerable<(string Code, string Name, string Module)> GetAllPermissions()
    {
        return new List<(string Code, string Name, string Module)>
        {
            (Dashboard.View, "عرض لوحة التحكم", "Dashboard"),
            
            (Users.View, "عرض المستخدمين", "Users"),
            (Users.Create, "إنشاء مستخدم", "Users"),
            (Users.Edit, "تعديل مستخدم", "Users"),
            (Users.Delete, "حذف مستخدم", "Users"),
            
            (Listings.View, "عرض الإعلانات", "Listings"),
            (Listings.Approve, "الموافقة على إعلان", "Listings"),
            (Listings.Reject, "رفض إعلان", "Listings"),
            (Listings.Edit, "تعديل إعلان", "Listings"),
            (Listings.Delete, "حذف إعلان", "Listings"),
            (Listings.Feature, "تمييز إعلان", "Listings"),
            
            (Orders.View, "عرض الطلبات", "Orders"),
            (Orders.Edit, "تعديل طلب", "Orders"),
            (Orders.Refund, "استرداد طلب", "Orders"),
            (Orders.Cancel, "إلغاء طلب", "Orders"),
            
            (Vendors.View, "عرض البائعين", "Vendors"),
            (Vendors.Approve, "الموافقة على بائع", "Vendors"),
            (Vendors.Suspend, "تعليق بائع", "Vendors"),
            (Vendors.Edit, "تعديل بائع", "Vendors"),
            
            (Reports.View, "عرض التقارير", "Reports"),
            (Reports.Export, "تصدير التقارير", "Reports"),
            
            (Settings.View, "عرض الإعدادات", "Settings"),
            (Settings.Edit, "تعديل الإعدادات", "Settings"),
            
            (AdminUsers.View, "عرض المشرفين", "AdminUsers"),
            (AdminUsers.Create, "إنشاء مشرف", "AdminUsers"),
            (AdminUsers.Edit, "تعديل مشرف", "AdminUsers"),
            (AdminUsers.Delete, "حذف مشرف", "AdminUsers"),
            
            (Roles.View, "عرض الأدوار", "Roles"),
            (Roles.Create, "إنشاء دور", "Roles"),
            (Roles.Edit, "تعديل دور", "Roles"),
            (Roles.Delete, "حذف دور", "Roles"),
        };
    }
}
