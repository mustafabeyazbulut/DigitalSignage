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
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var def = JsonSerializer.Deserialize<LayoutDefinitionModel>(LayoutDefinition ?? "{}", options);
                    if (def?.Rows == null) return 0;
                    return CountSections(def.Rows);
                }
                catch { return 0; }
            }
        }

        private static int CountSections(List<LayoutRowDefinition> rows)
        {
            int count = 0;
            foreach (var row in rows)
            {
                if (row.Columns == null) continue;
                foreach (var col in row.Columns)
                {
                    if (col.Rows != null && col.Rows.Count > 0)
                        count += CountSections(col.Rows);
                    else
                        count++;
                }
            }
            return count;
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
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var def = JsonSerializer.Deserialize<LayoutDefinitionModel>(LayoutDefinition ?? "{}", options);
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
        /// <summary>
        /// İç içe bölme: sütunun kendi alt satır/sütun yapısı (opsiyonel).
        /// Null ise sütun yaprak (leaf) hücredir.
        /// </summary>
        public List<LayoutRowDefinition>? Rows { get; set; }
    }
}
