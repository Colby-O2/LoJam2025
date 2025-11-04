using UnityEngine;

namespace DialogueGraph.Enumeration
{
    public enum DialogueType
    {
        Start,
        SingleChoice,
        MultipleChoice,
        SetBoolean,
        Branch,
        Increment,
        Comparator,
        Connector,
        SetInt,
        EmitEvent
    }
}
