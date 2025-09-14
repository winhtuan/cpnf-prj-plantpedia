using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class PlantImg
{
    public string ImageId { get; set; } = null!;

    public string PlantId { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string Caption { get; set; } = null!;

    public virtual PlantInfo Plant { get; set; } = null!;
}
