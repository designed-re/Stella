using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CorePlugin.EF;
/// <summary>
/// Stores e-amusement cards
/// </summary>
public class Facility
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string PCBId { get; set; } = null!;

    public string Country { get; set; } = null!;
    public string Region { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Type { get; set; } = 0;
    public string CountryName { get; set; } = null!;
    public string CountryJName { get; set; } = null!;
    public string RegionName { get; set; } = null!;
    public string RegionJName { get; set; } = null!;
    public string CustomerCode { get; set; } = null!;
    public string CompanyCode { get; set; } = null!;
    public string FacilityId { get; set; } = null!;

}
