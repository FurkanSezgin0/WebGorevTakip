using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using anbanBoardApp.Models;

namespace anbanBoardApp.Controllers
{
    public class SprintlerController : Controller
    {
        public IActionResult Index()
        {
            List<Sprint> sprintler = new List<Sprint>();
            List<SelectListItem> projeler = new List<SelectListItem>();

            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                using (SqlCommand komut = new SqlCommand("SELECT s.Id, s.ProjeId, s.Ad, p.Ad as ProjeAdi FROM Sprintler s JOIN Projeler p ON s.ProjeId = p.Id ORDER BY s.Id DESC", baglanti))
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        sprintler.Add(new Sprint
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            ProjeId = Convert.ToInt32(dr["ProjeId"]),
                            Ad = dr["Ad"].ToString(),
                            ProjeAdi = dr["ProjeAdi"].ToString()
                        });
                    }
                }
                using (SqlCommand pKomut = new SqlCommand("SELECT Id, Ad FROM Projeler", baglanti))
                using (SqlDataReader pDr = pKomut.ExecuteReader())
                {
                    while (pDr.Read()) projeler.Add(new SelectListItem { Value = pDr["Id"].ToString(), Text = pDr["Ad"].ToString() });
                }
                ViewBag.Projeler = projeler;
            }
            return View(sprintler);
        }

        [HttpPost]
        public IActionResult Ekle(int ProjeId, string Ad)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
                new SqlCommand($"INSERT INTO Sprintler (ProjeId, Ad) VALUES ({ProjeId}, '{Ad}')", baglanti).ExecuteNonQuery();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Sil(int id)    
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
                new SqlCommand($"DELETE FROM Sprintler WHERE Id = {id}", baglanti).ExecuteNonQuery();
            return RedirectToAction("Index");
        }
    }
}