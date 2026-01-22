using System;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class CustomDataModuleRef : Reference
{
    public MinMaxGradientRef? m_custom1;
    public MinMaxGradientRef? m_custom2;
    
    public CustomDataModuleRef(){}

    public CustomDataModuleRef(ParticleSystem.CustomDataModule module)
    {
        m_custom1 = new MinMaxGradientRef(module.GetColor(ParticleSystemCustomData.Custom1));
        m_custom2 = new MinMaxGradientRef(module.GetColor(ParticleSystemCustomData.Custom2));
    }
    

    public void Update(ParticleSystem.CustomDataModule cm)
    {
        ParticleSystem.MinMaxGradient custom1 = cm.GetColor(ParticleSystemCustomData.Custom1);
        ParticleSystem.MinMaxGradient custom2 = cm.GetColor(ParticleSystemCustomData.Custom2);

        if (m_custom1 != null)
        {
            m_custom1.Update(custom1);
        }

        if (m_custom2 != null)
        {
            m_custom2.Update(custom2);
        }
        
        cm.SetColor(ParticleSystemCustomData.Custom1, custom1);
        cm.SetColor(ParticleSystemCustomData.Custom2, custom2);
    }
}