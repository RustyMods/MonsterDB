using System;
using System.Collections.Generic;

namespace MonsterDB;

[Serializable]
public class VisualRef : Reference
{
    public Vector3Ref? m_scale;
    public List<LevelSetupRef>? m_levelSetups;
    public MaterialRef[]? m_materials;
}