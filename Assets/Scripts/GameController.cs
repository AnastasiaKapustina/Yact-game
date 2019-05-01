using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class GameController : MonoBehaviour
    {
        private const int MODE_ROLL = 2;
        private int mode = 0;

        private GameObject nextCameraPosition = null;
        private GameObject startCameraPosition = null;
        public GameObject camRoll = null;
        public GameObject camStart = null;
        private float cameraMovementSpeed = 0.5F;
        private float cameraMovement = 0;

        public GameObject RollBtn;
        public GameObject NextBtn;
        public GameObject Table;
        public GameObject spawnPoint;



        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (nextCameraPosition != null)
                MoveCamera();
        }

        public void StartGame()
        {
            SetMode(MODE_ROLL);
        }

        private void SetMode(int pMode)
        {
            if (nextCameraPosition != null || pMode == mode) return;

            switch (pMode)
            {

                case MODE_ROLL:
                    startCameraPosition = camStart;
                    nextCameraPosition = camRoll;
                    GameObject.Find("Start Game").SetActive(false);
                    GameObject.Find("SmallBoard").SetActive(false);
                    RollBtn.SetActive(true);
                    Table.SetActive(true);
                    NextBtn.SetActive(true);
                    break;
            }

            mode = pMode;
            cameraMovement = 0;
        }

        private Vector3 Force()
        {
            Vector3 rollTarget = Vector3.zero + new Vector3(2.47f + 1.35f * Random.value, 1.3f, 0.55f - 0.91f * Random.value);
            return Vector3.Lerp(spawnPoint.transform.position, rollTarget, 1).normalized * (6 - Random.value * 0);
            // 
        }

        public void Roll()
        {
            Dice.Clear();
            Dice.Roll("5d10", spawnPoint.transform.position, Force());
        }
    }
}
