using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class PlantType
{
    public string PlantTypeId { get; set; } = null!;

    public string TypeName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public virtual ICollection<PlantInfo> PlantInfos { get; set; } = new List<PlantInfo>();
}
