using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemasHD.Models
{
    public class Ticket
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Category { get; set; }
        public string Module { get; set; }
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        public Nullable<DateTime> FinishDate { get; set; }
        public Nullable<DateTime> CommitmentDate { get; set; }
        public bool Attendance { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
        public string Company { get; set; }
        public string Status { get; set; }
        public string FilePath { get; set; }
        public Nullable<DateTime> ProposalDate { get; set; }
        public Nullable<DateTime> LastUpdate { get; set; }
    }
}