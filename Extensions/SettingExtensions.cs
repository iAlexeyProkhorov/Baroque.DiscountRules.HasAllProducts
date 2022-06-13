using Nop.Core.Domain.Discounts;
using Nop.Plugin.Baroque.DiscountRules.HasAllProducts.Models;

namespace Nop.Plugin.Baroque.DiscountRules.HasAllProducts.Extensions
{
    public static class SettingExtensions
    {
        public const string HAS_ONE_PRODUCT = "DiscountRequirement.RestrictedProductIds-{0}";

        /// <summary>
        /// Format setting name for requirement rule
        /// </summary>
        /// <param name="requirementId">Requirement rule id number</param>
        /// <returns>Formatted setting name</returns>
        public static string FormatSettingName(int? requirementId)
        {
            var result = string.Format(HAS_ONE_PRODUCT, requirementId.GetValueOrDefault(0));
            return result;
        }

        /// <summary>
        /// Format setting name for discount requirement
        /// </summary>
        /// <param name="discountRequirement">Discount requirement entity</param>
        /// <returns>Formatted setting name</returns>
        public static string FormatSettingName(this DiscountRequirement discountRequirement)
        {
            return FormatSettingName(discountRequirement.Id);
        }

        /// <summary>
        /// Format setting name for discount requirement
        /// </summary>
        /// <param name="addProductPopup">Add product popup model</param>
        /// <returns>Formatted setting name</returns>
        public static string FormatSettingName(this AddProductPopupModel addProductPopup)
        {
            return FormatSettingName(addProductPopup.RequirementId);
        }

        /// <summary>
        /// Format setting name for discount requirement
        /// </summary>
        /// <param name="productModel">Requirement rule product model</param>
        /// <returns>Formatted setting name</returns>
        public static string FormatSettingName(this ConfigurationModel.RequirementProductModel productModel)
        {
            return FormatSettingName(productModel.RequirementId);
        }
    }
}
