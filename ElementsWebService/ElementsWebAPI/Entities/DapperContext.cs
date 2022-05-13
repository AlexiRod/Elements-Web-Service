using Microsoft.Data.SqlClient;
using System.Data;

namespace ElementsWebAPI.Entities
{
    /// <summary>
    /// Контекст базы данных для работы Dapper
    /// </summary>
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        /// <summary>
        /// Конструктор для задания конфигурации сервиса
        /// </summary>
        /// <param name="configuration">Конфигурация сервиса</param>
        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SqlConnection");
        }

        /// <summary>
        /// Конструктор для тестирования.
        /// </summary>
        /// <param name="connection">Строка подключения к БД</param>
        public DapperContext(string connection)
        {
            _connectionString = connection;
        }

        /// <summary>
        /// Создание подключения к БД через данные из ConnectionString
        /// </summary>
        /// <returns>Соединение с БД</returns>
        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);

    }
}
