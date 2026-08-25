using System;

namespace KanbanBoardApp.Models
{
    public class Yorum
    {
        public int Id { get; set; }
        public int GorevId { get; set; }
        public int KullaniciId { get; set; }
        public string YorumMetni { get; set; }
        public DateTime Tarih { get; set; }
    }
}