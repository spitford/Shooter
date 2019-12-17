using Project.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Managers {
    public class ApplicationManager : MonoBehaviour {
        void Start() {
            SceneManagementManager.Instance.LoadLevel(SceneList.MAIN_MENU, (levelName) => {});
        }
    }
}