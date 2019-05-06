using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace Game
{
    public class Gameplay : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }

        static Dictionary<int, int> values = new Dictionary<int, int>();

        static public int CurrentPlayer = 1;
        static public int SUMMARY_ROW = 17;
       

        
        static public void updateScore(int id)
        {
            LogArray(GameObject.Find("TableManager").GetComponent<Row>().enemies);
            int score = 0;
            values = Dice.GetValues();
            switch (id)
            {
                case 0:
                    score = Ones();
                    break;
                case 1:
                    score = Twos();
                    break;
                case 2:
                    score = Threes();
                    break;
                case 3:
                    score = Fours();
                    break;
                case 4:
                    score = Fives();
                    break;
                case 5:
                    score = Sixes();
                    break;
                case 6:
                    score = Sevens();
                    break;
                case 7:
                    score = Eights();
                    break;
                case 8:
                    score = Nines();
                    break;
                case 9:
                    score = Tens();
                    break;
                case 10:
                    score = ThreeOfKind();
                    break;
                case 11:
                    score = FourOfKind();
                    break;
                case 12:
                    score = SmallStraight();
                    break;
                case 13:
                    score = BigStraight();
                    break;
                case 14:
                    score = FullHouse();
                    break;
                case 15:
                    score = Yacht();
                    break;
                case 16:
                    score = Chance();
                    break;
            }
            
        }

        static public void Score()
        {
            if (CurrentPlayer == 1)
            {
                int i = 0;
                int sum = 0;
                foreach (var val in GameObject.Find("TableManager").GetComponent<Row>().enemies)
                {
                    if (score1.ContainsKey(i))
                    {
                        val.player1 = score1[i];
                    }
                    else
                    {
                        val.player1 = 0;
                    }
                    if (i != SUMMARY_ROW)
                        sum += val.player1;
                    i++;
                }
               
            }
            else
            {
                int i = 0;
                int sum = 0;
                foreach (var val in GameObject.Find("TableManager").GetComponent<Row>().enemies)
                {
                    if (score2.ContainsKey(i))
                    {
                        val.player2 = score2[i];
                    }
                    else
                    {
                        val.player2 = 0;
                    }
                    if (i != SUMMARY_ROW)
                        sum += val.player2;
                    i++;
                }
               
            }
        }

        static public int Ones(){

            return values.ContainsKey(1) ? values[1] : 0;
        }

        static public int Twos()
        {

            return values.ContainsKey(2) ? values[2]*2 : 0;
        }

        static public int Threes()
        {

            return values.ContainsKey(3) ? values[3]*3 : 0;
        }
        static public int Fours()
        {

            return values.ContainsKey(4) ? values[4]*4 : 0;
        }
        static public int Fives()
        {

            return values.ContainsKey(5) ? values[5]*5 : 0;
        }
        static public int Sixes()
        {
            
            return values.ContainsKey(6) ? values[6]*6 : 0;
        }
        static public int Sevens()
        {

            return values.ContainsKey(7) ? values[7]*7 : 0;
        }
        static public int Eights()
        {

            return values.ContainsKey(8) ? values[8]*8 : 0;
        }
        static public int Nines()
        {

            return values.ContainsKey(9) ? values[9]*9 : 0;
        }
        static public int Tens()
        {

            return values.ContainsKey(10) ? values[10]*10 : 0;
        }
        static public int ThreeOfKind()
        {
            var score = 0;
            foreach (var val in values)
            {
               if(val.Value==3)
                score = val.Key*3;
            }
            return score;
        }
        
        static public int FourOfKind()
        {
            var score = 0;
            foreach (var val in values)
            {
                if (val.Value == 4)
                    score = val.Key * 4;
            }
            return score;
        }

        static public int SmallStraight()
        {
            var score = 0;
            int sqMax = 1;
            int sq = 1;
            foreach (var val in values)
            {
                sq = 1;
                for (int i = val.Key + 1; i < values.Keys.Max(); i++)
                {
                    if (values.ContainsKey(i))
                    {
                        sq++;
                        if (sq == 3)
                            return 25;
                    } else
                    {
                        break;
                    }
                }
            }
            return score;
        }

        static public int BigStraight()
        {
            var score = 0;
            int sqMax = 1;
            int sq = 1;
            foreach (var val in values)
            {
                sq = 1;
                for (int i = val.Key + 1; i < values.Keys.Max(); i++)
                {
                    if (values.ContainsKey(i))
                    {
                        sq++;
                        if (sq == 4)
                            return 30;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return score;
        }

        static public int FullHouse()
        {
            bool flag1 = false;
            bool flag2 = false;
            foreach (var val in values)
            {
                    if (val.Value == 2)
                    {
                         flag1 = true;
                    }
                    if (val.Value == 3)
                    {
                        flag1 = true;
                    }
            }
            return (flag1 && flag2) ? 30 : 0;
        }
        static public int Yacht()
        {
            foreach (var val in values)
            {
                if (val.Value == 5)
                {
                    return 120;
                }
            }
            return 0;
        }

        static public int Chance()
        {
            int sum = 0;
            foreach (var val in values)
            {
                   sum+=val.Key* val.Value;
            }
            return sum;
        }
        static public void LogArray(List<Col> arr)
        {
            foreach(var item in arr)
            {
                Debug.Log(item.player1);
            }
        }
    }
}
