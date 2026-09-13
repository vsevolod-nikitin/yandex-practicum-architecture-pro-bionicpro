using BionicPro.Reports.Models;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace BionicPro.Reports
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly string _connectionString;

        public ReportsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Reports")
                ?? throw new NullReferenceException("Connection string for 'Reports' is not found in configuration.");
        }

        [HttpGet("")]
        public async Task<IActionResult> GetSummaryReport()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            using var connection = new NpgsqlConnection(_connectionString);

            var query = $@"
                SELECT 
                    buyer_id AS BuyerId, 
                    total_orders AS TotalOrders, 
                    total_spent AS TotalSpent, 
                    total_discount AS TotalDiscount, 
                    avg_sensor_value AS AvgSensorValue, 
                    max_power AS MaxPower 
                FROM buyer_summary_report;";

            var reportData = await connection.QueryAsync<CustomerSummaryReport>(query);

            return Ok(reportData.FirstOrDefault());
        }
    }
}
