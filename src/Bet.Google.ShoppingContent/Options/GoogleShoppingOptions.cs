namespace Bet.Google.ShoppingContent.Options
{
    public class GoogleShoppingOptions
    {
        public string? GoogleServiceAccountFile { get; set; }

#pragma warning disable SA1011 // Closing square brackets should be spaced correctly
        public byte[]? GoogleServiceAccount { get; set; }
#pragma warning restore SA1011 // Closing square brackets should be spaced correctly

        public int MaxListPageSize { get; set; } = 100;

        public ulong MerchantId { get; set; }
    }
}
