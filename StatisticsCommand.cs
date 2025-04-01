// Command Pattern Implementation (Gang of Four)
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Spa_Management_System
{
    // Command interface
    public interface IStatisticsCommand
    {
        DataTable Execute();
        string CommandName { get; }
    }

    // Command invoker
    public class StatisticsInvoker
    {
        private readonly SqlConnectionManager _connectionManager;

        public StatisticsInvoker()
        {
            _connectionManager = SqlConnectionManager.Instance;
        }

        public DataTable ExecuteCommand(IStatisticsCommand command)
        {
            try
            {
                return command.Execute();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error executing {command.CommandName}: {ex.Message}");
            }
        }

        public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            return _connectionManager.ExecuteQuery(query, parameters);
        }
    }

    // Concrete commands
    public class GetDailySalesSummaryCommand : IStatisticsCommand
    {
        private readonly DateTime _date;
        private readonly StatisticsInvoker _invoker;

        public GetDailySalesSummaryCommand(StatisticsInvoker invoker, DateTime date)
        {
            _invoker = invoker;
            _date = date;
        }

        public string CommandName => "Daily Sales Summary";

        public DataTable Execute()
        {
            string query = "EXEC sp_GetDailySalesSummary @Date";
            SqlParameter param = new SqlParameter("@Date", _date.Date);
            return _invoker.ExecuteQuery(query, param);
        }
    }

    public class GetPaymentMethodBreakdownCommand : IStatisticsCommand
    {
        private readonly DateTime _date;
        private readonly StatisticsInvoker _invoker;

        public GetPaymentMethodBreakdownCommand(StatisticsInvoker invoker, DateTime date)
        {
            _invoker = invoker;
            _date = date;
        }

        public string CommandName => "Payment Method Breakdown";

        public DataTable Execute()
        {
            string query = "EXEC sp_GetPaymentMethodBreakdown @Date";
            SqlParameter param = new SqlParameter("@Date", _date.Date);
            return _invoker.ExecuteQuery(query, param);
        }
    }

    public class GetTopServicesCommand : IStatisticsCommand
    {
        private readonly DateTime _date;
        private readonly int _topCount;
        private readonly StatisticsInvoker _invoker;

        public GetTopServicesCommand(StatisticsInvoker invoker, DateTime date, int topCount = 5)
        {
            _invoker = invoker;
            _date = date;
            _topCount = topCount;
        }

        public string CommandName => "Top Services";

        public DataTable Execute()
        {
            string query = "EXEC sp_GetTopServices @Date, @TopCount";
            SqlParameter[] parameters = {
                new SqlParameter("@Date", _date.Date),
                new SqlParameter("@TopCount", _topCount)
            };
            return _invoker.ExecuteQuery(query, parameters);
        }
    }

    public class GetTopConsumablesCommand : IStatisticsCommand
    {
        private readonly DateTime _date;
        private readonly int _topCount;
        private readonly StatisticsInvoker _invoker;

        public GetTopConsumablesCommand(StatisticsInvoker invoker, DateTime date, int topCount = 5)
        {
            _invoker = invoker;
            _date = date;
            _topCount = topCount;
        }

        public string CommandName => "Top Consumables";

        public DataTable Execute()
        {
            string query = "EXEC sp_GetTopConsumables @Date, @TopCount";
            SqlParameter[] parameters = {
                new SqlParameter("@Date", _date.Date),
                new SqlParameter("@TopCount", _topCount)
            };
            return _invoker.ExecuteQuery(query, parameters);
        }
    }

    public class GetCustomerStatisticsCommand : IStatisticsCommand
    {
        private readonly DateTime _date;
        private readonly StatisticsInvoker _invoker;

        public GetCustomerStatisticsCommand(StatisticsInvoker invoker, DateTime date)
        {
            _invoker = invoker;
            _date = date;
        }

        public string CommandName => "Customer Statistics";

        public DataTable Execute()
        {
            string query = "EXEC sp_GetCustomerStatistics @Date";
            SqlParameter param = new SqlParameter("@Date", _date.Date);
            return _invoker.ExecuteQuery(query, param);
        }
    }

    public class GetLowStockItemsCommand : IStatisticsCommand
    {
        private readonly int _stockThreshold;
        private readonly StatisticsInvoker _invoker;

        public GetLowStockItemsCommand(StatisticsInvoker invoker, int stockThreshold = 10)
        {
            _invoker = invoker;
            _stockThreshold = stockThreshold;
        }

        public string CommandName => "Low Stock Items";

        public DataTable Execute()
        {
            string query = "EXEC sp_GetLowStockItems @StockThreshold";
            SqlParameter param = new SqlParameter("@StockThreshold", _stockThreshold);
            return _invoker.ExecuteQuery(query, param);
        }
    }
} 