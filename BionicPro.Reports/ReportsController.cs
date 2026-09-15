using System.Net;
using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
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
    public class ReportsController(
        IConfiguration configuration,
         IAmazonS3 s3Client) : ControllerBase
    {
        private const string BucketName = "reports";

        private readonly string connectionString = configuration.GetConnectionString("Reports")
                ?? throw new NullReferenceException("Connection string for 'Reports' is not found in configuration.");

        [HttpGet("")]
        public async Task<IActionResult> GetSummaryReport()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized("User ID not found in claims.");
            }

            var datePath = $"year={DateTime.UtcNow:yyyy}/month={DateTime.UtcNow:MM}/day={DateTime.UtcNow:dd}";
            var s3Key = $"{datePath}/customer_{currentUserId}.json";

            try
            {
                using var getResponse = await s3Client.GetObjectAsync(BucketName, s3Key);
                using var reader = new StreamReader(getResponse.ResponseStream);
                var contentBody = await reader.ReadToEndAsync();

                return Ok(contentBody);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Файл не найден в S3 — продолжаем с базой данных
            }

            // TODO Заменить PostgreSQL на ClickHouse
            using var connection = new NpgsqlConnection(connectionString);

            var query = $@"
                SELECT 
                    customer_id AS CustomerId, 
                    total_orders AS TotalOrders, 
                    total_spent AS TotalSpent, 
                    total_discount AS TotalDiscount, 
                    avg_sensor_value AS AvgSensorValue, 
                    max_power AS MaxPower 
                FROM customer_summary_report;";

            var reportData = await connection.QueryAsync<CustomerSummaryReport>(query);
            var reportAsJson = JsonSerializer.Serialize(reportData.First());

            if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, BucketName))
            {
                await s3Client.PutBucketAsync(BucketName);
            }

            var putRequest = new PutObjectRequest
                {
                    BucketName = BucketName,
                    Key = s3Key,
                    ContentBody = reportAsJson,
                    ContentType = "application/json"
                };
            await s3Client.PutObjectAsync(putRequest);

            return Ok(reportAsJson);
        }
    }
}
