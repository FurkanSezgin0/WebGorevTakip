using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using anbanBoardApp.Models;

namespace anbanBoardApp.Controllers
{
    public class EforController : Controller
    {
        public IActionResult Index()
        {
            List<Gorev> eforluGorevler = new List<Gorev>();
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                using (SqlCommand komut = new SqlCommand("SELECT Id, Baslik, Durum, EforSaati, TahminiSure FROM Gorevler WHERE EforSaati > 0 OR TahminiSure > 0 ORDER BY Id DESC", baglanti))
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        eforluGorevler.Add(new Gorev
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Baslik = dr["Baslik"].ToString(),
                            Durum = dr["Durum"].ToString(),
                            EforSaati = Convert.ToDecimal(dr["EforSaati"]),
                            TahminiSure = Convert.ToDecimal(dr["TahminiSure"])
                        });
                    }
                }
            }
            return View(eforluGorevler);
        }
    }
}