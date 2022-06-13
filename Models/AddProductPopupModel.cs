using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Baroque.DiscountRules.HasAllProducts.Models
{
    public class AddProductPopupModel : ProductSearchModel
    {
        /// <summary>
        /// GEts or sets discount requirement id number
        /// </summary>
        public int RequirementId { get; set; }

        public int[] SelectedProductIds { get; set; }

        public AddProductPopupModel()
        {
            this.SelectedProductIds = new int[0];
        }

        public class ProductListModel: BasePagedListModel<ProductModel>
        {

        }

        public class ProductModel : BaseNopEntityModel
        { 
            public string PictureUrl { get; set; }

            public string Sku { get; set; }

            public string Name { get; set; }

            public bool Published { get; set; }
        }
    }
}
