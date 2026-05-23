namespace ACommerce.RideLifecycle.Models
{
    public enum RideState
    {
        Requested = 0,
        Matched = 1,
        Assigned = 2,
        Active = 3,
        Completed = 4,
        Cancelled = 5,
        Paid = 6
    }
}
