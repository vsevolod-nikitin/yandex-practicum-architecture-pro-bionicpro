CREATE MATERIALIZED VIEW default.mv_sales_to_report TO default.customer_summary_report_mv AS
SELECT 
    payload.after.customer_id AS customer_id,
    1 AS total_orders, 
    payload.after.total AS total_spent,
    payload.after.discount AS total_discount,
    0.0 AS avg_sensor_value,
    0.0 AS max_power,
    now() AS sign_time
FROM default.kafka_sales_queue
WHERE payload.op IN ('c', 'u');
