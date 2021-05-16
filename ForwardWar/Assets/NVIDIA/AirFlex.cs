using UnityEngine;
using System.Collections;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class AirFlex : MonoBehaviour
    {
        FlexActor m_actor;

        void OnEnable()
        {
            m_actor = GetComponent<FlexActor>();
            if (m_actor)
            {
                m_actor.onFlexUpdate += OnFlexUpdate;
            }
        }

        void OnDisable()
        {
            if (m_actor)
            {
                m_actor.onFlexUpdate -= OnFlexUpdate;
                m_actor = null;
            }
        }

        void OnFlexUpdate(FlexContainer.ParticleData _particleData)
        {
            if (m_actor && m_actor.container)
            {
                m_actor.container.AddFluidIndices(m_actor.indices, m_actor.indexCount);
            }
        }

    }
}