using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using anbanBoardApp.Models;

namespace anbanBoardApp.Controllers
{
    public class BoardController : Controller
    {
        
        public IActionResult Index()
        {
            List<Sutun> sutunlar = new List<Sutun>();
            List<Gorev> gorevler = new List<Gorev>();
            List<SelectListItem> kullanicilar = new List<SelectListItem>();
            List<SelectListItem> projeler = new List<SelectListItem>();
            List<Sprint> sprintler = new List<Sprint>();

            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                using (SqlCommand komut = new SqlCommand("SELECT * FROM Sutunlar ORDER BY Sira", baglanti))
                using (SqlDataReader dr = komut.ExecuteReader())
                    while (dr.Read()) sutunlar.Add(new Sutun { Id = (int)dr["Id"], Ad = dr["Ad"].ToString(), KodAdi = dr["KodAdi"].ToString(), Renk = dr["Renk"].ToString(), KilitliMi = (bool)dr["KilitliMi"] });
                ViewBag.Sutunlar = sutunlar;

                using (SqlCommand kKomut = new SqlCommand("SELECT Id, AdSoyad FROM Kullanicilar", baglanti))
                using (SqlDataReader kDr = kKomut.ExecuteReader())
                    while (kDr.Read()) kullanicilar.Add(new SelectListItem { Value = kDr["Id"].ToString(), Text = kDr["AdSoyad"].ToString() });
                ViewBag.Kullanicilar = kullanicilar;

                using (SqlCommand pKomut = new SqlCommand("SELECT Id, Ad FROM Projeler", baglanti))
                using (SqlDataReader pDr = pKomut.ExecuteReader())
                    while (pDr.Read()) projeler.Add(new SelectListItem { Value = pDr["Id"].ToString(), Text = pDr["Ad"].ToString() });
                ViewBag.Projeler = projeler;

                using (SqlCommand sKomut = new SqlCommand("SELECT Id, ProjeId, Ad FROM Sprintler", baglanti))
                using (SqlDataReader sDr = sKomut.ExecuteReader())
                    while (sDr.Read()) sprintler.Add(new Sprint { Id = (int)sDr["Id"], ProjeId = (int)sDr["ProjeId"], Ad = sDr["Ad"].ToString() });
                ViewBag.SprintlerBag = sprintler;

                Dictionary<int, int> yorumSayilari = new Dictionary<int, int>();
                using (SqlCommand yKomut = new SqlCommand("SELECT GorevId, COUNT(Id) as Sayi FROM Yorumlar GROUP BY GorevId", baglanti))
                using (SqlDataReader yDr = yKomut.ExecuteReader())
                    while (yDr.Read()) yorumSayilari[Convert.ToInt32(yDr["GorevId"])] = Convert.ToInt32(yDr["Sayi"]);

                Dictionary<int, List<int>> gorevAtamaId = new Dictionary<int, List<int>>();
                using (SqlCommand aKomut = new SqlCommand("SELECT GorevId, KullaniciId FROM GorevAtamalar", baglanti))
                using (SqlDataReader aDr = aKomut.ExecuteReader())
                {
                    while (aDr.Read())
                    {
                        int gId = Convert.ToInt32(aDr["GorevId"]);
                        if (!gorevAtamaId.ContainsKey(gId)) gorevAtamaId[gId] = new List<int>();
                        gorevAtamaId[gId].Add(Convert.ToInt32(aDr["KullaniciId"]));
                    }
                }

                string sqlGorevler = @"
                    SELECT g.*, p.Ad as ProjeAdi, s.Ad as SprintAdi 
                    FROM Gorevler g 
                    LEFT JOIN Projeler p ON g.ProjeId = p.Id 
                    LEFT JOIN Sprintler s ON g.SprintId = s.Id 
                    ORDER BY g.Id DESC";

                using (SqlCommand komut = new SqlCommand(sqlGorevler, baglanti))
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        gorevler.Add(new Gorev
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Baslik = dr["Baslik"] != DBNull.Value ? dr["Baslik"].ToString() : "İsimsiz Görev",
                            Aciklama = dr["Aciklama"] != DBNull.Value ? dr["Aciklama"].ToString() : "",
                            Durum = dr["Durum"] != DBNull.Value ? dr["Durum"].ToString() : "Backlog",
                            Oncelik = dr["Oncelik"] != DBNull.Value ? dr["Oncelik"].ToString() : "Orta",
                            TahminiSure = dr["TahminiSure"] != DBNull.Value ? Convert.ToDecimal(dr["TahminiSure"]) : 0m,
                            EforSaati = dr["EforSaati"] != DBNull.Value ? Convert.ToDecimal(dr["EforSaati"]) : 0m,
                            TeslimTarihi = dr["TeslimTarihi"] != DBNull.Value ? Convert.ToDateTime(dr["TeslimTarihi"]) : null,
                            ProjeId = dr["ProjeId"] != DBNull.Value ? Convert.ToInt32(dr["ProjeId"]) : null,
                            SprintId = dr["SprintId"] != DBNull.Value ? Convert.ToInt32(dr["SprintId"]) : null,
                            ProjeAdi = dr["ProjeAdi"] != DBNull.Value ? dr["ProjeAdi"].ToString() : "",
                            SprintAdi = dr["SprintAdi"] != DBNull.Value ? dr["SprintAdi"].ToString() : "",
                            YorumSayisi = yorumSayilari.ContainsKey(Convert.ToInt32(dr["Id"])) ? yorumSayilari[Convert.ToInt32(dr["Id"])] : 0,
                            AtananKullaniciIdleri = gorevAtamaId.ContainsKey(Convert.ToInt32(dr["Id"])) ? gorevAtamaId[Convert.ToInt32(dr["Id"])] : new List<int>()
                        });
                    }
                }
            }
            return View(gorevler);
        }

        [HttpPost]
        public IActionResult DurumGuncelle(int gorevId, string yeniDurum)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                SqlCommand selectCmd = new SqlCommand($"SELECT Durum, BaslamaTarihi FROM Gorevler WHERE Id = {gorevId}", baglanti);
                string eskiDurum = ""; DateTime? baslamaTarihi = null;
                using (SqlDataReader dr = selectCmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        eskiDurum = dr["Durum"] != DBNull.Value ? dr["Durum"].ToString() : "";
                        baslamaTarihi = dr["BaslamaTarihi"] != DBNull.Value ? Convert.ToDateTime(dr["BaslamaTarihi"]) : null;
                    }
                }

                string sql = $"UPDATE Gorevler SET Durum = '{yeniDurum}'";
                if (yeniDurum == "DevamEdiyor" && eskiDurum != "DevamEdiyor") sql += ", BaslamaTarihi = GETDATE()";
                else if (eskiDurum == "DevamEdiyor" && yeniDurum != "DevamEdiyor" && baslamaTarihi.HasValue)
                {
                    decimal harcananSaat = (decimal)(DateTime.Now - baslamaTarihi.Value).TotalMinutes / 60m;
                    sql += $", EforSaati = EforSaati + {harcananSaat.ToString(System.Globalization.CultureInfo.InvariantCulture)}, BaslamaTarihi = NULL";
                }
                sql += $" WHERE Id = {gorevId}";
                new SqlCommand(sql, baglanti).ExecuteNonQuery();
            }
            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult GorevEkle(string Baslik, string Aciklama, string Durum, string Oncelik, decimal TahminiSure, int? ProjeId, int? SprintId, List<int> AtananKullaniciIdleri, DateTime? TeslimTarihi)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                SqlCommand komut = new SqlCommand("INSERT INTO Gorevler (Baslik, Aciklama, Durum, Oncelik, TahminiSure, ProjeId, SprintId, TeslimTarihi) OUTPUT INSERTED.Id VALUES (@B, @A, @D, @O, @TS, @P, @S, @T)", baglanti);
                komut.Parameters.AddWithValue("@B", Baslik);
                komut.Parameters.AddWithValue("@A", (object)Aciklama ?? DBNull.Value);
                komut.Parameters.AddWithValue("@D", Durum ?? "Backlog");
                komut.Parameters.AddWithValue("@O", Oncelik ?? "Orta");
                komut.Parameters.AddWithValue("@TS", TahminiSure);
                komut.Parameters.AddWithValue("@P", (object)ProjeId ?? DBNull.Value);
                komut.Parameters.AddWithValue("@S", (object)SprintId ?? DBNull.Value);
                komut.Parameters.AddWithValue("@T", (object)TeslimTarihi ?? DBNull.Value);

                int yeniId = (int)komut.ExecuteScalar();

                if (AtananKullaniciIdleri != null)
                    foreach (int kId in AtananKullaniciIdleri) new SqlCommand($"INSERT INTO GorevAtamalar (GorevId, KullaniciId) VALUES ({yeniId}, {kId})", baglanti).ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult GorevDuzenle(int Id, string Baslik, string Aciklama, string Durum, string Oncelik, decimal TahminiSure, int? ProjeId, int? SprintId, List<int> AtananKullaniciIdleri, DateTime? TeslimTarihi)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                SqlCommand komut = new SqlCommand("UPDATE Gorevler SET Baslik=@B, Aciklama=@A, Durum=@D, Oncelik=@O, TahminiSure=@TS, ProjeId=@P, SprintId=@S, TeslimTarihi=@T WHERE Id=@Id", baglanti);
                komut.Parameters.AddWithValue("@Id", Id);
                komut.Parameters.AddWithValue("@B", Baslik);
                komut.Parameters.AddWithValue("@A", (object)Aciklama ?? DBNull.Value);
                komut.Parameters.AddWithValue("@D", Durum ?? "Backlog");
                komut.Parameters.AddWithValue("@O", Oncelik ?? "Orta");
                komut.Parameters.AddWithValue("@TS", TahminiSure);
                komut.Parameters.AddWithValue("@P", (object)ProjeId ?? DBNull.Value);
                komut.Parameters.AddWithValue("@S", (object)SprintId ?? DBNull.Value);
                komut.Parameters.AddWithValue("@T", (object)TeslimTarihi ?? DBNull.Value);
                komut.ExecuteNonQuery();

                new SqlCommand($"DELETE FROM GorevAtamalar WHERE GorevId = {Id}", baglanti).ExecuteNonQuery();
                if (AtananKullaniciIdleri != null)
                    foreach (int kId in AtananKullaniciIdleri) new SqlCommand($"INSERT INTO GorevAtamalar (GorevId, KullaniciId) VALUES ({Id}, {kId})", baglanti).ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult GorevSil(int id)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                new SqlCommand($"DELETE FROM GorevAtamalar WHERE GorevId = {id}", baglanti).ExecuteNonQuery();
                new SqlCommand($"DELETE FROM Yorumlar WHERE GorevId = {id}", baglanti).ExecuteNonQuery();
                new SqlCommand($"DELETE FROM Gorevler WHERE Id = {id}", baglanti).ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GorevDetay(int id)
        {
            var sonuc = new Dictionary<string, object>();
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                using (SqlCommand komut = new SqlCommand("SELECT g.*, p.Ad as ProjeAdi, s.Ad as SprintAdi FROM Gorevler g LEFT JOIN Projeler p ON g.ProjeId=p.Id LEFT JOIN Sprintler s ON g.SprintId=s.Id WHERE g.Id = @id", baglanti))
                {
                    komut.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader dr = komut.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            sonuc.Add("id", id);
                            sonuc.Add("baslik", dr["Baslik"] != DBNull.Value ? dr["Baslik"].ToString() : "");
                            sonuc.Add("aciklama", dr["Aciklama"] != DBNull.Value ? dr["Aciklama"].ToString() : "");
                            sonuc.Add("oncelik", dr["Oncelik"] != DBNull.Value ? dr["Oncelik"].ToString() : "");
                            sonuc.Add("durum", dr["Durum"] != DBNull.Value ? dr["Durum"].ToString() : "");
                            sonuc.Add("proje", dr["ProjeAdi"] != DBNull.Value ? dr["ProjeAdi"].ToString() : "Bağımsız Görev");
                            sonuc.Add("sprint", dr["SprintAdi"] != DBNull.Value ? dr["SprintAdi"].ToString() : "-");
                            sonuc.Add("teslimTarihi", dr["TeslimTarihi"] != DBNull.Value ? Convert.ToDateTime(dr["TeslimTarihi"]).ToString("dd.MM.yyyy HH:mm") : "Belirtilmedi");
                        }
                    }
                }
                var yorumlar = new List<object>();
                using (SqlCommand yKomut = new SqlCommand($"SELECT y.YorumMetni, y.Tarih, k.AdSoyad FROM Yorumlar y JOIN Kullanicilar k ON y.KullaniciId=k.Id WHERE y.GorevId={id} ORDER BY y.Id DESC", baglanti))
                using (SqlDataReader yDr = yKomut.ExecuteReader())
                    while (yDr.Read()) yorumlar.Add(new { metin = yDr["YorumMetni"].ToString(), kisi = yDr["AdSoyad"].ToString(), tarih = Convert.ToDateTime(yDr["Tarih"]).ToString("dd.MM.yyyy HH:mm") });
                sonuc.Add("yorumlar", yorumlar);
            }
            return Json(sonuc);
        }

        [HttpPost]
        public IActionResult YorumEkleAjax(int gorevId, string yorumMetni)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
                new SqlCommand($"INSERT INTO Yorumlar (GorevId, KullaniciId, YorumMetni) VALUES ({gorevId}, 1, '{yorumMetni.Replace("'", "''")}')", baglanti).ExecuteNonQuery();
            return Json(new { success = true });
        }

       
        [HttpGet]
        public IActionResult Personel()
        {
            List<Kullanici> kullanicilar = new List<Kullanici>();
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                using (SqlCommand komut = new SqlCommand("SELECT * FROM Kullanicilar ORDER BY Id DESC", baglanti))
                using (SqlDataReader dr = komut.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        kullanicilar.Add(new Kullanici
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            
                            AdSoyad = dr["AdSoyad"] != DBNull.Value ? dr["AdSoyad"].ToString() : "Bilinmeyen Kullanıcı",
                            Email = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : "E-Posta Belirtilmedi",
                            Rol = dr["Rol"] != DBNull.Value ? dr["Rol"].ToString() : "Çalışan"
                        });
                    }
                }
            }
            return View(kullanicilar);
        }

        [HttpPost]
        public IActionResult PersonelEkle(string AdSoyad, string Email, string Rol)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                SqlCommand komut = new SqlCommand("INSERT INTO Kullanicilar (AdSoyad, Email, Rol) VALUES (@A, @E, @R)", baglanti);

               
                komut.Parameters.AddWithValue("@A", !string.IsNullOrEmpty(AdSoyad) ? AdSoyad : "İsimsiz Kullanıcı");
                komut.Parameters.AddWithValue("@E", !string.IsNullOrEmpty(Email) ? Email : "belirtilmedi");
                komut.Parameters.AddWithValue("@R", !string.IsNullOrEmpty(Rol) ? Rol : "Çalışan");

                komut.ExecuteNonQuery();
            }
            return RedirectToAction("Personel");
        }

        [HttpPost]
        public IActionResult PersonelSil(int id)
        {
            using (SqlConnection baglanti = SqlBaglantisi.BaglantiGetir())
            {
                new SqlCommand($"DELETE FROM Kullanicilar WHERE Id = {id}", baglanti).ExecuteNonQuery();
            }
            return RedirectToAction("Personel");
        }
    }
}