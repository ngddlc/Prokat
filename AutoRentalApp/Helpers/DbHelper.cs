using System.Configuration;

namespace AutoRentalApp.Helpers
{
    public static class DbHelper
    {
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
                   ?? "Host=localhost;Port=5432;Database=Prokat_Avto;Username=postgres;Password=1234";
        }
    }
}