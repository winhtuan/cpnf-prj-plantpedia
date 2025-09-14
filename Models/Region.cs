using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class Region
{
    public string RegionId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Note { get; set; } = null!;

    public virtual ICollection<PlantInfo> Plants { get; set; } = new List<PlantInfo>();
}
