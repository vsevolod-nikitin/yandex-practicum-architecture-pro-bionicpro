from airflow import DAG
from airflow.providers.postgres.operators.postgres import PostgresOperator
from datetime import datetime

default_args = {
    'owner': 'airflow',
    'start_date': datetime(2026, 9, 13),
}

postgres_conn_id='write_to_postgres'

with DAG('postgres_dag',
         default_args=default_args,
         schedule_interval='@once',
         catchup=False) as dag:

    create_sales_table = PostgresOperator(
        task_id='create_sales_table',
        postgres_conn_id=postgres_conn_id,
        sql="""
        DROP TABLE IF EXISTS sales;
        CREATE TABLE sales (
            id SERIAL PRIMARY KEY,
            order_number BIGINT,
            total NUMERIC(18,2),
            discount NUMERIC(18,2),
            customer_id BIGINT
        );
        """
    )

    create_telemetry_table = PostgresOperator(
        task_id='create_telemetry_table',
        postgres_conn_id=postgres_conn_id,
        sql="""
        DROP TABLE IF EXISTS telemetry;
        CREATE TABLE telemetry (
            id SERIAL PRIMARY KEY,
            customer_id BIGINT,
            sensor_type VARCHAR(255),
            value NUMERIC(18,2),
            power NUMERIC(18,2)
        );
        """
    )

    create_customer_summary_report_table = PostgresOperator(
        task_id='create_customer_summary_report_table',
        postgres_conn_id=postgres_conn_id,
        sql="""
        DROP TABLE IF EXISTS customer_summary_report;
        CREATE TABLE customer_summary_report (
            customer_id BIGINT,
            total_orders BIGINT,
            total_spent NUMERIC(18,2),
            total_discount NUMERIC(18,2),
            avg_sensor_value NUMERIC(18,2),
            max_power NUMERIC(18,2)
        );
        """
    )

    run_sales_insert_queries = PostgresOperator(
        task_id="run_sales_insert_queries",
        postgres_conn_id=postgres_conn_id,
        sql="queries/insert_sales.sql",
    )
    run_telemetry_insert_queries = PostgresOperator(
        task_id="run_telemetry_insert_queries",
        postgres_conn_id=postgres_conn_id,
        sql="queries/insert_telemetry.sql",
    )

    run_customer_summary_report_insert_queries = PostgresOperator(
        task_id="run_customer_summary_report_insert_queries",
        postgres_conn_id=postgres_conn_id,
        sql="queries/summary_report.sql",
    )

    result = (
        create_sales_table
        >>create_telemetry_table
        >>create_customer_summary_report_table
        >>run_sales_insert_queries
        >>run_telemetry_insert_queries
        >>run_customer_summary_report_insert_queries)