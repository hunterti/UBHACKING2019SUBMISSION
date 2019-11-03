using System;
using System.Collections.Generic;
using System.Text;

namespace Debugger_Tool
{
    class Class_Example
    {
        public TennisMatch match;
        public Class_Example()
        {
            match = new TennisMatch(30);
        }
        public TennisMatch.score GetScoreP1() 
        {
            return match.Player_One_Score;
        }
        public void UpdateScore(bool player1, TennisMatch.score update) 
        {
            if (player1)
                match.Player_One_Score = update;
            else
                match.Player_Two_Score = update;
        }
}
    class TennisMatch
    {
        object currentScore;
        public enum score { ZERO, FIFTEEN, THIRTY, FORTY };
        public score Player_One_Score;
        public score Player_Two_Score;
        private int num_rounds;
        public TennisMatch(int num_rounds)
        {
            num_rounds = 30;
            Player_One_Score = score.FIFTEEN;
            Player_Two_Score = score.THIRTY;
        }
    }
}
