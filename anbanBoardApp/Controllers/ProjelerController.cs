using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using anbanBoardApp.Models;

namespace anbanBoardApp.Controllers
{
    public class ProjelerController : Controller
    {
        public IActionResult Index()
        {
            List<Proje> projeler = new List<Proje>();
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                using (SqlCommand komut = new SqlCommand("SELECT * FROM Projeler ORDER BY Id DESC", baglanti))
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        projeler.Add(new Proje
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Ad = dr["Ad"].ToString(),
                            Aciklama = dr["Aciklama"].ToString(),
                            BaslangicTarihi = Convert.ToDateTime(dr["BaslangicTarihi"]),
                            BitisTarihi = dr["BitisTarihi"] != DBNull.Value ? Convert.ToDateTime(dr["BitisTarihi"]) : null,
                            Durum = dr["Durum"].ToString()
                        });
                    }
                }
            }
            return View(projeler);
        }

        [HttpPost]
        public IActionResult Ekle(string Ad, string Aciklama, DateTime BaslangicTarihi, DateTime? BitisTarihi)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                SqlCommand komut = new SqlCommand("INSERT INTO Projeler (Ad, Aciklama, BaslangicTarihi, BitisTarihi, Durum) VALUES (@Ad, @Aciklama, @Bas, @Bit, 'Aktif')", baglanti);
                komut.Parameters.AddWithValue("@Ad", Ad);
                komut.Parameters.AddWithValue("@Aciklama", (object)Aciklama ?? DBNull.Value);
                komut.Parameters.AddWithValue("@Bas", BaslangicTarihi);
                komut.Parameters.AddWithValue("@Bit", (object)BitisTarihi ?? DBNull.Value);
                komut.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Sil(int id)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                new SqlCommand($"DELETE FROM Projeler WHERE Id = {id}", baglanti).ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}