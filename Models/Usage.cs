using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class Usage
{
    public string UsageId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Note { get; set; } = null!;

    public virtual ICollection<PlantInfo> Plants { get; set; } = new List<PlantInfo>();
}
