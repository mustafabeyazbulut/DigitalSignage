using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DigitalSignage.Models
{
    public class Layout
    {
        public int LayoutID { get; set; }
        public int CompanyID { get; set; }

        [Required]
        [MaxLength(255)]
        public string LayoutName { get; set; } = string.Empty;

        /// <summary>
        /// Düzen yapısının JSON tanımı.
        /// Format: { "rows": [{ "height": 50, "columns": [{ "width": 60 }, { "width": 40 }] }, ...] }
        /// </summary>
        public string LayoutDefinition { get; set; } = "{\"rows\":[{\"height\":100,\"columns\":[{\"width\":100}]}]}";

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Düzen tanımı JSON'ından hesaplanan toplam bölüm (hücre) sayısı.
        /// </summary>
        public int TotalSections
        {
            get
            {
                try
                {
                    var def = JsonSerializer.Deserialize<LayoutDefinitionModel>(LayoutDefinition ?? "{}");
                    if (def?.Rows == null) return 0;
                    int count = 0;
                    foreach (var row in def.Rows)
                        count += row.Columns?.Count ?? 0;
                    return count;
                }
                catch { return 0; }
            }
        }

        /// <summary>
        /// Düzen tanımı JSON'ından hesaplanan satır sayısı.
        /// </summary>
        public int RowCount
        {
            get
            {
                try
                {
                    var def = JsonSerializer.Deserialize<LayoutDefinitionModel>(LayoutDefinition ?? "{}");
                    return def?.Rows?.Count ?? 0;
                }
                catch { return 0; }
            }
        }

        // Navigasyon Özellikleri
        public Company Company { get; set; } = null!;
        public ICollection<LayoutSection> LayoutSections { get; set; } = new List<LayoutSection>();
        public ICollection<Page> Pages { get; set; } = new List<Page>();
    }

    /// <summary>
    /// LayoutDefinition JSON deserializasyon modeli.
    /// </summary>
    public class LayoutDefinitionModel
    {
        public List<LayoutRowDefinition>? Rows { get; set; }
    }

    public class LayoutRowDefinition
    {
        public double Height { get; set; }
        public List<LayoutColumnDefinition>? Columns { get; set; }
    }

    public class LayoutColumnDefinition
    {
        public double Width { get; set; }
    }
}
