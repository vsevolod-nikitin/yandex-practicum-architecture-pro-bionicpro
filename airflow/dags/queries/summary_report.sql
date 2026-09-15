INSERT INTO customer_summary_report (
    customer_id, 
    total_orders, 
    total_spent, 
    total_discount, 
    avg_sensor_value, 
    max_power
)
WITH aggregated_sales AS (
    SELECT 
        customer_id,
        COUNT(id) AS total_orders,
        SUM(total) AS total_spent,
        SUM(discount) AS total_discount
    FROM sales
    GROUP BY customer_id
),
aggregated_telemetry AS (
    SELECT 
        customer_id,
        AVG(value) AS avg_sensor_value,
        MAX(power) AS max_power
    FROM telemetry
    GROUP BY customer_id
)
SELECT 
    COALESCE(s.customer_id, t.customer_id) AS customer_id,
    COALESCE(s.total_orders, 0),
    COALESCE(s.total_spent, 0.00),
    COALESCE(s.total_discount, 0.00),
    COALESCE(t.avg_sensor_value, 0.00),
    COALESCE(t.max_power, 0.00)
FROM aggregated_sales s
FULL OUTER JOIN aggregated_telemetry t ON s.customer_id = t.customer_id;