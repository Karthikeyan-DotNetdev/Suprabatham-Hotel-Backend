using CommonModels;
using CommonServices;
using laptop_service.Models.MASTERS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using WIN_BOT;

namespace laptop_service.Controllers.MASTERS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/product")]
    [ApiController]
    public class Product_Master_Controller : ControllerBase
    {
        // =========================================================
        // ADD PRODUCT
        // =========================================================

        [HttpPost]
        [Route("AddProduct")]
        public async Task<IActionResult> AddProduct(
            [FromBody] M_PRODUCT_MASTER product)
        {
            try
            {
                JObject jobject = new JObject();

                product.Product_Name = product.Product_Name?.Trim();
                product.Short_Name = product.Short_Name?.Trim();
                product.Barcode = product.Barcode?.Trim();
                product.Category_Code = product.Category_Code?.Trim();
                product.UOM_Code = product.UOM_Code?.Trim();
                product.HSN_Code = product.HSN_Code?.Trim();
                product.GST_Code = product.GST_Code?.Trim();
                product.Variant_Code = product.Variant_Code?.Trim();
                product.Kitchen_Code = product.Kitchen_Code?.Trim();
                product.Created_By = product.Created_By?.Trim();

                if (string.IsNullOrWhiteSpace(product.Product_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Product Name cannot be empty.");

                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(product.Category_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Please select Category.");

                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(product.UOM_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Please select UOM.");

                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(product.Created_By))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Created By cannot be empty.");

                    return Ok(jobject.ToString());
                }

                if ((product.MRP_Price ?? 0) < 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "MRP Price cannot be negative.");

                    return Ok(jobject.ToString());
                }

                if ((product.Cost_Price ?? 0) < 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cost Price cannot be negative.");

                    return Ok(jobject.ToString());
                }

                if ((product.Selling_Price ?? 0) < 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Selling Price cannot be negative."
                    );

                    return Ok(jobject.ToString());
                }

                string safeProductName =
                    product.Product_Name.Replace("'", "''");

                string duplicateQuery = $@"
                    SELECT COUNT(*)
                    FROM M_ProductMaster
                    WHERE UPPER(LTRIM(RTRIM(Product_Name))) =
                          UPPER(LTRIM(RTRIM('{safeProductName}')))
                    AND Is_Active <> 'D'";

                int duplicateCount =
                    SQLService.ExecuteScalarQuery(duplicateQuery);

                if (duplicateCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Product Name already exists.");

                    return Ok(jobject.ToString());
                }

                if (!string.IsNullOrWhiteSpace(product.Barcode))
                {
                    string safeBarcode =
                        product.Barcode.Replace("'", "''");

                    string barcodeQuery = $@"
                        SELECT COUNT(*)
                        FROM M_ProductMaster
                        WHERE Barcode = '{safeBarcode}'
                        AND Is_Active <> 'D'";

                    int barcodeCount =
                        SQLService.ExecuteScalarQuery(barcodeQuery);

                    if (barcodeCount > 0)
                    {
                        jobject.Add("status", false);
                        jobject.Add("message", "Barcode already exists.");

                        return Ok(jobject.ToString());
                    }
                }

                product = await LoadProductMaxCode(product);

                var data = new M_PRODUCT_MASTER
                {
                    Product_Code = product.Product_Code,

                    Product_Name =
                        product.Product_Name.ToUpper(),

                    Short_Name =
                        string.IsNullOrWhiteSpace(product.Short_Name)
                            ? null
                            : product.Short_Name.ToUpper(),

                    Product_Description =
                        product.Product_Description,

                    Barcode =
                        string.IsNullOrWhiteSpace(product.Barcode)
                            ? null
                            : product.Barcode,

                    Category_Code = product.Category_Code,
                    UOM_Code = product.UOM_Code,

                    HSN_Code =
                        string.IsNullOrWhiteSpace(product.HSN_Code)
                            ? null
                            : product.HSN_Code,

                    GST_Code =
                        string.IsNullOrWhiteSpace(product.GST_Code)
                            ? null
                            : product.GST_Code,

                    Variant_Code =
                        string.IsNullOrWhiteSpace(product.Variant_Code)
                            ? null
                            : product.Variant_Code,

                    Kitchen_Code =
                        string.IsNullOrWhiteSpace(product.Kitchen_Code)
                            ? null
                            : product.Kitchen_Code,

                    MRP_Price = product.MRP_Price ?? 0,
                    Cost_Price = product.Cost_Price ?? 0,
                    Selling_Price = product.Selling_Price ?? 0,

                    Is_Taxable =
                        string.IsNullOrWhiteSpace(product.Is_Taxable)
                            ? "Y"
                            : product.Is_Taxable.ToUpper(),

                    Tax_Inclusive =
                        string.IsNullOrWhiteSpace(product.Tax_Inclusive)
                            ? "N"
                            : product.Tax_Inclusive.ToUpper(),

                    Is_Stock_Item =
                        string.IsNullOrWhiteSpace(product.Is_Stock_Item)
                            ? "N"
                            : product.Is_Stock_Item.ToUpper(),

                    Is_Available =
                        string.IsNullOrWhiteSpace(product.Is_Available)
                            ? "Y"
                            : product.Is_Available.ToUpper(),

                    Display_Order = product.Display_Order ?? 0,

                    Is_Active = "A",

                    Created_By = product.Created_By,

                    Created_On =
                        DateTime.Now.ToString(
                            "yyyy-MM-dd HH:mm:ss"
                        ),

                    Updated_By = null,
                    Updated_On = null
                };

                var ignoredColumns = new List<string>
                {
                    "Product_Id",
                    "Updated_By",
                    "Updated_On"
                };

                string insertQuery = SQLHelper.BuildInsertQuery(
                    data,
                    "M_ProductMaster",
                    ignoredColumns
                );

                int result =
                    SQLService.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add(
                        "message",
                        "Product saved successfully."
                    );

                    jobject.Add(
                        "product_code",
                        product.Product_Code
                    );
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot save Product.");
                }

                return Ok(jobject.ToString());
            }
            catch (Exception ex)
            {
                return Ok(new JObject
                {
                    { "status", false },
                    { "message", ex.Message }
                }.ToString());
            }
        }

        // =========================================================
        // UPDATE PRODUCT
        // =========================================================

        [HttpPost]
        [Route("UpdateProduct/{Product_Code}")]
        public IActionResult UpdateProduct(
            string Product_Code,
            [FromBody] M_PRODUCT_MASTER product)
        {
            try
            {
                JObject jobject = new JObject();

                Product_Code = Product_Code?.Trim() ?? "";

                product.Product_Name =
                    product.Product_Name?.Trim();

                product.Short_Name =
                    product.Short_Name?.Trim();

                product.Barcode =
                    product.Barcode?.Trim();

                product.Category_Code =
                    product.Category_Code?.Trim();

                product.UOM_Code =
                    product.UOM_Code?.Trim();

                if (string.IsNullOrWhiteSpace(Product_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Product Code cannot be empty."
                    );

                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(product.Product_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Product Name cannot be empty."
                    );

                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(product.Category_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Please select Category.");

                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(product.UOM_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Please select UOM.");

                    return Ok(jobject.ToString());
                }

                if ((product.MRP_Price ?? 0) < 0 ||
                    (product.Cost_Price ?? 0) < 0 ||
                    (product.Selling_Price ?? 0) < 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Product prices cannot be negative."
                    );

                    return Ok(jobject.ToString());
                }

                string safeProductCode =
                    Product_Code.Replace("'", "''");

                string safeProductName =
                    product.Product_Name.Replace("'", "''");

                string duplicateQuery = $@"
                    SELECT COUNT(*)
                    FROM M_ProductMaster
                    WHERE UPPER(LTRIM(RTRIM(Product_Name))) =
                          UPPER(LTRIM(RTRIM('{safeProductName}')))
                    AND Product_Code <> '{safeProductCode}'
                    AND Is_Active <> 'D'";

                int duplicateCount =
                    SQLService.ExecuteScalarQuery(duplicateQuery);

                if (duplicateCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Product Name already exists."
                    );

                    return Ok(jobject.ToString());
                }

                if (!string.IsNullOrWhiteSpace(product.Barcode))
                {
                    string safeBarcode =
                        product.Barcode.Replace("'", "''");

                    string barcodeQuery = $@"
                        SELECT COUNT(*)
                        FROM M_ProductMaster
                        WHERE Barcode = '{safeBarcode}'
                        AND Product_Code <> '{safeProductCode}'
                        AND Is_Active <> 'D'";

                    int barcodeCount =
                        SQLService.ExecuteScalarQuery(barcodeQuery);

                    if (barcodeCount > 0)
                    {
                        jobject.Add("status", false);
                        jobject.Add("message", "Barcode already exists.");

                        return Ok(jobject.ToString());
                    }
                }

                var data = new M_PRODUCT_MASTER
                {
                    Product_Code = Product_Code,

                    Product_Name =
                        product.Product_Name.ToUpper(),

                    Short_Name =
                        string.IsNullOrWhiteSpace(product.Short_Name)
                            ? null
                            : product.Short_Name.ToUpper(),

                    Product_Description =
                        product.Product_Description,

                    Barcode =
                        string.IsNullOrWhiteSpace(product.Barcode)
                            ? null
                            : product.Barcode,

                    Category_Code = product.Category_Code,
                    UOM_Code = product.UOM_Code,

                    HSN_Code =
                        string.IsNullOrWhiteSpace(product.HSN_Code)
                            ? null
                            : product.HSN_Code,

                    GST_Code =
                        string.IsNullOrWhiteSpace(product.GST_Code)
                            ? null
                            : product.GST_Code,

                    Variant_Code =
                        string.IsNullOrWhiteSpace(product.Variant_Code)
                            ? null
                            : product.Variant_Code,

                    Kitchen_Code =
                        string.IsNullOrWhiteSpace(product.Kitchen_Code)
                            ? null
                            : product.Kitchen_Code,

                    MRP_Price = product.MRP_Price ?? 0,
                    Cost_Price = product.Cost_Price ?? 0,
                    Selling_Price = product.Selling_Price ?? 0,

                    Is_Taxable =
                        string.IsNullOrWhiteSpace(product.Is_Taxable)
                            ? "Y"
                            : product.Is_Taxable.ToUpper(),

                    Tax_Inclusive =
                        string.IsNullOrWhiteSpace(product.Tax_Inclusive)
                            ? "N"
                            : product.Tax_Inclusive.ToUpper(),

                    Is_Stock_Item =
                        string.IsNullOrWhiteSpace(product.Is_Stock_Item)
                            ? "N"
                            : product.Is_Stock_Item.ToUpper(),

                    Is_Available =
                        string.IsNullOrWhiteSpace(product.Is_Available)
                            ? "Y"
                            : product.Is_Available.ToUpper(),

                    Display_Order = product.Display_Order ?? 0,

                    Is_Active =
                        string.IsNullOrWhiteSpace(product.Is_Active)
                            ? "A"
                            : product.Is_Active.ToUpper(),

                    Updated_By = product.Updated_By,

                    Updated_On =
                        DateTime.Now.ToString(
                            "yyyy-MM-dd HH:mm:ss"
                        )
                };

                var ignoredColumns = new List<string>
                {
                    "Product_Id",
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    data,
                    "M_ProductMaster",
                    "Product_Code",
                    Product_Code,
                    ignoredColumns
                );

                int result =
                    SQLService.ExecuteNonQuery(updateQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add(
                        "message",
                        "Product updated successfully."
                    );
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Product not found or cannot update."
                    );
                }

                return Ok(jobject.ToString());
            }
            catch (Exception ex)
            {
                return Ok(new JObject
                {
                    { "status", false },
                    { "message", ex.Message }
                }.ToString());
            }
        }

        // =========================================================
        // DELETE PRODUCT - SOFT DELETE
        // =========================================================

        [HttpGet]
        [Route("DeleteProduct/{Product_Code}")]
        public IActionResult DeleteProduct(
            string Product_Code,
            [FromQuery] string? Updated_By)
        {
            try
            {
                JObject jobject = new JObject();

                if (string.IsNullOrWhiteSpace(Product_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Product Code cannot be empty."
                    );

                    return Ok(jobject.ToString());
                }

                string safeProductCode =
                    Product_Code.Replace("'", "''");

                string safeUpdatedBy =
                    (Updated_By ?? "").Replace("'", "''");

                string query = $@"
                    UPDATE M_ProductMaster
                    SET
                        Is_Active = 'D',
                        Is_Available = 'N',
                        Updated_By = '{safeUpdatedBy}',
                        Updated_On = GETDATE()
                    WHERE Product_Code = '{safeProductCode}'
                    AND Is_Active <> 'D'";

                int result =
                    SQLService.ExecuteNonQuery(query);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add(
                        "message",
                        "Product deleted successfully."
                    );
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add(
                        "message",
                        "Product not found or already deleted."
                    );
                }

                return Ok(jobject.ToString());
            }
            catch (Exception ex)
            {
                return Ok(new JObject
                {
                    { "status", false },
                    { "message", ex.Message }
                }.ToString());
            }
        }

        // =========================================================
        // PRODUCT LIST
        // =========================================================

        [HttpGet]
        [Route("ProductList")]
        public IActionResult ProductList()
        {
            try
            {
                ApiResponse response = new ApiResponse();

                DataTable dt = SQLService.GetDataTable(
                    "EXEC SP_ProductMasterList"
                );

                response.status = true;
                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // =========================================================
        // PRODUCT BY CODE
        // =========================================================

        [HttpGet]
        [Route("ProductByCode/{Product_Code}")]
        public IActionResult ProductByCode(string Product_Code)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                string safeProductCode =
                    Product_Code.Replace("'", "''");

                DataTable dt = SQLService.GetDataTable($@"
                    EXEC SP_ProductMasterByCode
                        @Product_Code = '{safeProductCode}'"
                );

                response.status = dt.Rows.Count > 0;
                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // =========================================================
        // AVAILABLE PRODUCTS FOR POS BILLING
        // =========================================================

        [HttpGet]
        [Route("AvailableProductList")]
        public IActionResult AvailableProductList()
        {
            try
            {
                ApiResponse response = new ApiResponse();

                DataTable dt = SQLService.GetDataTable(@"
                    EXEC SP_AvailableProductList"
                );

                response.status = true;
                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // =========================================================
        // AUTOMATIC PRODUCT CODE
        // =========================================================

        private Task<M_PRODUCT_MASTER> LoadProductMaxCode(
            M_PRODUCT_MASTER product)
        {
            DataTable dt = SQLService.GetDataTable(@"
                SELECT
                    ISNULL(
                        MAX(
                            TRY_CAST(
                                SUBSTRING(
                                    Product_Code,
                                    2,
                                    LEN(Product_Code)
                                ) AS INT
                            )
                        ),
                        0
                    ) + 1 AS NextCode
                FROM M_ProductMaster
                WHERE Product_Code LIKE 'P%'"
            );

            if (dt.Rows.Count > 0)
            {
                int code =
                    Convert.ToInt32(dt.Rows[0]["NextCode"]);

                product.Product_Code =
                    "P" + code.ToString().PadLeft(5, '0');
            }

            return Task.FromResult(product);
        }

        public class ToggleFavoriteRequest
        {
            public string ProductCode { get; set; } = string.Empty;
            public string IsFavorite { get; set; } = "N";
        }

        [HttpPost]
        [Route("ToggleFavorite")]
        public IActionResult ToggleFavorite([FromBody] ToggleFavoriteRequest req)
        {
            try
            {
                if (string.IsNullOrEmpty(req.ProductCode))
                {
                    return Ok(new { status = false, message = "ProductCode cannot be empty" });
                }

                string status = req.IsFavorite == "Y" ? "Y" : "N";
                string query = $"UPDATE M_ProductMaster SET Is_Favorite = '{status}' WHERE Product_Code = '{req.ProductCode}'";
                int rows = SQLService.ExecuteNonQuery(query);

                return Ok(new { status = rows > 0, message = rows > 0 ? "Toggled successfully" : "No rows updated" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = false, message = ex.Message });
            }
        }
    }
}