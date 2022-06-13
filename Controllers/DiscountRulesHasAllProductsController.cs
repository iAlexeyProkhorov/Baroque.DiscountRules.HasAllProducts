using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Baroque.DiscountRules.HasAllProducts.Extensions;
using Nop.Plugin.Baroque.DiscountRules.HasAllProducts.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Discounts;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Baroque.DiscountRules.HasAllProducts.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class DiscountRulesHasAllProductsController : BasePluginController
    {
        #region Fields

        private readonly IDiscountService _discountService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;

        #endregion

        #region Constructor

        public DiscountRulesHasAllProductsController(IDiscountService discountService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            ISettingService settingService)
        {
            _discountService = discountService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _settingService = settingService;
        }

        #endregion

        #region Utilites

        protected virtual string ParseToString(IList<ConfigurationModel.RequirementProductModel> requredItems)
        {
            string requiredRow = string.Empty;

            if (requredItems.Count > 0)
            {
                foreach (var item in requredItems)
                {
                    if (item.MaxQuantity != null && item.MinQuantity != null)
                    {
                        var sumrow = string.Format("{0}: {1}-{2}", item.Id, item.MinQuantity, item.MaxQuantity);
                        requiredRow = string.Concat(requiredRow, ", ", sumrow);
                    }
                    else
                        if (item.MinQuantity != null)
                    {
                        var sumrow = string.Format("{0}: {1}", item.Id, item.MinQuantity);
                        requiredRow = string.Concat(requiredRow, ", ", sumrow);
                    }
                    else
                    {
                        var sumrow = string.Format("{0}", item.Id);
                        requiredRow = string.Concat(requiredRow, ", ", sumrow);
                    }
                }
                requiredRow = requiredRow.Remove(0, 2);
            }
            return requiredRow;
        }

        protected virtual IPagedList<ConfigurationModel.RequirementProductModel> ParseToList(int requirementId)
        {
            string restrictedProductIds = _settingService.GetSettingByKey<string>(SettingExtensions.FormatSettingName(requirementId));
            var requiredItems = new List<ConfigurationModel.RequirementProductModel>();

            if (string.IsNullOrEmpty(restrictedProductIds))
                return new PagedList<ConfigurationModel.RequirementProductModel>(requiredItems, 0, int.MaxValue);

            //separate items in string row
            var restrictedProducts = restrictedProductIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();

            for (int i = 0; i <= restrictedProducts.Count - 1; i++)
            {
                var restrictedProduct = restrictedProducts[i];

                //go to next item
                if (string.IsNullOrWhiteSpace(restrictedProduct))
                    continue;

                int productId = 0;

                //try to get product id number
                if (restrictedProduct.Contains(":"))
                {
                    //go to next item if id can't be parsed
                    if (!int.TryParse(restrictedProduct.Split(new[] { ':' })[0], out productId))
                        continue;
                }
                else
                //go to next item if id can't be parsed
                    if (!int.TryParse(restrictedProduct, out productId))
                    continue;

                var product = _productService.GetProductById(productId);

                //go to next item if product aren't exist
                if (product == null)
                    continue;

                var picture = _pictureService.GetProductPicture(product, string.Empty);
                var model = new ConfigurationModel.RequirementProductModel
                {
                    Id = productId,
                    RequirementId = requirementId,
                    Sku = product.Sku,
                    Name = product.Name,
                    Published = product.Published,
                    PictureUrl = _pictureService.GetPictureUrl(picture, 120)
                };

                if (restrictedProduct.Contains(":"))
                {
                    if (restrictedProduct.Contains("-"))
                    {
                        int quantityMin;
                        int.TryParse(restrictedProduct.Split(new[] { ':' })[1].Split(new[] { '-' })[0], out quantityMin);
                        int quantityMax;
                        int.TryParse(restrictedProduct.Split(new[] { ':' })[1].Split(new[] { '-' })[1], out quantityMax);

                        model.MaxQuantity = quantityMax;
                        model.MinQuantity = quantityMin;
                    }
                    else
                    {
                        int quantity;
                        int.TryParse(restrictedProduct.Split(new[] { ':' })[1], out quantity);

                        model.MinQuantity = quantity;
                    }
                }
                requiredItems.Add(model);
            }

            return new PagedList<ConfigurationModel.RequirementProductModel>(requiredItems, 0, int.MaxValue);
        }

        #endregion

        #region Configuration

        public virtual IActionResult Configure(int discountId, int? discountRequirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);

            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            if (discountRequirementId.HasValue)
            {
                var discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);

                if (discountRequirement == null)
                    return Content("Failed to load requirement.");
            }

            var model = new ConfigurationModel
            {
                RequirementId = discountRequirementId ?? 0,
                DiscountId = discountId,
            };

            model.SetPopupGridPageSize();

            //add a prefix
            ViewData.TemplateInfo.HtmlFieldPrefix = $"DiscountRulesHasAllProducts{discountRequirementId.GetValueOrDefault(0)}";

            return View("~/Plugins/DiscountRules.HasAllProducts/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public virtual IActionResult Configure(int discountId, int? discountRequirementId, string empty)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageDiscounts))
                return Content("Access denied");

            var discount = _discountService.GetDiscountById(discountId);

            if (discount == null)
                throw new ArgumentException("Discount could not be loaded");

            DiscountRequirement discountRequirement = null;

            if (discountRequirementId.HasValue)
                discountRequirement = discount.DiscountRequirements.FirstOrDefault(dr => dr.Id == discountRequirementId.Value);

            if (discountRequirement != null)
            {
                //update existing rule
                _settingService.SetSetting(discountRequirement.FormatSettingName(), string.Empty);
            }
            else
            {
                //save new rule
                discountRequirement = new DiscountRequirement
                {
                    DiscountRequirementRuleSystemName = "DiscountRequirement.HasAllProducts"
                };

                discount.DiscountRequirements.Add(discountRequirement);
                _discountService.UpdateDiscount(discount);

                _settingService.SetSetting(discountRequirement.FormatSettingName(), string.Empty);
            }

            return Json(new { Result = true, NewRequirementId = discountRequirement.Id });
        }

        #endregion

        #region List / Update / Delete

        [HttpPost]
        public virtual IActionResult List(ConfigurationModel model)
        {
            var requirementProducts = ParseToList(model.RequirementId);
            var gridModel = new ConfigurationModel.RequirementProductListModel().PrepareToGrid(model, requirementProducts, () => requirementProducts);

            return Json(gridModel);
        }

        public virtual IActionResult UpdateProduct(ConfigurationModel.RequirementProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePlugins))
                return ErrorJson("Access denied");

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var requirementProducts = ParseToList(model.RequirementId);
            var updateProduct = requirementProducts.FirstOrDefault(x => x.Id == model.Id);

            if (updateProduct == null)
                return ErrorJson($"Product with id {model.Id} wasn't add to this  discount requirement rule.");

            updateProduct.MinQuantity = model.MinQuantity;
            updateProduct.MaxQuantity = model.MaxQuantity;

            var result = ParseToString(requirementProducts);

            _settingService.SetSetting(model.FormatSettingName(), result);

            return new NullJsonResult();
        }

        public virtual IActionResult DeleteProduct(ConfigurationModel.RequirementProductModel model)
        {
            var requirementProducts = ParseToList(model.RequirementId);

            var deleteProduct = requirementProducts.FirstOrDefault(x => x.Id == model.Id);
            if (deleteProduct == null)
                throw new Exception($"Product with id {model.Id} wasn't add to this  discount requirement rule.");

            //remove product from rule products
            requirementProducts.Remove(deleteProduct);

            //parse rule products to list
            var result = ParseToString(requirementProducts);

            //save updated rule products
            _settingService.SetSetting(model.FormatSettingName(), result);

            return new NullJsonResult();
        }

        #endregion

        #region Add Product Popup

        public virtual IActionResult AddProductPopup(int requirementId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var productSearchModel = _productModelFactory.PrepareProductSearchModel(new ProductSearchModel());
            var model = new AddProductPopupModel()
            {
                AllowVendorsToImportProducts = productSearchModel.AllowVendorsToImportProducts,
                AvailableCategories = productSearchModel.AvailableCategories,
                AvailableManufacturers = productSearchModel.AvailableManufacturers,
                AvailablePageSizes = productSearchModel.AvailablePageSizes,
                AvailableProductTypes = productSearchModel.AvailableProductTypes,
                AvailablePublishedOptions = productSearchModel.AvailablePublishedOptions,
                AvailableStores = productSearchModel.AvailableStores,
                AvailableVendors = productSearchModel.AvailableVendors,
                AvailableWarehouses = productSearchModel.AvailableWarehouses,
                IsLoggedInAsVendor = productSearchModel.IsLoggedInAsVendor,
                CustomProperties = productSearchModel.CustomProperties,
                Draw = productSearchModel.Draw,
                GoDirectlyToSku = productSearchModel.GoDirectlyToSku,
                HideStoresList = productSearchModel.HideStoresList,
                Length = productSearchModel.Length,
                RequirementId = requirementId,
                SearchCategoryId = productSearchModel.SearchCategoryId,
                SearchIncludeSubCategories = productSearchModel.SearchIncludeSubCategories,
                SearchManufacturerId = productSearchModel.SearchManufacturerId,
                SearchProductName = productSearchModel.SearchProductName,
                SearchProductTypeId = productSearchModel.SearchProductTypeId,
                SearchPublishedId = productSearchModel.SearchPublishedId,
                SearchStoreId = productSearchModel.SearchStoreId,
                SearchVendorId = productSearchModel.SearchVendorId,
                SearchWarehouseId = productSearchModel.SearchWarehouseId,
                Start = productSearchModel.Start
            };

            return View("~/Plugins/DiscountRules.HasAllProducts/Views/ProductAddPopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult AddProductPopup(AddProductPopupModel model)
        {
            var savedProductIds = ParseToList(model.RequirementId).Select(x => x.Id);
            var resultProductIds = savedProductIds.Concat(model.SelectedProductIds).Distinct();

            _settingService.SetSetting(model.FormatSettingName(), string.Join(", ", resultProductIds));

            ViewBag.RefreshPage = true;

            return View("~/Plugins/DiscountRules.HasAllProducts/Views/ProductAddPopup.cshtml", model);
        }

        [HttpPost]
        public virtual IActionResult AddProductPopupList(AddProductPopupModel model)
        {
            var products = _productService.SearchProducts(
                categoryIds: new List<int> { model.SearchCategoryId },
                manufacturerId: model.SearchManufacturerId,
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                pageIndex: model.Page - 1,
                pageSize: model.PageSize,
                showHidden: true
                );

            var gridModel = new AddProductPopupModel.ProductListModel().PrepareToGrid(model, products, () =>
            {
                return products.Select(product =>
                {
                    var picture = _pictureService.GetProductPicture(product, string.Empty);
                    return new AddProductPopupModel.ProductModel()
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Published = product.Published,
                        PictureUrl = _pictureService.GetPictureUrl(picture, 75, true),
                        Sku = product.Sku
                    };
                });
            });

            return Json(gridModel);
        }

        #endregion
    }
}