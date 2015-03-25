using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public interface IAutoOrderSettingsConfiguration
{
    bool AllowAutoship { get; }
    bool AllowStartDateChoice { get; }
    List<Exigo.WebService.FrequencyType> AvailableFrequencyTypes { get; }
    Exigo.WebService.FrequencyType DefaultFrequencyType { get; }
}
