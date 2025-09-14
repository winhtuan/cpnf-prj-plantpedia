using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class PlantFamily
{
    public string FamilyId { get; set; } = null!;

    public string FamilyName { get; set; } = null!;

    public virtual ICollection<PlantInfo> PlantInfos { get; set; } = new List<PlantInfo>();
}
