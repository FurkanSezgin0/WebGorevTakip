using System.Data.SqlClient;

namespace anbanBoardApp.Models
{
    public class SqlBaglantisi
    {
        public static SqlConnection BaglantiGetir()
        {
            
            string baglantiCumlesi = @"Data Source=.\SQLEXPRESS;Initial Catalog=KanbanDB;Integrated Security=True;";

            SqlConnection baglanti = new SqlConnection(baglantiCumlesi);
            baglanti.Open();
            return baglanti;
        }
    }
}