        using CommonModels;
        using CommonServices;
        using laptop_service.Models.MASTERS;
        using Microsoft.AspNetCore.Authorization;
        using Microsoft.AspNetCore.Mvc;
        using Newtonsoft.Json.Linq;
        using System.Data;
        using System.Globalization;
        using System.Text;
        using WIN_BOT;

        namespace laptop_service.Controllers.MASTERS
        {
            [Authorize]
            [AllowAnonymous]
            [Route("api/kot")]
            [ApiController]
            public class KOT_Controller : Controller
            {
                // ============================================================
                // SAVE FIRST KOT
                // Creates:
                // 1. M_KOT_Main record
                // 2. M_KOT_Details records with SUB_01
                // ============================================================

                [HttpPost]
                [Route("SaveKOT")]
                public IActionResult SaveKOT([FromBody] KOT_SAVE_REQUEST request)
                {
                    try
                    {
                        JObject response = new JObject();

                        if (request == null)
                        {
                            response.Add("status", false);
                            response.Add("message", "KOT request cannot be empty.");

                            return Ok(response.ToString());
                        }

                        // Normalize Order Type before validation.
                        request.Order_Type =
                            request.Order_Type?.Trim().ToUpper() ?? "DINE_IN";

                        request.Branch_Code =
                            request.Branch_Code?.Trim();

                        request.Table_Code =
                            request.Table_Code?.Trim();

                        request.Customer_Code =
                            request.Customer_Code?.Trim();

                        request.Customer_Name =
                            request.Customer_Name?.Trim();

                        request.Mobile_No =
                            request.Mobile_No?.Trim();

                        request.Created_By =
                            request.Created_By?.Trim();

                        string validationMessage =
                            ValidateNewKOT(request);

                        if (!string.IsNullOrWhiteSpace(validationMessage))
                        {
                            response.Add("status", false);
                            response.Add("message", validationMessage);

                            return Ok(response.ToString());
                        }

                /*
                 * DINE_IN:
                 * Check whether the selected table already has an active KOT.
                 *
                 * TAKEAWAY:
                 * Do not check Table_Code because it is not required.
                 */
                // ============================================================
                // CHECK TABLE SEAT AVAILABILITY
                // Allows multiple KOTs for the same table based on chair count.
                // ============================================================

                if (request.Order_Type == "DINE_IN")
                {
                    string seatAvailabilityQuery = $@"
SELECT
    FT.Chair AS Total_Seats,

    ISNULL
    (
        (
            SELECT SUM(ISNULL(KM.Guest_Count, 0))
            FROM M_KOT_Main KM
            WHERE KM.Branch_Code = FT.Branch_Code
            AND KM.Table_Code = FT.Table_Code
            AND KM.Order_Type = 'DINE_IN'
            AND KM.Order_Status IN ('KOT', 'CHECKOUT')
            AND KM.Is_Active = 'A'
        ),
        0
    ) AS Occupied_Seats

FROM floor_table_master FT
WHERE FT.Branch_Code =
    '{EscapeSql(request.Branch_Code)}'
AND FT.Table_Code =
    '{EscapeSql(request.Table_Code)}'
AND FT.Is_Active = 'A';";

                    DataTable seatTable =
                        SQLService.GetDataTable(seatAvailabilityQuery);

                    if (seatTable == null ||
                        seatTable.Rows.Count == 0)
                    {
                        response.Add("status", false);
                        response.Add(
                            "message",
                            "Selected table was not found or is inactive."
                        );

                        return Ok(response.ToString());
                    }

                    DataRow seatRow =
                        seatTable.Rows[0];

                    int totalSeats =
                        seatRow["Total_Seats"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                seatRow["Total_Seats"]
                            );

                    int occupiedSeats =
                        seatRow["Occupied_Seats"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(
                                seatRow["Occupied_Seats"]
                            );

                    int availableSeats =
                        totalSeats - occupiedSeats;

                    if (totalSeats <= 0)
                    {
                        response.Add("status", false);
                        response.Add(
                            "message",
                            "Chair count is not configured for the selected table."
                        );

                        return Ok(response.ToString());
                    }

                    if (availableSeats <= 0)
                    {
                        response.Add("status", false);
                        response.Add(
                            "message",
                            "All seats are occupied for the selected table."
                        );

                        response.Add("total_seats", totalSeats);
                        response.Add("occupied_seats", occupiedSeats);
                        response.Add("available_seats", 0);

                        return Ok(response.ToString());
                    }

                    if (request.Guest_Count > availableSeats)
                    {
                        response.Add("status", false);

                        response.Add(
                            "message",
                            $"Only {availableSeats} seat(s) are available for this table."
                        );

                        response.Add("requested_guests", request.Guest_Count);
                        response.Add("total_seats", totalSeats);
                        response.Add("occupied_seats", occupiedSeats);
                        response.Add("available_seats", availableSeats);

                        return Ok(response.ToString());
                    }
                }

                decimal totalQty =
                            request.Items.Sum(x => x.Qty);

                        decimal totalAmount = request.Items.Sum(
                            x => x.Amount > 0
                                ? x.Amount
                                : x.Qty * x.Rate
                        );

                        string sql = BuildNewKOTTransactionQuery(
                            request,
                            totalQty,
                            totalAmount
                        );

                        DataTable resultTable =
                            SQLService.GetDataTable(sql);

                        if (resultTable.Rows.Count == 0)
                        {
                            response.Add("status", false);
                            response.Add("message", "Cannot save KOT.");

                            return Ok(response.ToString());
                        }

                        DataRow row = resultTable.Rows[0];

                        response.Add("status", true);
                        response.Add("message", "KOT Saved Successfully");
                        response.Add(
                            "kot_id",
                            Convert.ToInt64(row["KOT_Id"])
                        );
                        response.Add(
                            "kot_no",
                            row["KOT_No"].ToString()
                        );
                        response.Add(
                            "sub_kot_no",
                            row["Sub_KOT_No"].ToString()
                        );

                        return Ok(response.ToString());
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

                // ============================================================
                // REORDER
                // Uses same KOT main record.
                // Adds only new detail rows with SUB_02, SUB_03, etc.
                // ============================================================

                [HttpPost]
                [Route("ReorderKOT")]
                public IActionResult ReorderKOT(
                    [FromBody] KOT_REORDER_REQUEST request)
                {
                    try
                    {
                        JObject response = new JObject();

                        if (request.KOT_Id <= 0 &&
                            string.IsNullOrWhiteSpace(request.KOT_No))
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "KOT Id or KOT Number is required."
                            );

                            return Ok(response.ToString());
                        }

                        if (request.Items == null || request.Items.Count == 0)
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "At least one product is required."
                            );

                            return Ok(response.ToString());
                        }

                        if (string.IsNullOrWhiteSpace(request.Updated_By))
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Updated By cannot be empty."
                            );

                            return Ok(response.ToString());
                        }

                        string itemValidation =
                            ValidateKOTItems(request.Items);

                        if (!string.IsNullOrWhiteSpace(itemValidation))
                        {
                            response.Add("status", false);
                            response.Add("message", itemValidation);

                            return Ok(response.ToString());
                        }

                        string kotWhere = request.KOT_Id > 0
                            ? $"KOT_Id = {request.KOT_Id}"
                            : $"KOT_No = '{EscapeSql(request.KOT_No)}'";

                        string checkQuery = $@"
        SELECT COUNT(*)
        FROM M_KOT_Main
        WHERE {kotWhere}
        AND Order_Status = 'KOT'
        AND Is_Active = 'A'";

                        int kotCount =
                            SQLService.ExecuteScalarQuery(checkQuery);

                        if (kotCount == 0)
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Active KOT not found or KOT is already checked out."
                            );

                            return Ok(response.ToString());
                        }

                        decimal reorderQty =
                            request.Items.Sum(x => x.Qty);

                        decimal reorderAmount = request.Items.Sum(
                            x => x.Amount > 0
                                ? x.Amount
                                : x.Qty * x.Rate
                        );

                        string sql = BuildReorderTransactionQuery(
                            request,
                            reorderQty,
                            reorderAmount
                        );

                        DataTable resultTable =
                            SQLService.GetDataTable(sql);

                        if (resultTable.Rows.Count == 0)
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Cannot save KOT reorder."
                            );

                            return Ok(response.ToString());
                        }

                        DataRow row = resultTable.Rows[0];

                        response.Add("status", true);
                        response.Add(
                            "message",
                            "KOT Reorder Saved Successfully"
                        );
                        response.Add("kot_id", Convert.ToInt64(row["KOT_Id"]));
                        response.Add("kot_no", row["KOT_No"].ToString());
                        response.Add("sub_kot_no", row["Sub_KOT_No"].ToString());
                        response.Add(
                            "sub_kot_sequence",
                            Convert.ToInt32(row["Sub_KOT_Sequence"])
                        );

                        return Ok(response.ToString());
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

        // ============================================================
        // KOT LIST
        // Optional filters:
        // branchCode
        // orderStatus
        // ============================================================
        [HttpGet]
        [Route("KOTList")]
        public IActionResult KOTList(
string? branchCode = null,
string? orderStatus = null,
string? orderType = null)
        {
            try
            {
                ApiResponse response =
                    new ApiResponse();

                branchCode =
                    string.IsNullOrWhiteSpace(branchCode)
                        ? null
                        : branchCode.Trim();

                orderStatus =
                    string.IsNullOrWhiteSpace(orderStatus)
                        ? null
                        : orderStatus.Trim().ToUpper();

                orderType =
                    string.IsNullOrWhiteSpace(orderType)
                        ? null
                        : orderType.Trim().ToUpper();

                if (orderType != null &&
                    orderType != "DINE_IN" &&
                    orderType != "TAKEAWAY")
                {
                    response.status = false;
                    response.message =
                        "Order Type must be DINE_IN or TAKEAWAY.";

                    return new JsonResult(response);
                }

                string query = $@"
EXEC SP_KOT_LIST
    @BranchCode = {SqlNullable(branchCode)},
    @OrderStatus = {SqlNullable(orderStatus)},
    @OrderType = {SqlNullable(orderType)}";

                DataTable dt =
                    SQLService.GetDataTable(query);

                response.status = true;
                response.message =
                    dt.Rows.Count > 0
                        ? "KOT list loaded successfully."
                        : "No KOT records found.";

                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // GET COMPLETE KOT
        // Returns main data and details grouped by Sub KOT.
        // ============================================================

        [HttpGet]
        [Route("GetKOTById/{kotId:long}")]
        public IActionResult GetKOTById(long kotId)
        {
            try
            {
                if (kotId <= 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Valid KOT Id is required."
                    });
                }

                string query = $@"
EXEC SP_GET_KOT_MAIN_BY_ID
    @KOT_Id = {kotId}";

                DataSet ds =
                    SQLService.GetDataSet(query);

                if (ds == null ||
                    ds.Tables.Count == 0 ||
                    ds.Tables[0].Rows.Count == 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "KOT not found."
                    });
                }

                DataTable mainTable =
                    ds.Tables[0];

                DataTable detailTable =
                    ds.Tables.Count > 1
                        ? ds.Tables[1]
                        : new DataTable();

                var mainArray =
                    UtilityService.DataTableToJArray(
                        mainTable
                    );

                var detailArray =
                    UtilityService.DataTableToJArray(
                        detailTable
                    );

                return new JsonResult(new
                {
                    status = true,
                    message =
                        "KOT details loaded successfully.",

                    data = new
                    {
                        main =
                            mainArray.FirstOrDefault(),

                        details =
                            detailArray
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        // ============================================================
        // GET KOT BY TABLE
        // Used when opening an occupied table.
        // ============================================================
        [HttpGet]
        [Route("GetActiveKOTByTable")]
        public IActionResult GetActiveKOTByTable(
            string branchCode,
            string tableCode)
        {
            try
            {
                branchCode =
                    branchCode?.Trim() ?? string.Empty;

                tableCode =
                    tableCode?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(
                    branchCode))
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message =
                            "Branch Code is required."
                    });
                }

                if (string.IsNullOrWhiteSpace(
                    tableCode))
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message =
                            "Table Code is required."
                    });
                }

                string mainQuery = $@"
EXEC SP_GET_ACTIVE_DINEIN_KOT_BY_TABLE
    @BranchCode = '{EscapeSql(branchCode)}',
    @TableCode = '{EscapeSql(tableCode)}'";

                DataTable mainTable =
                    SQLService.GetDataTable(mainQuery);

                if (mainTable.Rows.Count == 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message =
                            "No active Dine-In KOT found for this table."
                    });
                }

                long kotId =
                    Convert.ToInt64(
                        mainTable.Rows[0]["KOT_Id"]
                    );

                string detailQuery = $@"
EXEC SP_GET_KOT_DETAILS_BY_ID
    @KOT_Id = {kotId}";

                DataTable detailTable =
                    SQLService.GetDataTable(detailQuery);

                var mainArray =
                    UtilityService.DataTableToJArray(
                        mainTable
                    );

                var detailArray =
                    UtilityService.DataTableToJArray(
                        detailTable
                    );

                return new JsonResult(new
                {
                    status = true,
                    message =
                        "Active Dine-In KOT loaded successfully.",

                    data = new
                    {
                        main =
                            mainArray.FirstOrDefault(),

                        details =
                            detailArray
                    }
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
        [HttpGet]
        [Route("GetActiveTakeawayKOTList")]
        public IActionResult GetActiveTakeawayKOTList(
         string branchCode)
        {
            try
            {
                branchCode =
                    branchCode?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(
                    branchCode))
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message =
                            "Branch Code is required."
                    });
                }

                string query = $@"
EXEC SP_GET_ACTIVE_TAKEAWAY_KOT_LIST
    @BranchCode =
        '{EscapeSql(branchCode)}'";

                DataTable dt =
                    SQLService.GetDataTable(query);

                return new JsonResult(new
                {
                    status = true,

                    message =
                        dt.Rows.Count > 0
                            ? "Active Takeaway KOT list loaded successfully."
                            : "No active Takeaway KOT found.",

                    data =
                        UtilityService.DataTableToJArray(
                            dt
                        )
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
        // ============================================================
        // CHECKOUT
        // Billing is not created here.
        // Only changes KOT status to CHECKOUT.
        // ============================================================

        [HttpPost]
                [Route("CheckoutKOT")]
                public IActionResult CheckoutKOT(
                    [FromBody] KOT_STATUS_REQUEST request)
                {
                    try
                    {
                        JObject response = new JObject();

                        if (request.KOT_Id <= 0 &&
                            string.IsNullOrWhiteSpace(request.KOT_No))
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "KOT Id or KOT Number is required."
                            );

                            return Ok(response.ToString());
                        }

                        if (string.IsNullOrWhiteSpace(request.Updated_By))
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Updated By cannot be empty."
                            );

                            return Ok(response.ToString());
                        }

                        string where = request.KOT_Id > 0
                            ? $"KOT_Id = {request.KOT_Id}"
                            : $"KOT_No = '{EscapeSql(request.KOT_No)}'";

                        string query = $@"
        UPDATE M_KOT_Main
        SET
            Order_Status = 'CHECKOUT',
            Updated_By = '{EscapeSql(request.Updated_By)}',
            Updated_On = GETDATE()
        WHERE {where}
        AND Order_Status = 'KOT'
        AND Is_Active = 'A'";

                        int result =
                            SQLService.ExecuteNonQuery(query);

                        if (result > 0)
                        {
                            response.Add("status", true);
                            response.Add(
                                "message",
                                "KOT Checkout Successfully"
                            );
                        }
                        else
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "KOT not found or already checked out."
                            );
                        }

                        return Ok(response.ToString());
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

                // ============================================================
                // REOPEN CHECKOUT
                // Used when user returns from checkout to KOT.
                // ============================================================

                [HttpPost]
                [Route("ReopenKOT")]
                public IActionResult ReopenKOT(
                    [FromBody] KOT_STATUS_REQUEST request)
                {
                    try
                    {
                        JObject response = new JObject();

                        if (request.KOT_Id <= 0)
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Valid KOT Id is required."
                            );

                            return Ok(response.ToString());
                        }

                        if (string.IsNullOrWhiteSpace(request.Updated_By))
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Updated By cannot be empty."
                            );

                            return Ok(response.ToString());
                        }

                        string query = $@"
        UPDATE M_KOT_Main
        SET
            Order_Status = 'KOT',
            Updated_By = '{EscapeSql(request.Updated_By)}',
            Updated_On = GETDATE()
        WHERE KOT_Id = {request.KOT_Id}
        AND Order_Status = 'CHECKOUT'
        AND Is_Active = 'A'";

                        int result =
                            SQLService.ExecuteNonQuery(query);

                        if (result > 0)
                        {
                            response.Add("status", true);
                            response.Add(
                                "message",
                                "KOT Reopened Successfully"
                            );
                        }
                        else
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Checkout KOT not found."
                            );
                        }

                        return Ok(response.ToString());
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

                // ============================================================
                // MARK AS BILLED
                // Call only after sales/billing save succeeds.
                // ============================================================

                [HttpPost]
                [Route("CompleteKOT")]
                public IActionResult CompleteKOT(
                    [FromBody] KOT_STATUS_REQUEST request)
                {
                    try
                    {
                        JObject response = new JObject();

                        if (request.KOT_Id <= 0)
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Valid KOT Id is required."
                            );

                            return Ok(response.ToString());
                        }

                        if (string.IsNullOrWhiteSpace(request.Updated_By))
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Updated By cannot be empty."
                            );

                            return Ok(response.ToString());
                        }

                        string query = $@"
        UPDATE M_KOT_Main
        SET
            Order_Status = 'BILLED',
            Updated_By = '{EscapeSql(request.Updated_By)}',
            Updated_On = GETDATE()
        WHERE KOT_Id = {request.KOT_Id}
        AND Order_Status IN ('KOT', 'CHECKOUT')
        AND Is_Active = 'A'";

                        int result =
                            SQLService.ExecuteNonQuery(query);

                        if (result > 0)
                        {
                            response.Add("status", true);
                            response.Add(
                                "message",
                                "KOT Completed Successfully"
                            );
                        }
                        else
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "KOT not found or already completed."
                            );
                        }

                        return Ok(response.ToString());
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

                // ============================================================
                // UPDATE INDIVIDUAL ITEM STATUS
                // ============================================================

                [HttpPost]
                [Route("UpdateItemStatus")]
                public IActionResult UpdateItemStatus(
                    [FromBody] KOT_ITEM_STATUS_REQUEST request)
                {
                    try
                    {
                        JObject response = new JObject();

                        if (request.KOT_Detail_Id <= 0)
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Valid KOT Detail Id is required."
                            );

                            return Ok(response.ToString());
                        }

                        string[] validStatuses =
                        {
                            "KOT",
                            "PREPARING",
                            "READY",
                            "SERVED",
                            "CANCELLED"
                        };

                        string itemStatus =
                            request.Item_Status?.Trim().ToUpper() ?? "";

                        if (!validStatuses.Contains(itemStatus))
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Invalid item status."
                            );

                            return Ok(response.ToString());
                        }

                        if (string.IsNullOrWhiteSpace(request.Updated_By))
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Updated By cannot be empty."
                            );

                            return Ok(response.ToString());
                        }

                        string query = $@"
        UPDATE M_KOT_Details
        SET
            Item_Status = '{EscapeSql(itemStatus)}',
            Updated_By = '{EscapeSql(request.Updated_By)}',
            Updated_On = GETDATE()
        WHERE KOT_Detail_Id = {request.KOT_Detail_Id}
        AND Is_Active = 'A'";

                        int result =
                            SQLService.ExecuteNonQuery(query);

                        if (result > 0)
                        {
                            response.Add("status", true);
                            response.Add(
                                "message",
                                "KOT Item Status Updated Successfully"
                            );
                        }
                        else
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "KOT item not found."
                            );
                        }

                        return Ok(response.ToString());
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

                // ============================================================
                // CANCEL ENTIRE KOT
                // ============================================================

                [HttpPost]
                [Route("CancelKOT")]
                public IActionResult CancelKOT(
                    [FromBody] KOT_STATUS_REQUEST request)
                {
                    try
                    {
                        JObject response = new JObject();

                        if (request.KOT_Id <= 0)
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Valid KOT Id is required."
                            );

                            return Ok(response.ToString());
                        }

                        if (string.IsNullOrWhiteSpace(request.Updated_By))
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Updated By cannot be empty."
                            );

                            return Ok(response.ToString());
                        }

                        string updatedBy =
                            EscapeSql(request.Updated_By);

                        string query = $@"
        BEGIN TRY
            BEGIN TRANSACTION;

            UPDATE M_KOT_Main
            SET
                Order_Status = 'CANCELLED',
                Is_Active = 'D',
                Updated_By = '{updatedBy}',
                Updated_On = GETDATE()
            WHERE KOT_Id = {request.KOT_Id}
            AND Order_Status <> 'BILLED'
            AND Is_Active = 'A';

            IF @@ROWCOUNT = 0
            BEGIN
                THROW 50001,
                'KOT not found, already cancelled, or already billed.',
                1;
            END;

            UPDATE M_KOT_Details
            SET
                Item_Status = 'CANCELLED',
                Is_Active = 'D',
                Updated_By = '{updatedBy}',
                Updated_On = GETDATE()
            WHERE KOT_Id = {request.KOT_Id}
            AND Is_Active = 'A';

            COMMIT TRANSACTION;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            THROW;
        END CATCH";

                        int result =
                            SQLService.ExecuteNonQuery(query);

                        if (result > 0)
                        {
                            response.Add("status", true);
                            response.Add(
                                "message",
                                "KOT Cancelled Successfully"
                            );
                        }
                        else
                        {
                            response.Add("status", false);
                            response.Add(
                                "message",
                                "Cannot cancel KOT."
                            );
                        }

                        return Ok(response.ToString());
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

        // ============================================================
        // PRIVATE: BUILD FIRST KOT QUERY
        // ============================================================

        private string BuildNewKOTTransactionQuery(
KOT_SAVE_REQUEST request,
decimal totalQty,
decimal totalAmount)
        {
            string branchCode =
                EscapeSql(request.Branch_Code);

            string tableCode =
                SqlNullable(request.Table_Code);

            string customerCode =
                SqlNullable(request.Customer_Code);

            string customerName =
                SqlNullable(
                    request.Customer_Name?.ToUpper()
                );

            string mobileNo =
                SqlNullable(request.Mobile_No);

            string orderType =
                EscapeSql(request.Order_Type);

            string remarks =
                SqlNullable(request.Remarks);

            string createdBy =
                EscapeSql(request.Created_By);

            int guestCount =
                request.Order_Type == "DINE_IN"
                    ? request.Guest_Count
                    : 0;

            bool isDineIn =
                request.Order_Type == "DINE_IN";

            string activeTableValidation =
                string.Empty;

            if (isDineIn)
            {
                activeTableValidation = $@"
        DECLARE @Total_Seats INT;
        DECLARE @Occupied_Seats INT;
        DECLARE @Available_Seats INT;

        SELECT
            @Total_Seats = Chair
        FROM floor_table_master WITH (UPDLOCK, HOLDLOCK)
        WHERE Branch_Code = '{branchCode}'
        AND Table_Code = {tableCode}
        AND Is_Active = 'A';

        IF @Total_Seats IS NULL
        BEGIN
            THROW 50001,
            'Selected table was not found or is inactive.',
            1;
        END;

        IF ISNULL(@Total_Seats, 0) <= 0
        BEGIN
            THROW 50002,
            'Chair count is not configured for the selected table.',
            1;
        END;

        SELECT
            @Occupied_Seats =
                ISNULL(
                    SUM(ISNULL(Guest_Count, 0)),
                    0
                )
        FROM M_KOT_Main WITH (UPDLOCK, HOLDLOCK)
        WHERE Branch_Code = '{branchCode}'
        AND Table_Code = {tableCode}
        AND Order_Type = 'DINE_IN'
        AND Order_Status IN ('KOT', 'CHECKOUT')
        AND Is_Active = 'A';

        SET @Available_Seats =
            @Total_Seats -
            ISNULL(@Occupied_Seats, 0);

        IF @Available_Seats <= 0
        BEGIN
            THROW 50003,
            'All seats are occupied for the selected table.',
            1;
        END;

        IF {guestCount} > @Available_Seats
        BEGIN
            DECLARE @SeatErrorMessage NVARCHAR(300);

            SET @SeatErrorMessage =
                'Only ' +
                CAST(@Available_Seats AS VARCHAR(10)) +
                ' seat(s) are available for this table. ' +
                'Requested guest count: ' +
                CAST({guestCount} AS VARCHAR(10)) +
                '.';

            THROW 50004,
                @SeatErrorMessage,
                1;
        END;";
            }

            StringBuilder sql =
                new StringBuilder();

            sql.AppendLine($@"
SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @KOT_Id BIGINT;
    DECLARE @KOT_No VARCHAR(50);
    DECLARE @KOT_Sequence INT;

    DECLARE @Sub_KOT_No VARCHAR(20) = 'SUB_01';
    DECLARE @Sub_KOT_Sequence INT = 1;

    DECLARE @Today DATE =
        CAST(GETDATE() AS DATE);

    {activeTableValidation}

    SELECT
        @KOT_Sequence =
            ISNULL
            (
                MAX
                (
                    TRY_CAST(
                        RIGHT(KOT_No, 4)
                        AS INT
                    )
                ),
                0
            ) + 1
    FROM M_KOT_Main WITH (UPDLOCK, HOLDLOCK)
    WHERE Branch_Code = '{branchCode}'
    AND CAST(KOT_Date AS DATE) = @Today;

    SET @KOT_No =
        'KOT_' +
        '{branchCode}' + '_' +
        CONVERT(VARCHAR(8), @Today, 112) + '_' +
        RIGHT
        (
            '0000' +
            CAST(@KOT_Sequence AS VARCHAR(10)),
            4
        );

    INSERT INTO M_KOT_Main
    (
        KOT_No,
        KOT_Date,
        Branch_Code,
        Table_Code,
        Guest_Count,
        Customer_Code,
        Customer_Name,
        Mobile_No,
        Order_Type,
        Order_Status,
        Total_Qty,
        Total_Amount,
        Remarks,
        Direct_Bill,
        Is_Active,
        Created_By,
        Created_On,
        Updated_By,
        Updated_On
    )
    VALUES
    (
        @KOT_No,
        GETDATE(),
        '{branchCode}',
        {tableCode},
        {guestCount},
        {customerCode},
        {customerName},
        {mobileNo},
        '{orderType}',
        'KOT',
        {SqlDecimal(totalQty)},
        {SqlDecimal(totalAmount)},
        {remarks},
        '{EscapeSql(request.Direct_Bill ?? "N")}',
        'A',
        '{createdBy}',
        GETDATE(),
        NULL,
        NULL
    );

    SET @KOT_Id =
        SCOPE_IDENTITY();
");

            foreach (KOT_DETAIL_REQUEST item in request.Items)
            {
                sql.AppendLine(
                    BuildDetailInsertQuery(
                        kotIdExpression: "@KOT_Id",
                        kotNoExpression: "@KOT_No",
                        subKOTNoExpression: "@Sub_KOT_No",
                        subSequenceExpression:
                            "@Sub_KOT_Sequence",
                        item: item,
                        userCode: request.Created_By
                    )
                );
            }

            sql.AppendLine(@"
    COMMIT TRANSACTION;

    SELECT
        @KOT_Id AS KOT_Id,
        @KOT_No AS KOT_No,
        @Sub_KOT_No AS Sub_KOT_No,
        @Sub_KOT_Sequence AS Sub_KOT_Sequence;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;");

            return sql.ToString();
        }

        // ============================================================
        // PRIVATE: BUILD REORDER QUERY
        // ============================================================

        private string BuildReorderTransactionQuery(
                    KOT_REORDER_REQUEST request,
                    decimal reorderQty,
                    decimal reorderAmount)
                {
                    string kotWhere = request.KOT_Id > 0
                        ? $"KOT_Id = {request.KOT_Id}"
                        : $"KOT_No = '{EscapeSql(request.KOT_No)}'";

                    string updatedBy =
                        EscapeSql(request.Updated_By);

                    StringBuilder sql = new StringBuilder();

                    sql.AppendLine($@"
        SET NOCOUNT ON;

        BEGIN TRY
            BEGIN TRANSACTION;

            DECLARE @KOT_Id BIGINT;
            DECLARE @KOT_No VARCHAR(50);
            DECLARE @Sub_KOT_Sequence INT;
            DECLARE @Sub_KOT_No VARCHAR(20);

            SELECT TOP 1
                @KOT_Id = KOT_Id,
                @KOT_No = KOT_No
            FROM M_KOT_Main WITH (UPDLOCK, HOLDLOCK)
            WHERE {kotWhere}
            AND Order_Status = 'KOT'
            AND Is_Active = 'A';

            IF @KOT_Id IS NULL
            BEGIN
                THROW 50001,
                'Active KOT not found or KOT already checked out.',
                1;
            END;

            SELECT
                @Sub_KOT_Sequence =
                    ISNULL(MAX(Sub_KOT_Sequence), 0) + 1
            FROM M_KOT_Details WITH (UPDLOCK, HOLDLOCK)
            WHERE KOT_Id = @KOT_Id;

            SET @Sub_KOT_No =
                'SUB_' +
                RIGHT(
                    '00' +
                    CAST(@Sub_KOT_Sequence AS VARCHAR(10)),
                    2
                );
        ");

                    foreach (KOT_DETAIL_REQUEST item in request.Items)
                    {
                        sql.AppendLine(BuildDetailInsertQuery(
                            kotIdExpression: "@KOT_Id",
                            kotNoExpression: "@KOT_No",
                            subKOTNoExpression: "@Sub_KOT_No",
                            subSequenceExpression: "@Sub_KOT_Sequence",
                            item: item,
                            userCode: request.Updated_By
                        ));
                    }

                    sql.AppendLine($@"
            UPDATE M_KOT_Main
            SET
                Total_Qty =
                    ISNULL(Total_Qty, 0) +
                    {SqlDecimal(reorderQty)},

                Total_Amount =
                    ISNULL(Total_Amount, 0) +
                    {SqlDecimal(reorderAmount)},

                Updated_By = '{updatedBy}',
                Updated_On = GETDATE()

            WHERE KOT_Id = @KOT_Id;

            COMMIT TRANSACTION;

            SELECT
                @KOT_Id AS KOT_Id,
                @KOT_No AS KOT_No,
                @Sub_KOT_No AS Sub_KOT_No,
                @Sub_KOT_Sequence AS Sub_KOT_Sequence;
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            THROW;
        END CATCH;");

                    return sql.ToString();
                }

                // ============================================================
                // PRIVATE: BUILD DETAIL INSERT
                // ============================================================

                private string BuildDetailInsertQuery(
              string kotIdExpression,
              string kotNoExpression,
              string subKOTNoExpression,
              string subSequenceExpression,
              KOT_DETAIL_REQUEST item,
              string? userCode)
                {
                    decimal amount =
                        item.Amount > 0
                            ? item.Amount
                            : item.Qty * item.Rate;

                    return $@"
            INSERT INTO M_KOT_Details
            (
                KOT_Id,
                KOT_No,

                Sub_KOT_No,
                Sub_KOT_Sequence,

                Product_Code,
                Product_Name,

                Category_Code,

                Variant_Code,
                Variant_Name,

                Kitchen_Code,

                Cooking_Code,
                Cooking_Notes,

                Qty,
                Rate,
                Amount,

                Item_Status,
                Is_Active,

                Created_By,
                Created_On,

                Updated_By,
                Updated_On
            )
            VALUES
            (
                {kotIdExpression},
                {kotNoExpression},

                {subKOTNoExpression},
                {subSequenceExpression},

                '{EscapeSql(item.Product_Code)}',
                '{EscapeSql(
                            item.Product_Name?.ToUpper()
                        )}',

                {SqlNullable(item.Category_Code)},

                {SqlNullable(item.Variant_Code)},
                {SqlNullable(
                            item.Variant_Name?.ToUpper()
                        )},

                {SqlNullable(item.Kitchen_Code)},

                {SqlNullable(item.Cooking_Code)},
                {SqlNullable(item.Cooking_Notes)},

                {SqlDecimal(item.Qty)},
                {SqlDecimal(item.Rate)},
                {SqlDecimal(amount)},

                'KOT',
                'A',

                '{EscapeSql(userCode)}',
                GETDATE(),

                NULL,
                NULL
            );";
                }

            // ============================================================
            // VALIDATIONS
            // ============================================================

            private string ValidateNewKOT(
     KOT_SAVE_REQUEST request)
            {
                if (request == null)
                {
                    return "KOT request cannot be empty.";
                }

                if (string.IsNullOrWhiteSpace(
                    request.Branch_Code))
                {
                    return "Branch Code cannot be empty.";
                }

                if (string.IsNullOrWhiteSpace(
                    request.Order_Type))
                {
                    return "Order Type cannot be empty.";
                }

                string orderType =
                    request.Order_Type
                        .Trim()
                        .ToUpper();

                if (orderType != "DINE_IN" &&
                    orderType != "TAKEAWAY")
                {
                    return
                        "Order Type must be DINE_IN or TAKEAWAY.";
                }

                /*
                 * DINE-IN
                 *
                 * Mandatory:
                 * - Branch Code
                 * - Table Code
                 * - Guest Count
                 * - Items
                 * - Created By
                 *
                 * Optional:
                 * - Customer Code
                 * - Customer Name
                 * - Mobile Number
                 */
                if (orderType == "DINE_IN")
                {
                    if (string.IsNullOrWhiteSpace(
                        request.Table_Code))
                    {
                        return
                            "Table Code is required for Dine-In orders.";
                    }

                    if (request.Guest_Count <= 0)
                    {
                        return
                            "Guest Count must be greater than zero for Dine-In orders.";
                    }
                }

                /*
                 * TAKEAWAY
                 *
                 * Table and guest details are not applicable.
                 * Customer details remain optional.
                 */
                if (orderType == "TAKEAWAY")
                {
                    request.Table_Code = null;
                    request.Guest_Count = 0;
                }

                /*
                 * Customer details are optional for both order types.
                 *
                 * Empty values are converted into NULL before insert.
                 */
                if (string.IsNullOrWhiteSpace(
                    request.Customer_Code))
                {
                    request.Customer_Code = null;
                }

                if (string.IsNullOrWhiteSpace(
                    request.Customer_Name))
                {
                    request.Customer_Name = null;
                }

                if (string.IsNullOrWhiteSpace(
                    request.Mobile_No))
                {
                    request.Mobile_No = null;
                }

                if (string.IsNullOrWhiteSpace(
                    request.Created_By))
                {
                    return "Created By cannot be empty.";
                }

                if (request.Items == null ||
                    request.Items.Count == 0)
                {
                    return
                        "At least one product is required.";
                }

                return ValidateKOTItems(
                    request.Items
                );
            }
            private string ValidateKOTItems(
            List<KOT_DETAIL_REQUEST> items)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        KOT_DETAIL_REQUEST item =
                            items[i];

                        int rowNumber = i + 1;

                        if (string.IsNullOrWhiteSpace(
                            item.Product_Code))
                        {
                            return
                                $"Product Code is required at item {rowNumber}.";
                        }

                        if (string.IsNullOrWhiteSpace(
                            item.Product_Name))
                        {
                            return
                                $"Product Name is required at item {rowNumber}.";
                        }

                        if (item.Qty <= 0)
                        {
                            return
                                $"Quantity must be greater than zero at item {rowNumber}.";
                        }

                        if (item.Rate < 0)
                        {
                            return
                                $"Rate cannot be negative at item {rowNumber}.";
                        }

                        item.Product_Code =
                            item.Product_Code.Trim();

                        item.Product_Name =
                            item.Product_Name.Trim();

                        item.Category_Code =
                            item.Category_Code?.Trim();

                        item.Variant_Code =
                            item.Variant_Code?.Trim();

                        item.Variant_Name =
                            item.Variant_Name?.Trim();

                        item.Kitchen_Code =
                            item.Kitchen_Code?.Trim();

                        item.Cooking_Code =
                            item.Cooking_Code?.Trim();

                        item.Cooking_Notes =
                            item.Cooking_Notes?.Trim();

                        // Always calculate amount on the server.
                        item.Amount =
                            item.Qty * item.Rate;
                    }

                    return string.Empty;
                }
                // ============================================================
                // SQL VALUE HELPERS
                // ============================================================

                private static string EscapeSql(string? value)
                {
                    return (value ?? string.Empty)
                        .Trim()
                        .Replace("'", "''");
                }

                private static string SqlNullable(string? value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        return "NULL";
                    }

                    return $"'{EscapeSql(value)}'";
                }

                private static string SqlDecimal(decimal value)
                {
                    return value.ToString(
                        "0.###",
                        CultureInfo.InvariantCulture
                    );
                }
        [HttpGet]
        [Route("CheckTableAvailability")]
        public IActionResult CheckTableAvailability(string branchCode)
        {
            try
            {
                branchCode = branchCode?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(branchCode))
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Branch Code is required."
                    });
                }

                // Stored Procedure-ai parameterized parameter format-il execute seigirom
                string query = $"EXEC SP_CHECK_TABLE_AVAILABILITY '{EscapeSql(branchCode)}'";

                // Safe DataTable retrieval using SQLService helper
                DataTable dtAll = SQLService.GetDataTable(query);
                var tablesList = new List<object>();

                if (dtAll != null && dtAll.Rows.Count > 0)
                {
                    foreach (DataRow r in dtAll.Rows)
                    {
                        bool tableExists = r["Table_Exists"] != DBNull.Value && Convert.ToBoolean(r["Table_Exists"]);
                        bool isAvailable = r["Is_Available"] != DBNull.Value && Convert.ToBoolean(r["Is_Available"]);
                        bool isOccupied = r["Is_Occupied"] != DBNull.Value && Convert.ToBoolean(r["Is_Occupied"]);

                        tablesList.Add(new
                        {
                            table_exists = tableExists,
                            is_available = isAvailable,
                            is_occupied = isOccupied,
                            branch_code = r["Branch_Code"]?.ToString(),
                            table_code = r["Table_Code"]?.ToString(),
                            floor_name = r["Floor_Name"]?.ToString(),
                            table_name = r["Table_Name"]?.ToString(),
                            chair = r["Chair"] == DBNull.Value ? 0 : Convert.ToInt32(r["Chair"]),
                            occupied_seats = r["Occupied_Seats"] == DBNull.Value ? 0 : Convert.ToInt32(r["Occupied_Seats"]),
                            available_seats = r["Available_Seats"] == DBNull.Value ? 0 : Convert.ToInt32(r["Available_Seats"]),
                            active_kot_count = r["Active_KOT_Count"] == DBNull.Value ? 0 : Convert.ToInt32(r["Active_KOT_Count"])
                        });
                    }
                }

                return new JsonResult(new
                {
                    status = true,
                    message = "Tables list retrieved successfully.",
                    data = tablesList
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }



        // ============================================================
        // SAVE BILL
        // Calls existing stored procedure:
        // dbo.SP_SAVE_BILL_FROM_KOT
        // ============================================================

        [HttpPost]
        [Route("SaveBill")]
        public IActionResult SaveBill(
            [FromBody] BILL_SAVE_REQUEST request)
        {
            try
            {
                ApiResponse apiResponse =
                    new ApiResponse();

                if (request == null)
                {
                    apiResponse.status = false;
                    apiResponse.message =
                        "Bill request cannot be empty.";

                    return new JsonResult(apiResponse);
                }

                if (request.KOT_Id <= 0)
                {
                    apiResponse.status = false;
                    apiResponse.message =
                        "Valid KOT Id is required.";

                    return new JsonResult(apiResponse);
                }

                if (string.IsNullOrWhiteSpace(
                    request.Created_By))
                {
                    apiResponse.status = false;
                    apiResponse.message =
                        "Created By is required.";

                    return new JsonResult(apiResponse);
                }

                request.Payment_Mode =
                    string.IsNullOrWhiteSpace(
                        request.Payment_Mode)
                        ? null
                        : request.Payment_Mode
                            .Trim()
                            .ToUpper();

                request.Payment_Reference =
                    string.IsNullOrWhiteSpace(
                        request.Payment_Reference)
                        ? null
                        : request.Payment_Reference.Trim();

                request.Remarks =
                    string.IsNullOrWhiteSpace(
                        request.Remarks)
                        ? null
                        : request.Remarks.Trim();

                request.Created_By =
                    request.Created_By.Trim();

                string query = $@"
EXEC dbo.SP_SAVE_BILL_FROM_KOT
    @KOT_Id = {request.KOT_Id},

    @Discount_Percentage =
        {SqlDecimal(
                    request.Discount_Percentage
                )},

    @Discount_Amount =
        {SqlDecimal(
                    request.Discount_Amount
                )},

    @Tax_Amount =
        {SqlDecimal(
                    request.Tax_Amount
                )},

    @Round_Off =
        {SqlDecimal(
                    request.Round_Off
                )},

    @Paid_Amount =
        {SqlDecimal(
                    request.Paid_Amount
                )},

    @Payment_Mode =
        {SqlNullable(
                    request.Payment_Mode
                )},

    @Payment_Reference =
        {SqlNullable(
                    request.Payment_Reference
                )},

    @Remarks =
        {SqlNullable(
                    request.Remarks
                )},

    @Created_By =
        '{EscapeSql(
                    request.Created_By
                )}'";

                DataTable dt =
                    SQLService.GetDataTable(query);

                if (dt == null ||
                    dt.Rows.Count == 0)
                {
                    apiResponse.status = false;
                    apiResponse.message =
                        "Unable to save bill.";

                    return new JsonResult(apiResponse);
                }

                apiResponse.status = true;
                apiResponse.message =
                    "Bill saved successfully.";

                apiResponse.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(apiResponse);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetTableWiseKOTOrders")]
        public IActionResult GetTableWiseKOTOrders(
            string branchCode,
            string? orderStatus = null,
            string? kotDate = null)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                // ========================================================
                // NORMALIZE
                // ========================================================

                branchCode =
                    branchCode?.Trim() ?? string.Empty;

                orderStatus =
                    string.IsNullOrWhiteSpace(orderStatus)
                        ? null
                        : orderStatus.Trim().ToUpper();

                kotDate =
                    string.IsNullOrWhiteSpace(kotDate)
                        ? null
                        : kotDate.Trim();

                // ========================================================
                // VALIDATE BRANCH
                // ========================================================

                if (string.IsNullOrWhiteSpace(branchCode))
                {
                    response.status = false;
                    response.message =
                        "Branch Code is required.";

                    return new JsonResult(response);
                }

                // ========================================================
                // VALIDATE ORDER STATUS
                // ========================================================

                if (!string.IsNullOrWhiteSpace(orderStatus))
                {
                    string[] validStatuses =
                    {
                "KOT",
                "CHECKOUT",
                "BILLED",
                "CANCELLED"
            };

                    if (!validStatuses.Contains(orderStatus))
                    {
                        response.status = false;
                        response.message =
                            "Invalid Order Status.";

                        return new JsonResult(response);
                    }
                }

                // ========================================================
                // VALIDATE DATE
                // ========================================================

                DateTime? parsedDate = null;

                if (!string.IsNullOrWhiteSpace(kotDate))
                {
                    if (!DateTime.TryParseExact(
                        kotDate,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime date))
                    {
                        response.status = false;
                        response.message =
                            "Invalid KOT Date. Use yyyy-MM-dd format.";

                        return new JsonResult(response);
                    }

                    parsedDate = date.Date;
                }

                // ========================================================
                // BUILD STORED PROCEDURE QUERY
                // ========================================================

                string branchSql =
                    $"'{EscapeSql(branchCode)}'";

                string statusSql =
                    SqlNullable(orderStatus);

                string dateSql =
                    parsedDate.HasValue
                        ? $"'{parsedDate.Value:yyyy-MM-dd}'"
                        : "NULL";

                string query = $@"
EXEC SP_GET_TABLE_WISE_KOT_ORDERS

    @BranchCode =
        {branchSql},

    @OrderStatus =
        {statusSql},

    @KOTDate =
        {dateSql}";

                // ========================================================
                // EXECUTE
                // ========================================================

                DataTable dt =
                    SQLService.GetDataTable(query);

                // ========================================================
                // RESPONSE
                // ========================================================

                response.status = true;

                response.message =
                    dt != null && dt.Rows.Count > 0
                        ? "Table wise KOT orders loaded successfully."
                        : "No KOT orders found.";

                response.data =
                    UtilityService.DataTableToJArray(dt);

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("CancelKOTItem")]
        public IActionResult CancelKOTItem([FromBody] KOT_ITEM_CANCEL_REQUEST request)
        {
            try
            {
                ApiResponse response = new ApiResponse();

                // ========================================================
                // VALIDATION
                // ========================================================

                if (request == null)
                {
                    response.status = false;
                    response.message = "Cancel KOT item request cannot be empty.";
                    return new JsonResult(response);
                }

                if (request.KOT_Id <= 0)
                {
                    response.status = false;
                    response.message = "Valid KOT Id is required.";
                    return new JsonResult(response);
                }

                if (request.KOT_Detail_Id <= 0)
                {
                    response.status = false;
                    response.message = "Valid KOT Detail Id is required.";
                    return new JsonResult(response);
                }

                if (string.IsNullOrWhiteSpace(request.Updated_By))
                {
                    response.status = false;
                    response.message = "Updated By cannot be empty.";
                    return new JsonResult(response);
                }

                // ========================================================
                // NORMALIZE
                // ========================================================

                request.Updated_By = request.Updated_By.Trim();

                // ========================================================
                // STORED PROCEDURE
                // ========================================================

                string query = $@"
EXEC dbo.SP_CANCEL_KOT_ITEM
    @KOT_Id = {request.KOT_Id},
    @KOT_Detail_Id = {request.KOT_Detail_Id},
    @Updated_By = '{EscapeSql(request.Updated_By)}'";

                // ========================================================
                // EXECUTE
                // ========================================================

                DataTable dt = SQLService.GetDataTable(query);

                // ========================================================
                // NO RESULT
                // ========================================================

                if (dt == null || dt.Rows.Count == 0)
                {
                    response.status = false;
                    response.message = "Unable to cancel KOT item.";
                    return new JsonResult(response);
                }

                DataRow row = dt.Rows[0];

                // ========================================================
                // SUCCESS
                // ========================================================

                bool status = row["Status"] != DBNull.Value && Convert.ToBoolean(row["Status"]);
                string message = row["Message"]?.ToString() ?? "Unable to cancel KOT item.";

                if (!status)
                {
                    response.status = false;
                    response.message = message;
                    return new JsonResult(response);
                }

                // ========================================================
                // BUILD RESPONSE
                // ========================================================

                response.status = true;
                response.message = message;
                response.data = new
                {
                    kot_id = row["KOT_Id"] == DBNull.Value ? 0 : Convert.ToInt64(row["KOT_Id"]),
                    kot_no = row["KOT_No"]?.ToString(),
                    kot_detail_id = row["KOT_Detail_Id"] == DBNull.Value ? 0 : Convert.ToInt64(row["KOT_Detail_Id"]),
                    product_name = row["Product_Name"]?.ToString(),
                    cancelled_qty = row["Cancelled_Qty"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Cancelled_Qty"]),
                    cancelled_amount = row["Cancelled_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Cancelled_Amount"]),
                    total_qty = row["Total_Qty"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Total_Qty"]),
                    total_amount = row["Total_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Total_Amount"]),
                    order_status = row["Order_Status"]?.ToString()
                };

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
        public class KOT_ITEM_CANCEL_REQUEST
        {
            public long KOT_Id { get; set; }
            public long KOT_Detail_Id { get; set; }
            public string? Updated_By { get; set; }
        }

        [HttpGet]
        [Route("GetBillByKotId/{kotId}")]
        public IActionResult GetBillByKotId(long kotId)
        {
            try
            {
                if (kotId <= 0)
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Valid KOT ID is required."
                    });
                }

                // Query saved bill from Bill_Main for the given KOT_Id
                string query = $@"SELECT * FROM Bill_Main WHERE KOT_Id = {kotId} AND Is_Active = 'A'";
                DataTable dt = SQLService.GetDataTable(query);

                return new JsonResult(new
                {
                    status = true,
                    data = UtilityService.DataTableToJArray(dt)
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }


        [HttpGet]
        [Route("GetNextDirectBillToken")]
        public IActionResult GetNextDirectBillToken(string branchCode, long? kotId = null)
        {
            try
            {
                branchCode = branchCode?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(branchCode))
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Branch Code is required."
                    });
                }

                string query = $"EXEC SP_GET_NEXT_DIRECT_BILL_TOKEN @BranchCode = '{EscapeSql(branchCode)}'" +
                               (kotId.HasValue && kotId.Value > 0 ? $", @KOT_Id = {kotId.Value}" : "");

                DataTable dt = SQLService.GetDataTable(query);

                int nextToken = 1;
                if (dt != null && dt.Rows.Count > 0)
                {
                    nextToken = Convert.ToInt32(dt.Rows[0]["NextTokenNumber"]);
                }

                return new JsonResult(new
                {
                    status = true,
                    token_no = nextToken
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet]
        [Route("GetDirectBillSettings")]
        public IActionResult GetDirectBillSettings(string branchCode)
        {
            try
            {
                branchCode = branchCode?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(branchCode))
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Branch Code is required."
                    });
                }

                string query = $@"EXEC sp_GetBranchSettings @BranchCode = '{EscapeSql(branchCode)}', @Category = 'PAYMENT'";
                DataTable dt = SQLService.GetDataTable(query);

                var settings = new Dictionary<string, string>();
                if (dt != null)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string key = row["Setting_Key"]?.ToString() ?? string.Empty;
                        string val = row["Setting_Value"]?.ToString() ?? string.Empty;
                        if (!string.IsNullOrEmpty(key))
                        {
                            settings[key] = val;
                        }
                    }
                }

                return new JsonResult(new
                {
                    status = true,
                    individual_token = settings.ContainsKey("Individual_Token") ? settings["Individual_Token"] : "false",
                    token_include_bill = settings.ContainsKey("Token_Include_Bill") ? settings["Token_Include_Bill"] : "false",
                    directbill_header_text = settings.ContainsKey("DirectBill_Header_Text") ? settings["DirectBill_Header_Text"] : string.Empty,
                    directbill_footer_text = settings.ContainsKey("DirectBill_Footer_Text") ? settings["DirectBill_Footer_Text"] : string.Empty
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        public class DirectBillSettingsRequest
        {
            public string BranchCode { get; set; }
            public bool IndividualToken { get; set; }
            public bool TokenIncludeBill { get; set; }
            public string DirectBillHeaderText { get; set; }
            public string DirectBillFooterText { get; set; }
            public string UpdatedBy { get; set; }
        }

        [HttpPost]
        [Route("SaveDirectBillSettings")]
        public IActionResult SaveDirectBillSettings([FromBody] DirectBillSettingsRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.BranchCode))
                {
                    return new JsonResult(new
                    {
                        status = false,
                        message = "Request payload and Branch Code are required."
                    });
                }

                string branch = EscapeSql(request.BranchCode.Trim());
                string updatedBy = EscapeSql(request.UpdatedBy?.Trim() ?? "Admin");
                string indToken = request.IndividualToken ? "true" : "false";
                string incBill = request.TokenIncludeBill ? "true" : "false";
                string header = EscapeSql(request.DirectBillHeaderText?.Trim() ?? string.Empty);
                string footer = EscapeSql(request.DirectBillFooterText?.Trim() ?? string.Empty);

                string query1 = $@"EXEC sp_SaveBranchSetting @BranchCode = '{branch}', @Category = 'PAYMENT', @SettingKey = 'Individual_Token', @SettingValue = '{indToken}', @DataType = 'BOOLEAN', @UpdatedBy = '{updatedBy}'";
                string query2 = $@"EXEC sp_SaveBranchSetting @BranchCode = '{branch}', @Category = 'PAYMENT', @SettingKey = 'Token_Include_Bill', @SettingValue = '{incBill}', @DataType = 'BOOLEAN', @UpdatedBy = '{updatedBy}'";
                string query3 = $@"EXEC sp_SaveBranchSetting @BranchCode = '{branch}', @Category = 'PAYMENT', @SettingKey = 'DirectBill_Header_Text', @SettingValue = '{header}', @DataType = 'STRING', @UpdatedBy = '{updatedBy}'";
                string query4 = $@"EXEC sp_SaveBranchSetting @BranchCode = '{branch}', @Category = 'PAYMENT', @SettingKey = 'DirectBill_Footer_Text', @SettingValue = '{footer}', @DataType = 'STRING', @UpdatedBy = '{updatedBy}'";

                SQLService.GetDataTable(query1);
                SQLService.GetDataTable(query2);
                SQLService.GetDataTable(query3);
                SQLService.GetDataTable(query4);

                return new JsonResult(new
                {
                    status = true,
                    message = "Direct Bill settings saved successfully."
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost]
        [Route("SaveDirectBill")]
        public IActionResult SaveDirectBill([FromBody] BILL_SAVE_REQUEST request)
        {
            try
            {
                ApiResponse apiResponse = new ApiResponse();

                if (request == null)
                {
                    apiResponse.status = false;
                    apiResponse.message = "Bill request cannot be empty.";
                    return new JsonResult(apiResponse);
                }

                if (string.IsNullOrWhiteSpace(request.Branch_Code))
                {
                    apiResponse.status = false;
                    apiResponse.message = "Branch Code is required.";
                    return new JsonResult(apiResponse);
                }

                if (request.Items == null || request.Items.Count == 0)
                {
                    apiResponse.status = false;
                    apiResponse.message = "Cart items cannot be empty.";
                    return new JsonResult(apiResponse);
                }

                string itemsJson = Newtonsoft.Json.JsonConvert.SerializeObject(request.Items).Replace("'", "''");
                string paymentMode = string.IsNullOrWhiteSpace(request.Payment_Mode) ? "CASH" : request.Payment_Mode.Trim().ToUpper();
                string createdBy = string.IsNullOrWhiteSpace(request.Created_By) ? "admin" : request.Created_By.Trim();
                string orderType = string.IsNullOrWhiteSpace(request.Order_Type) ? "TAKE_AWAY" : request.Order_Type.Trim().ToUpper();

                decimal subTotal = request.Sub_Total > 0 ? request.Sub_Total : (request.Net_Amount - request.Tax_Amount + request.Discount_Amount);
                decimal netAmount = request.Net_Amount > 0 ? request.Net_Amount : subTotal;
                decimal paidAmount = request.Paid_Amount > 0 ? request.Paid_Amount : netAmount;

                string query = $@"
EXEC dbo.SP_SAVE_DIRECT_BILL
    @Branch_Code         = '{EscapeSql(request.Branch_Code)}',
    @Order_Type          = '{EscapeSql(orderType)}',
    @Customer_Code       = {SqlNullable(request.Customer_Code)},
    @Customer_Name       = {SqlNullable(request.Customer_Name)},
    @Mobile_No           = {SqlNullable(request.Mobile_No)},
    @Sub_Total           = {SqlDecimal(subTotal)},
    @Discount_Percentage = {SqlDecimal(request.Discount_Percentage)},
    @Discount_Amount     = {SqlDecimal(request.Discount_Amount)},
    @Tax_Amount          = {SqlDecimal(request.Tax_Amount)},
    @Round_Off           = {SqlDecimal(request.Round_Off)},
    @Net_Amount          = {SqlDecimal(netAmount)},
    @Paid_Amount         = {SqlDecimal(paidAmount)},
    @Payment_Mode        = '{EscapeSql(paymentMode)}',
    @Payment_Reference   = {SqlNullable(request.Payment_Reference)},
    @Remarks             = {SqlNullable(request.Remarks)},
    @Created_By          = '{EscapeSql(createdBy)}',
    @ItemsJson           = '{itemsJson}'";

                DataTable dt = SQLService.GetDataTable(query);

                if (dt == null || dt.Rows.Count == 0)
                {
                    apiResponse.status = false;
                    apiResponse.message = "Unable to save direct bill.";
                    return new JsonResult(apiResponse);
                }

                apiResponse.status = true;
                apiResponse.message = "Bill saved successfully.";
                apiResponse.data = UtilityService.DataTableToJArray(dt);

                return new JsonResult(apiResponse);
            }
            catch (Exception ex)
            {
                return new JsonResult(new ApiResponse
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

    }


}