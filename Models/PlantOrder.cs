using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class PlantOrder
{
    public string OrderId { get; set; } = null!;

    public string OrderName { get; set; } = null!;

    public virtual ICollection<PlantInfo> PlantInfos { get; set; } = new List<PlantInfo>();
}
