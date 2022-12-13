using System.Diagnostics;

namespace ThriveEventFlow.Diagnostics; 

public interface IWithCustomTags {
    void SetCustomTags(TagList customTags);
}
