using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Dice : MonoBehaviour
    {
        public const int MOUSE_LEFT_BUTTON = 0;
        public const int MOUSE_RIGHT_BUTTON = 1;
        public const int MOUSE_MIDDLE_BUTTON = 2;

        public float rollSpeed = 0.25F;

        public static bool rolling = true;

        protected float rollTime = 0;

        private static ArrayList matNames = new ArrayList();
        private static ArrayList materials = new ArrayList();
        private static ArrayList rollQueue = new ArrayList();
        private static ArrayList allDice = new ArrayList();
        private static ArrayList rollingDice = new ArrayList();
        
        //Создаем префаб
        public static GameObject prefab(string name, Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Object pf = Resources.Load("Prefabs/"+name);
            if (pf != null)
            {
                GameObject inst = (GameObject)GameObject.Instantiate(pf, Vector3.zero, Quaternion.identity);
                if (inst != null)
                {
                    inst.transform.position = position;
                    inst.transform.Rotate(rotation);
                    inst.transform.localScale = scale;
                    return inst;
                }
            }
            else
                Debug.Log("Prefab " + name + " not found!");
            return null;
        }

        public static Material material(string matName)
        {
            Material mat = null;
            int idx = matNames.IndexOf(matName);
            if (idx < 0)
            {
                string[] a = matName.Split('-');
                if (a.Length > 1)
                {
                    a[0] = a[0].ToLower();
                    if (a[0].IndexOf("d") == 0)
                        mat = (Material)Resources.Load("Materials/" + a[0] + "/" + matName);
                }
                if (mat == null) mat = (Material)Resources.Load("Materials/" + matName);
                if (mat != null)
                {
                    matNames.Add(matName);
                    materials.Add(mat);
                }
            }
            else
                mat = (Material)materials[idx];
            return mat;
        }

        public static void Roll(string dice, Vector3 spawnPoint, Vector3 force)
        {
            rolling = true;
            dice = dice.ToLower();
            int count = 1;
            string dieType = "d6";

            int p = dice.IndexOf("d");

            if (p >= 0)
            {
                if (p > 0)
                {
                    string[] a = dice.Split('d');
                    count = System.Convert.ToInt32(a[0]);
                    if (a.Length > 1)
                        dieType = "d" + a[1];
                    else
                        dieType = "d6";
                }
                else
                    dieType = dice;

                for (int d = 0; d < count; d++)
                {

                    GameObject die = prefab(dieType, spawnPoint, Vector3.zero, new Vector3(0.07f, 0.07f, 0.07f));
                    die.transform.Rotate(new Vector3(Random.value * 360, Random.value * 360, Random.value * 360));
                    die.SetActive(false);
                    RollingDie rDie = new RollingDie(die, dieType, spawnPoint, force);
                    allDice.Add(rDie);
                    rollQueue.Add(rDie);
                }
            }
        }

        // Считаем кол-во костей определённого типа
        public static int Count(string dieType)
        {
            int v = 0;
            for (int d = 0; d < allDice.Count; d++)
            {
                RollingDie rDie = (RollingDie)allDice[d];
                if (rDie.name == dieType || dieType == "")
                    v++;
            }
            return v;
        }

        

        public static Dictionary<int, int> GetValues()
        {

            Dictionary<int, int> numbers = new Dictionary<int, int>();
            for (int d = 0; d < allDice.Count; d++)
            {
                RollingDie rDie = (RollingDie)allDice[d];
                {
                    if (numbers.ContainsKey(rDie.die.value))
                    {
                        numbers[rDie.die.value]++;
                    }
                    else {
                        numbers[rDie.die.value] = 1;
                    }
                }
            }
            return numbers;
        }

        //Убираем все кубики
        public static void Clear()
        {
            for (int d = 0; d < allDice.Count; d++)
                GameObject.Destroy(((RollingDie)allDice[d]).gameObject);

            allDice.Clear();
            rollingDice.Clear();
            rollQueue.Clear();

            rolling = false;
        }

        void Update()
        {
            if (rolling)
            {
                rollTime += Time.deltaTime;
                //кидаем кубик, если прошло rollTime
                if (rollQueue.Count > 0 && rollTime > rollSpeed)
                {
                    RollingDie rDie = (RollingDie)rollQueue[0];
                    GameObject die = rDie.gameObject;
                    die.SetActive(true);
                    die.GetComponent<Rigidbody>().AddForce((Vector3)rDie.force, ForceMode.Impulse);
                    die.GetComponent<Rigidbody>().AddTorque(new Vector3(-10 * Random.value * die.transform.localScale.magnitude, -10 * Random.value * die.transform.localScale.magnitude, -10 * Random.value * die.transform.localScale.magnitude), ForceMode.Impulse);
                    rollingDice.Add(rDie);
                    rollQueue.RemoveAt(0);
                    rollTime = 0;
                }
                else
                    if (rollQueue.Count == 0)
                {
                    if (!IsRolling())
                        rolling = false;
                }
            }
        }

        private bool IsRolling()
        {
            int d = 0;
            while (d < rollingDice.Count)
            {
                RollingDie rDie = (RollingDie)rollingDice[d];
                if (!rDie.rolling)
                    rollingDice.Remove(rDie);
                else
                    d++;
            }
            return (rollingDice.Count > 0);
        }
    }


    //Отдельный класс для кубика на поле
    class RollingDie
    {

        public GameObject gameObject;
        public Die die; 

        public string name = "";
        public Vector3 spawnPoint;
        public Vector3 force;

        public bool rolling
        {
            get
            {
                return die.rolling;
            }
        }

        public int value
        {
            get
            {
                return die.value;
            }
        }

        public RollingDie(GameObject gameObject, string name, Vector3 spawnPoint, Vector3 force)
        {
            this.gameObject = gameObject;
            this.name = name;
            this.spawnPoint = spawnPoint;
            this.force = force;
            die = (Die)gameObject.GetComponent(typeof(Die));
        }

        public void ReRoll()
        {
            if (name != "")
            {
                GameObject.Destroy(gameObject);
                Dice.Roll(name, spawnPoint, force);
            }
        }
    }
}