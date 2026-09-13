import React, { useState, useEffect } from 'react';

interface CustomerSummaryReport {
  customer_id: number;
  total_orders: number;
  total_spent: number;
  total_discount: number;
  avg_sensor_value: number;
  max_power: number;
}

const ReportPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isAuthenticated, setIsAuthenticated] = useState<boolean | null>(null);
  const [reportData, setReportData] = useState<CustomerSummaryReport | null>(null);

  useEffect(() => {
    fetch(`${process.env.REACT_APP_AUTH_URL}/api/auth/token`, { credentials: 'include' })
      .then(response => {
        if (response.ok) {
          setIsAuthenticated(true);
        } else {
          setIsAuthenticated(false);
        }
      })
      .catch(() => setIsAuthenticated(false));
  }, []);

  const handleLogin = () => {
    window.location.href = `${process.env.REACT_APP_AUTH_URL}/api/Auth/login?returnUrl=${window.location.href}`;
  };

  const downloadReport = async () => {
    try {
      setLoading(true);
      setError(null);

      const response = await fetch(`${process.env.REACT_APP_API_URL}/api/reports`, {
        credentials: 'include'
      });

      const data: CustomerSummaryReport = await response.json();
      setReportData(data);
      
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  if (!isAuthenticated) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen bg-gray-100">
        <button
          onClick={() => handleLogin()}
          className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        >
          Login
        </button>
      </div>
    );
  }

  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gray-100">
      <div className="p-8 bg-white rounded-lg shadow-md">
        <h1 className="text-2xl font-bold mb-6">Usage Reports</h1>
        
        <button
          onClick={downloadReport}
          disabled={loading}
          className={`px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 ${
            loading ? 'opacity-50 cursor-not-allowed' : ''
          }`}
        >
          {loading ? 'Generating Report...' : 'Download Report'}
        </button>

        {error && (
          <div className="mt-4 p-4 bg-red-100 text-red-700 rounded">
            {error}
          </div>
        )}

        {reportData && (
          <div className="mt-4 p-4 border rounded bg-gray-50 text-gray-800 space-y-2">
            <h2 className="font-semibold text-lg border-b pb-1 mb-2">Отчет (ID: {reportData.customer_id})</h2>
            <div className="flex justify-between text-sm"><span>Заказов:</span> <strong>{reportData.total_orders}</strong></div>
            <div className="flex justify-between text-sm"><span>Суммарная стоимость:</span> <strong>{reportData.total_spent} ₽</strong></div>
            <div className="flex justify-between text-sm"><span>Скидка:</span> <strong>{reportData.total_discount} ₽</strong></div>
            <div className="flex justify-between text-sm"><span>Ср. значение датчиков:</span> <strong>{reportData.avg_sensor_value}</strong></div>
            <div className="flex justify-between text-sm"><span>Макс. мощность:</span> <strong>{reportData.max_power} кВт</strong></div>
          </div>
        )}
      </div>
    </div>
  );
};

export default ReportPage;