using System;
using System.Collections.Generic;

namespace Plantpedia.Models;

public partial class Climate
{
    public string ClimateId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string TemperatureRange { get; set; } = null!;

    public string RainfallRange { get; set; } = null!;

    public string HumidityRange { get; set; } = null!;

    public virtual ICollection<PlantInfo> Plants { get; set; } = new List<PlantInfo>();
}
