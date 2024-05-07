using NCMS;
using UnityEngine;

namespace TraitsDuplicatorMod_NCMS
{
    [ModEntry]
    class Main : MonoBehaviour
    {
        void Awake()
        {
            TraitsDuplicatorModClass.init();
        }
    }
}
