using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class PlantClass
{
    public string ClassId { get; set; } = null!;

    public string ClassName { get; set; } = null!;

    public virtual ICollection<PlantInfo> PlantInfos { get; set; } = new List<PlantInfo>();
}
