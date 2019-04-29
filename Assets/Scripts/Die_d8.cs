using UnityEngine;
using System.Collections;

namespace Game
{
    public class Die_d8 : Die
    {
        override protected Vector3 HitVector(int side)
        {
            switch (side)
            {
                case 1: return new Vector3(0F, 0F, 1F);
                case 2: return new Vector3(-1F, 0F, -1F);
                case 3: return new Vector3(0.5F, 0F, -1F);//
                case 4: return new Vector3(1F, 0F, 0F);
                case 5: return new Vector3(-0.6F, 0.6F, -0.6F);
                case 6: return new Vector3(0F, 0F, -1F);
                case 7: return new Vector3(0F, 0F, -1F);
                case 8: return new Vector3(-0.7F, -0.2F, -0.7F);
            }
            return Vector3.zero;
        }

    }
}