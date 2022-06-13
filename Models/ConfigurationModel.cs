using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baroque.DiscountRules.HasAllProducts.Models
{
    public class ConfigurationModel : BaseSearchModel
    {
        public int DiscountId { get; set; }

        public int RequirementId { get; set; }

        public class RequirementProductListModel : BasePagedListModel<RequirementProductModel>
        {
        }

        public class RequirementProductModel : BaseNopEntityModel
        {
            /// <summary>
            /// Gets or sets discount requirement rule id number
            /// </summary>
            public int RequirementId { get; set; }

            /// <summary>
            /// Gets or sets product picture Url
            /// </summary>
            public string PictureUrl { get; set; }

            /// <summary>
            /// GEts or sets product name(not localized)
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets product Sku
            /// </summary>
            public string Sku { get; set; }

            /// <summary>
            /// Gets or sets product publish value
            /// </summary>
            public bool Published { get; set; }

            /// <summary>
            /// Gets or sets product minimum quantity
            /// </summary>
            public int? MinQuantity { get; set; }

            /// <summary>
            /// Gets or sets product maximum quantity
            /// </summary>
            public int? MaxQuantity { get; set; }
        }
    }
}