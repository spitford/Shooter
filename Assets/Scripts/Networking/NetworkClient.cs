﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;
using Project.Utility;
using System;
using Project.Player;
using Project.Scriptable;
using Project.Gameplay;

namespace Project.Networking {
    public class NetworkClient : SocketIOComponent {

        [Header("Network Client")]
        [SerializeField]
        private Transform networkContainer;
        [SerializeField]
        private GameObject playerPrefab;
        [SerializeField]
        private ServerObjects serverSpawnables;

        public static string ClientID { get; private set; }


        private Dictionary<string, NetworkIdentity> serverObjects;
        public override void Start() {
            base.Start();
            initialize();
            setupEvents();
        }

        public override void Update() {
            base.Update();
        }

        private void initialize() {
            serverObjects = new Dictionary<string, NetworkIdentity>();
        }

        private void setupEvents() {
            On("open", (E) => {
                Debug.Log("Connected to Server");
            });

            On("register", (E) => {
                ClientID = E.data["id"].ToString().RemoveQuotes();
                Debug.LogFormat("Our Client's ID ({0})", ClientID);
            });

            On("spawn", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();

                GameObject go = Instantiate(playerPrefab, networkContainer);
                go.name = string.Format("Player: ({0})", id);
                NetworkIdentity ni = go.GetComponent<NetworkIdentity>();
                ni.SetControllerID(id);
                ni.SetSocketReference(this);
                serverObjects.Add(id, ni);
            });

            On("disconnected", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();

                GameObject go = serverObjects[id].gameObject;
                Destroy(go);
                serverObjects.Remove(id);
            });

            On("updatePosition", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;

                NetworkIdentity ni = serverObjects[id];
                ni.transform.position = new Vector3(x, y, 0);
            });

            On("updateRotation", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();
                float tankRotation = E.data["tankRotation"].f;
                float barrelRotation = E.data["barrelRotation"].f;

                NetworkIdentity ni = serverObjects[id];
                ni.transform.localEulerAngles = new Vector3(0, 0, tankRotation);
                ni.GetComponent<PlayerManager>().SetRotation(barrelRotation);
            });

            On("serverSpawn", (E) => {
                string name = E.data["name"].str;
                string id = E.data["id"].ToString().RemoveQuotes();
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;
                Debug.LogFormat("Server wants to spawn a '{0}'", name);

                if (!serverObjects.ContainsKey(id)) {
                    ServerObjectData sod = serverSpawnables.GetObjectByName(name);
                    var spawnedObject = Instantiate(sod.Prefab, networkContainer);
                    spawnedObject.transform.position = new Vector3(x, y, 0);
                    var ni = spawnedObject.GetComponent<NetworkIdentity>();
                    ni.SetControllerID(id);
                    ni.SetSocketReference(this);

                    if (name == "Bullet") {
                        float directionX = E.data["direction"]["x"].f;
                        float directionY = E.data["direction"]["y"].f;
                        string activator = E.data["activator"].ToString().RemoveQuotes();

                        float rot = Mathf.Atan2(directionY, directionX) * Mathf.Rad2Deg;
                        Vector3 currentRotation = new Vector3(0, 0, rot - 90);
                        spawnedObject.transform.rotation = Quaternion.Euler(currentRotation);

                        WhoActivateMe whoActivateMe = spawnedObject.GetComponent<WhoActivateMe>();
                        whoActivateMe.SetActivator(activator);
                    }

                    serverObjects.Add(id, ni);
                }
            });

            On("serverDespawn", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                serverObjects.Remove(id);
                DestroyImmediate(ni.gameObject);
            });

            On("playerDied", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();
                NetworkIdentity ni = serverObjects[id];
                ni.gameObject.SetActive(false);
            });

            On("playerRespawn", (E) => {
                string id = E.data["id"].ToString().RemoveQuotes();
                float x = E.data["position"]["x"].f;
                float y = E.data["position"]["y"].f;
                NetworkIdentity ni = serverObjects[id];
                ni.transform.position = new Vector3(x, y, 0);
                ni.gameObject.SetActive(true);
            });
        }

        public void AttemptToJoinLobby() {
            Emit("joinGame");
        }
    }

    [Serializable]
    public class Player {
        public string id;
        public Position position;
    }


    [Serializable]
    public class Position {
        public float x;
        public float y;
    }

    [Serializable]
    public class PlayerRotation {
        public float tankRotation;
        public float barrelRotation;
    }

    [Serializable]
    public class BulletData {
        public string id;
        public string activator;
        public Position position;
        public Position direction;
    }

    [Serializable]
    public class IDData {
        public string id;
    }
}