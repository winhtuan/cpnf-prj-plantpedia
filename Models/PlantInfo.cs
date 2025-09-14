using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class PlantInfo
{
    public string PlantId { get; set; } = null!;

    public string ScientificName { get; set; } = null!;

    public string CommonName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string FamilyId { get; set; } = null!;

    public string OrderId { get; set; } = null!;

    public string ClassId { get; set; } = null!;

    public string PlantTypeId { get; set; } = null!;

    public virtual PlantClass Class { get; set; } = null!;

    public virtual PlantFamily Family { get; set; } = null!;

    public virtual PlantOrder Order { get; set; } = null!;

    public virtual ICollection<PlantImg> PlantImgs { get; set; } = new List<PlantImg>();

    public virtual PlantType PlantType { get; set; } = null!;

    public virtual ICollection<Climate> Climates { get; set; } = new List<Climate>();

    public virtual ICollection<Region> Regions { get; set; } = new List<Region>();

    public virtual ICollection<SoilType> SoilTypes { get; set; } = new List<SoilType>();

    public virtual ICollection<Usage> Usages { get; set; } = new List<Usage>();
}
