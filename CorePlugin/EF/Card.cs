using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace CorePlugin.EF
{
    public class Card
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CardId { get; set; }

        public string CardNo { get; set; }

        /// <summary>
        /// same with dataid
        /// </summary>
        public string RefId { get; set; }

        public string PassCode { get; set; }

        public int Paseli { get; set; } = 10000;

        public string? PaseliSession { get; set; }
    }
}
