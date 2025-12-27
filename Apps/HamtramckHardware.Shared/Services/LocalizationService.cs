using ACommerce.Templates.Customer.Services;

namespace HamtramckHardware.Shared.Services;

/// <summary>
/// Localization service for Hamtramck Hardware (English only for now)
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly Dictionary<string, string> _translations = new()
    {
        // Navigation
        ["Home"] = "Home",
        ["Search"] = "Search",
        ["Cart"] = "Cart",
        ["Profile"] = "My Account",
        ["Categories"] = "Categories",
        ["Orders"] = "My Orders",
        ["Favorites"] = "Favorites",
        ["Settings"] = "Settings",
        ["Notifications"] = "Notifications",

        // Auth
        ["Login"] = "Login",
        ["Register"] = "Register",
        ["Logout"] = "Logout",
        ["Email"] = "Email",
        ["Password"] = "Password",
        ["ConfirmPassword"] = "Confirm Password",
        ["ForgotPassword"] = "Forgot Password?",
        ["CreateAccount"] = "Create Account",
        ["AlreadyHaveAccount"] = "Already have an account?",
        ["DontHaveAccount"] = "Don't have an account?",
        ["LoginWithEmail"] = "Login with Email",
        ["ContinueAsGuest"] = "Continue as Guest",

        // Products
        ["Products"] = "Products",
        ["ProductDetails"] = "Product Details",
        ["AddToCart"] = "Add to Cart",
        ["BuyNow"] = "Buy Now",
        ["InStock"] = "In Stock",
        ["OutOfStock"] = "Out of Stock",
        ["Price"] = "Price",
        ["Quantity"] = "Quantity",
        ["Description"] = "Description",
        ["Specifications"] = "Specifications",
        ["Reviews"] = "Reviews",
        ["RelatedProducts"] = "Related Products",

        // Categories
        ["Plumbing"] = "Plumbing",
        ["Electrical"] = "Electrical",
        ["Tools"] = "Tools & Hardware",
        ["LawnGarden"] = "Lawn & Garden",
        ["Paint"] = "Paint & Supplies",
        ["Lumber"] = "Lumber & Building",
        ["Flooring"] = "Flooring",
        ["HomeImprovement"] = "Home Improvement",

        // Cart & Checkout
        ["YourCart"] = "Your Cart",
        ["EmptyCart"] = "Your cart is empty",
        ["Subtotal"] = "Subtotal",
        ["Tax"] = "Tax",
        ["Total"] = "Total",
        ["Checkout"] = "Checkout",
        ["ContinueShopping"] = "Continue Shopping",
        ["Remove"] = "Remove",
        ["UpdateQuantity"] = "Update",

        // Checkout
        ["ShippingAddress"] = "Shipping Address",
        ["BillingAddress"] = "Billing Address",
        ["PaymentMethod"] = "Payment Method",
        ["PlaceOrder"] = "Place Order",
        ["OrderSummary"] = "Order Summary",
        ["ShippingMethod"] = "Shipping Method",
        ["FreeShipping"] = "Free Shipping",
        ["StandardShipping"] = "Standard Shipping",
        ["ExpressShipping"] = "Express Shipping",

        // Orders
        ["OrderHistory"] = "Order History",
        ["OrderNumber"] = "Order #",
        ["OrderDate"] = "Order Date",
        ["OrderStatus"] = "Status",
        ["TrackOrder"] = "Track Order",
        ["Pending"] = "Pending",
        ["Processing"] = "Processing",
        ["Shipped"] = "Shipped",
        ["Delivered"] = "Delivered",
        ["Cancelled"] = "Cancelled",

        // Profile
        ["MyAccount"] = "My Account",
        ["PersonalInfo"] = "Personal Information",
        ["Addresses"] = "Addresses",
        ["PaymentMethods"] = "Payment Methods",
        ["SaveChanges"] = "Save Changes",
        ["FirstName"] = "First Name",
        ["LastName"] = "Last Name",
        ["Phone"] = "Phone",

        // General
        ["Loading"] = "Loading...",
        ["Error"] = "Error",
        ["Success"] = "Success",
        ["Save"] = "Save",
        ["Cancel"] = "Cancel",
        ["Delete"] = "Delete",
        ["Edit"] = "Edit",
        ["Back"] = "Back",
        ["Next"] = "Next",
        ["Submit"] = "Submit",
        ["Close"] = "Close",
        ["Yes"] = "Yes",
        ["No"] = "No",
        ["Ok"] = "OK",
        ["SeeAll"] = "See All",
        ["ViewMore"] = "View More",

        // Store Info
        ["StoreName"] = "Hamtramck Hardware",
        ["StoreAddress"] = "11828 Conant St, Detroit, MI 48212",
        ["StorePhone"] = "(313) 365-2400",
        ["StoreHours"] = "Mon-Sat: 8AM-8PM, Sun: 10AM-6PM",

        // Services
        ["PropaneRefill"] = "Propane Refill - $9.99",
        ["WindowRepair"] = "Same Day Window Glass/Screen Repair",
        ["WaterTanks"] = "Water Tanks - $399",
    };

    public string CurrentLanguage => "en";
    public bool IsRtl => false;

    public string this[string key] => GetString(key);

    public string GetString(string key)
    {
        return _translations.TryGetValue(key, out var value) ? value : key;
    }

    public string GetString(string key, params object[] args)
    {
        var template = GetString(key);
        return string.Format(template, args);
    }

    public Task SetLanguageAsync(string language)
    {
        // English only for now
        return Task.CompletedTask;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }
}
