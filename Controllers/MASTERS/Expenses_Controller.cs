using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WIN_BOT;
using laptop_service.Models.MASTERS;
using System.Data;
using CommonModels;
using CommonServices;

namespace laptop_service.Controllers.MASTERS
{
    [Authorize]
    [AllowAnonymous]
    [Route("api/expense")]
    [ApiController]
    public class ExpenseController : Controller
    {
        [HttpPost]
        [Route("AddExpense")]
        public async Task<IActionResult> AddExpense(M_EXPENSE_MASTER expense)
        {
            try
            {
                JObject jobject = new JObject();

                expense.Expense_Name = expense.Expense_Name?.Trim();

                if (string.IsNullOrWhiteSpace(expense.Expense_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Expense Name cannot be empty.");
                    return Ok(jobject.ToString());
                }
                // Check Active Record
                string activeCheckQuery = $@"
SELECT COUNT(*)
FROM M_ExpenseMaster
WHERE UPPER(Expense_Name) = UPPER('{expense.Expense_Name}')
AND Is_Active = 'A'";

                int activeCount = SQLService.ExecuteScalarQuery(activeCheckQuery);

                if (activeCount > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Expense Name already exists.");
                    return Ok(jobject.ToString());
                }   

                // Remove old deleted record if exists
                string deleteInactiveQuery = $@"
DELETE FROM M_ExpenseMaster
WHERE UPPER(Expense_Name) = UPPER('{expense.Expense_Name}')
AND Is_Active = 'D'";

                SQLService.ExecuteNonQuery(deleteInactiveQuery);

                // Generate New Code
                expense = await LoadExpenseMaxCode(expense);

                var exp = new M_EXPENSE_MASTER
                {
                    Expense_Code = expense.Expense_Code,
                    Expense_Code_Int = expense.Expense_Code_Int, // IMPORTANT
                    Expense_Name = expense.Expense_Name.ToUpper(),
                    Created_By = expense.Created_By,
                    Created_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Updated_By = null,
                    Updated_On = null,
                    Is_Active = expense?.Is_Active ?? "A"
                };

                var ignoredColumns = new List<string>
                {
                 
                    "Updated_By",
                    "Updated_On"
                };

                string insertQuery = SQLHelper.BuildInsertQuery(
                    exp,
                    "M_ExpenseMaster",
                    ignoredColumns);

                int result = SQLService.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Saved Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Save!");
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

        [HttpPost]
        [Route("UpdateExpense/{Expense_Code}")]
        public async Task<IActionResult> UpdateExpense(string Expense_Code, M_EXPENSE_MASTER expense)
        {
            try
            {
                JObject jobject = new JObject();

                expense.Expense_Name = expense.Expense_Name?.Trim();

                if (string.IsNullOrWhiteSpace(expense.Expense_Name))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Expense Name cannot be empty.");
                    return Ok(jobject.ToString());
                }

                string checkQuery = $@"
                SELECT COUNT(*)
                FROM M_ExpenseMaster
                WHERE Expense_Name = '{expense.Expense_Name}'
                AND Expense_Code <> '{Expense_Code}'";

                int count = SQLService.ExecuteScalarQuery(checkQuery);

                if (count > 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Expense Name already exists.");
                    return Ok(jobject.ToString());
                }

                expense.Expense_Code = Expense_Code;

                var exp = new M_EXPENSE_MASTER
                {
                    Expense_Code = Expense_Code,
                    Expense_Name = expense.Expense_Name.ToUpper(),
                    Updated_By = expense.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Is_Active = expense.Is_Active
                };

                var ignoredColumns = new List<string>
                {
                    "Expense_Code_Int",
                    "Created_By",
                    "Created_On"
                };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    exp,
                    "M_ExpenseMaster",
                    "Expense_Code",
                    Expense_Code,
                    ignoredColumns);

                int result = SQLService.ExecuteNonQuery(updateQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Updated Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Update!");
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

        [HttpGet]
        [Route("DeleteExpense/{Expense_Code}")]
        public IActionResult DeleteExpense(string Expense_Code)
        {
            JObject jobject = new JObject();

            string query = $@"
            UPDATE M_ExpenseMaster
            SET Is_Active='D'
            WHERE Expense_Code='{Expense_Code}'";

            int result = SQLService.ExecuteNonQuery(query);

            if (result > 0)
            {
                jobject.Add("status", true);
                jobject.Add("message", "Deleted Successfully");
            }
            else
            {
                jobject.Add("status", false);
                jobject.Add("message", "Cannot Delete!");
            }

            return Ok(jobject.ToString());
        }

        [HttpGet]
        [Route("ExpenseList")]
        public IActionResult ExpenseList()
        {
            ApiResponse apiResponse = new ApiResponse();

            DataTable dt = SQLService.GetDataTable("EXEC SP_ExpenseList");

            apiResponse.status = true;
            apiResponse.data = UtilityService.DataTableToJArray(dt);

            return new JsonResult(apiResponse);
        }

        private async Task<M_EXPENSE_MASTER> LoadExpenseMaxCode(M_EXPENSE_MASTER expense)
        {
            DataTable dt = SQLService.GetDataTable(
                "SELECT ISNULL(MAX(Expense_Code_Int),0)+1 FROM M_ExpenseMaster");

            if (dt.Rows.Count > 0)
            {
                int code = Convert.ToInt32(dt.Rows[0][0]);

                expense.Expense_Code_Int = code.ToString();
                expense.Expense_Code = "EX" + code.ToString().PadLeft(5, '0');
            }

            return expense;
        }

        [HttpPost]
        [Route("SaveExpenseEntry")]
        public IActionResult SaveExpenseEntry(M_EXPENSE_ENTRY entry)
        {
            try
            {
                JObject jobject = new JObject();

                if (string.IsNullOrWhiteSpace(entry.Expense_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Expense Code is required.");
                    return Ok(jobject.ToString());
                }
                if (string.IsNullOrWhiteSpace(entry.Branch_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Branch Code is required.");
                    return Ok(jobject.ToString());
                }
                if (entry.Amount <= 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Amount must be greater than zero.");
                    return Ok(jobject.ToString());
                }

                string expenseNameQuery = $@"
        SELECT Expense_Name
        FROM M_ExpenseMaster
        WHERE Expense_Code = '{entry.Expense_Code}'
        AND Is_Active = 'A'";

                DataTable dt = SQLService.GetDataTable(expenseNameQuery);

                if (dt.Rows.Count == 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Invalid Expense Code.");
                    return Ok(jobject.ToString());
                }

                string expenseName = dt.Rows[0]["Expense_Name"].ToString();

                var expenseEntry = new M_EXPENSE_ENTRY
                {
                    Branch_Code = entry.Branch_Code,
                    Expense_Code = entry.Expense_Code,
                    Expense_Name = expenseName,
                    Amount = entry.Amount,
                    Created_By = entry.Created_By,
                    Created_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Is_Active = "A"
                };

                var ignoredColumns = new List<string>
        {
            "Id",
            "Updated_By",
            "Updated_On"
        };

                string insertQuery = SQLHelper.BuildInsertQuery(
                    expenseEntry,
                    "M_ExpenseEntry",
                    ignoredColumns);

                int result = SQLService.ExecuteNonQuery(insertQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Expense Entry Saved Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Save!");
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
        [HttpPost]
        [Route("UpdateExpenseEntry/{Id}")]
        public IActionResult UpdateExpenseEntry(int Id, M_EXPENSE_ENTRY entry)
        {
            try
            {
                JObject jobject = new JObject();

                if (string.IsNullOrWhiteSpace(entry.Branch_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Branch Code is required.");
                    return Ok(jobject.ToString());
                }

                if (string.IsNullOrWhiteSpace(entry.Expense_Code))
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Expense Code is required.");
                    return Ok(jobject.ToString());
                }

                if (entry.Amount <= 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Amount must be greater than zero.");
                    return Ok(jobject.ToString());
                }

                string expenseNameQuery = $@"
        SELECT Expense_Name
        FROM M_ExpenseMaster
        WHERE Expense_Code = '{entry.Expense_Code}'
        AND Is_Active = 'A'";

                DataTable dt = SQLService.GetDataTable(expenseNameQuery);

                if (dt.Rows.Count == 0)
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Invalid Expense Code.");
                    return Ok(jobject.ToString());
                }

                string expenseName = dt.Rows[0]["Expense_Name"].ToString();

                var expenseEntry = new M_EXPENSE_ENTRY
                {
                    Id = Id,
                    Branch_Code = entry.Branch_Code,
                    Expense_Code = entry.Expense_Code,
                    Expense_Name = expenseName,
                    Amount = entry.Amount,
                    Updated_By = entry.Updated_By,
                    Updated_On = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Is_Active = entry.Is_Active ?? "A"
                };

                var ignoredColumns = new List<string>
        {
            "Created_By",
            "Created_On"
        };

                string updateQuery = SQLHelper.BuildUpdateQuery(
                    expenseEntry,
                    "M_ExpenseEntry",
                    "Id",
                    Id.ToString(),
                    ignoredColumns);

                int result = SQLService.ExecuteNonQuery(updateQuery);

                if (result > 0)
                {
                    jobject.Add("status", true);
                    jobject.Add("message", "Expense Entry Updated Successfully");
                }
                else
                {
                    jobject.Add("status", false);
                    jobject.Add("message", "Cannot Update!");
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
        [HttpGet]
        [Route("DeleteExpenseEntry/{Id}")]
        public IActionResult DeleteExpenseEntry(int Id)
        {
            JObject jobject = new JObject();

            string query = $@"
    UPDATE M_ExpenseEntry
    SET Is_Active='D'
    WHERE Id={Id}";

            int result = SQLService.ExecuteNonQuery(query);

            if (result > 0)
            {
                jobject.Add("status", true);
                jobject.Add("message", "Expense Entry Deleted Successfully");
            }
            else
            {
                jobject.Add("status", false);
                jobject.Add("message", "Cannot Delete!");
            }

            return Ok(jobject.ToString());
        }
        [HttpGet]
        [Route("ExpenseEntryReport")]
        public IActionResult ExpenseEntryReport(string fromDate, string toDate)
        {
            try
            {
                ApiResponse apiResponse = new ApiResponse();

                string query = $@"
        EXEC SP_M_ExpenseEntryReport
        '{fromDate}',
        '{toDate}'";

                DataTable dt = SQLService.GetDataTable(query);

                apiResponse.status = true;
                apiResponse.data = UtilityService.DataTableToJArray(dt);

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
    }
}