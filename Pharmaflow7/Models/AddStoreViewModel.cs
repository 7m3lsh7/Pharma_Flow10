namespace Pharmaflow7.Models
{
    public class AddStoreViewModel
    {
        public StoreViewModel NewStore { get; set; }
        public List<Store> ExistingStores { get; set; }

        public AddStoreViewModel()
        {
            NewStore = new StoreViewModel();
            ExistingStores = new List<Store>();
        }
    }
}