using Project.Utility.Attributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Gameplay {
    public class WhoActivateMe : MonoBehaviour {

        [SerializeField]
        [GreyOut]
        private string whoActivateMe;

        public void SetActivator(string ID) {
            whoActivateMe = ID;
        }

        public string GetActivator() {
            return whoActivateMe;
        }
    }
}