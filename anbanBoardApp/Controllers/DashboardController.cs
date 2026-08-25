using anbanBoardApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace KanbanBoardApp.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            List<int> durumDagilimi = new List<int> { 0, 0, 0, 0 }; 
            List<int> oncelikDagilimi = new List<int> { 0, 0, 0, 0 }; 

            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                
                using (SqlCommand komut = new SqlCommand("SELECT Durum, COUNT(Id) FROM Gorevler GROUP BY Durum", baglanti))
                using (SqlDataReader dr = komut.ExecuteReader())
                    while (dr.Read())
                    {
                        string durum = dr[0].ToString();
                        int sayi = Convert.ToInt32(dr[1]);
                        if (durum == "Backlog") durumDagilimi[0] = sayi;
                        else if (durum == "Bekliyor") durumDagilimi[1] = sayi;
                        else if (durum == "DevamEdiyor") durumDagilimi[2] = sayi;
                        else if (durum == "Tamamlandi") durumDagilimi[3] = sayi;
                    }

               
                using (SqlCommand komut2 = new SqlCommand("SELECT Oncelik, COUNT(Id) FROM Gorevler GROUP BY Oncelik", baglanti))
                using (SqlDataReader dr2 = komut2.ExecuteReader())
                    while (dr2.Read())
                    {
                        string oncelik = dr2[0].ToString();
                        int sayi = Convert.ToInt32(dr2[1]);
                        if (oncelik == "Kritik") oncelikDagilimi[0] = sayi;
                        else if (oncelik == "Yüksek") oncelikDagilimi[1] = sayi;
                        else if (oncelik == "Orta") oncelikDagilimi[2] = sayi;
                        else if (oncelik == "Düşük") oncelikDagilimi[3] = sayi;
                    }
            }

            ViewBag.DurumData = durumDagilimi;
            ViewBag.OncelikData = oncelikDagilimi;
            return View();
        }
    }
}