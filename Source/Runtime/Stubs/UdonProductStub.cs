using System;
using System.Threading.Tasks;
using VRC.Economy;
using VRC.SDKBase;

namespace VRC.SDK3.ClientSim
{
    public class UdonProductStub : IProduct
    {
        public UdonProductStub(IProduct product, VRCPlayerApi player)
        {
            ID = product.ID;
            Name = product.Name;
            Description = product.Description;
            Buyer = player;
        }
        
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public VRCPlayerApi Buyer { get; set; }

        public bool Equals(IProduct other)
        {
            return this.ID == other.ID;
        }

        public Task<bool> Create()
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete()
        {
            throw new NotImplementedException();
        }

        public Task<IProduct[]> Purchase(
            string variantID = null, 
            int totalPrice = 0, 
            int quantity = -1,
            Action onPurchaseSuccess = null,
            Action<string> onPurchaseError = null,
            string analyticsLocationType = null,
            string analyticsStoreId = null,
            string analyticsWorldId = null,
            string analyticsGroupId = null,
            string analyticsCreatorId = null)
        {
            throw new NotImplementedException();
        }
    }
}
