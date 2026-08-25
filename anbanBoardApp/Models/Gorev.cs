using System;
using System.Collections.Generic;

namespace anbanBoardApp.Models
{
    public class Gorev
    {
        public int Id { get; set; }
        public string Baslik { get; set; }
        public string Aciklama { get; set; }
        public string Durum { get; set; }
        public string Oncelik { get; set; }
        public decimal TahminiSure { get; set; }
        public decimal EforSaati { get; set; }
        public DateTime? BaslamaTarihi { get; set; }
        public DateTime Tarih { get; set; }
        public DateTime? TeslimTarihi { get; set; }
        public int YorumSayisi { get; set; }
        public List<int> AtananKullaniciIdleri { get; set; } = new List<int>();
        public List<string> AtananKullaniciAdlari { get; set; } = new List<string>();

        
        public int? ProjeId { get; set; }
        public int? SprintId { get; set; }
        public string ProjeAdi { get; set; }
        public string SprintAdi { get; set; }
    }
}