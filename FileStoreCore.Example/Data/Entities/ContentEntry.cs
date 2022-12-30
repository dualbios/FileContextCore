using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using FileStoreCore.Example.Data.Entities;

namespace FileStoreCore.Example.Data.Entities
{
    public class ContentEntry : Base
    {
        public string Text { get; set; }

        [ForeignKey("Content")]
        public int ContentId { get; set; }

        public virtual Content Content { get; set; }
    }
}
