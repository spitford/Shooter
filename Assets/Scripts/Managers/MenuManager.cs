using Project.Networking;
using Project.Utility;
using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Managers {
    public class MenuManager : MonoBehaviour {

        [SerializeField]
        private Button queueButton;

        private SocketIOComponent socketReference;

        private SocketIOComponent SocketReference {
            get {
                return socketReference = (socketReference == null) ? FindObjectOfType<NetworkClient>() : socketReference;
            }
        }

        void Start() {
            queueButton.interactable = false;

            SceneManagementManager.Instance.LoadLevel(SceneList.ONLINE, (levelName) => {
                queueButton.interactable = true;
            });
        }

        public void OnQueue () {
            SocketReference.Emit("joinGame");
        }
    }
}