using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasHD.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int IdTicket { get; set; }
        [DataType(DataType.MultilineText)]
        public string Detail { get; set; }
        [Required]
        public DateTime SendDate { get; set; }
        [Required]
        public string SenderEmail { get; set; }
        [Required]
        public string SubStatus { get; set; }
        public string FilePath { get; set; }
    }
}