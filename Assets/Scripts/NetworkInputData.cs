using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace root {
    public struct NetworkInputData : INetworkInput
    {
        public const byte MOUSEBUTTON1 = 0x01;
        public const byte MOUSEBUTTON2 = 0x02;

        public byte buttons;
        //For simplicity this examples uses a vector to indicate desired movement direction,
        //but know that there are less bandwidth expensive ways of doing this.
        //For example a bitfield with one bit per direction
        public Vector3 direction;
    }
}
