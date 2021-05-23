using UnityEngine;
using System.Collections;
using System.Threading;
using NVIDIA.Flex;

namespace MyThread
{
    public class DebugThread
    {
        volatile bool isWork = false;
        volatile Thread thread;
        FlexContainer.ParticleData particleData = null;
        int maxSize = 0;

        public void ActiveThread()
        {
            //if (thread != null)
            //    return;
            //
            //isWork = true;
            //thread = new Thread(() => Run(particleData, maxSize));
            //thread.Start();
        }

        public void UpdateParticle(FlexContainer.ParticleData _particleData, int _maxSize)
        {
            particleData = _particleData;
            maxSize = _maxSize;

            ActiveThread();
        }

        public void Stop()
        {
            //isWork = false;
        }

        public void Join()
        {
            //thread.Join();
        }

        void Run(FlexContainer.ParticleData _particleData, int max)
        {
            while (isWork)
            {
               //if (particleData == null)
               //    continue;
               //
               //for (int i = 0; i < max; ++i)
               //{
               //    Vector4 getPart = _particleData.GetParticle(i);
               //    if (getPart == null)
               //    {
               //        Debug.Log($"{i}");
               //        break;
               //    }
               //    else if (getPart != Vector4.zero)
               //    {
               //        Debug.Log($"{i}, {getPart}");
               //    }
               //
               //}

            }


        }
    }

}