using UnityEngine;
using NCMS;

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
