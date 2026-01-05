using System;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class PlayerVisualRef : VisualRef
{
    [YamlMember(Order = 110)] 
    public int[]? m_modelIndex = new [] { 0, 1 };
    [YamlMember(Order = 111)] 
    public string[]? m_beards = new[]
    {
        "Beard1",
        "Beard2",
        "Beard3",
        "Beard4",
        "Beard5",
        "Beard6",
        "Beard7",
        "Beard8",
        "Beard9",
        "Beard10",
        "Beard11",
        "Beard12",
        "Beard13",
        "Beard14",
        "Beard15",
        "Beard16",
        "Beard17",
        "Beard18",
        "Beard19",
        "Beard20",
        "Beard21",
        "Beard22",
        "Beard23",
        "Beard24",
        "Beard25",
        "Beard26",
        "BeardNone"
    };
    [YamlMember(Order = 112)]
    public string[]? m_hairs = new[]
    {
        "Hair1",
        "Hair2",
        "Hair3",
        "Hair4",
        "Hair5",
        "Hair6",
        "Hair7",
        "Hair8",
        "Hair9",
        "Hair10",
        "Hair11",
        "Hair12",
        "Hair13",
        "Hair14",
        "Hair15",
        "Hair16",
        "Hair17",
        "Hair18",
        "Hair19",
        "Hair20",
        "Hair21",
        "Hair22",
        "Hair23",
        "Hair24",
        "Hair25",
        "Hair26",
        "Hair27",
        "Hair28",
        "Hair29",
        "Hair30",
        "Hair31",
        "Hair32",
        "Hair33",
        "Hair34",
        "HairNone"
    };
    [YamlMember(Order = 113)]
    public string[]? m_hairColors = new[]
    {
        "#000000", // Black
        "#FAF0BF", // Platinum Blonde
        "#A15C00", // Brown
        "#26140D", // Dark Brown
        "#594026", // Medium Brown
        "#8C7359", // Light Brown
        "#F2DE82", // Golden Blonde
        "#D9BF73", // Dirty Blonde
        "#B8A685", // Sandy Blonde
        "#732E14", // Auburn
        "#8C1F0D", // Dark Red
        "#B8401F", // Ginger
        "#666666", // Dark Gray
        "#A6A6A6", // Silver Gray
        "#E0E0E6", // White / Silver
        "#404047", // Charcoal
    };
    [YamlMember(Order = 114)]
    public string[]? m_skinColors = new[]
    {
        "#FFFFFF", // Base / Pale (no tint)
        "#FFF2EB", // Very Fair
        "#FFEBDB", // Fair with Pink undertone
        "#FFF0E0", // Fair
        "#FFE6D1", // Light
        "#FFE0C7", // Light Medium
        "#FAD9B8", // Medium
        "#F2D1AD", // Medium Tan
        "#EBC79E", // Tan
        "#E0B88F", // Deep Tan
        "#D9AD85", // Light Brown
        "#C79E7A", // Medium Brown
        "#FFE0BF", // Warm Beige
        "#FAE6CC", // Peachy Fair
        "#F2DBC2", // Golden Light
    };
}