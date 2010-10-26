using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WGiBeat.Notes;

namespace WGiBeat.Managers
{
    public class CPUManager
    {
        public List<Dictionary<BeatlineNoteJudgement, double>> SkillLevels;
        private Random rnd;
        
        public CPUManager()
        {
            SkillLevels = new List<Dictionary<BeatlineNoteJudgement, double>>();
            rnd = new Random();
        }

        public void LoadWeights(string filename)
        {
            //TODO: Implement named difficulty levels.
            SkillLevels.Clear();
            var lines = File.ReadAllLines(filename);
            string[] splitters = {"[", "]", ","};
            foreach (string line in lines)
            {
                if ((line.Length == 0) || (line[0] == '#') || (line.IndexOf('[') == -1) || (line.IndexOf(']') == -1))
                {
                    continue;
                }
                var numbers = line.Split(splitters, StringSplitOptions.RemoveEmptyEntries);

                if (numbers.Count() < 7)
                {
                    continue;
                }
                SkillLevels.Add(CreateSkillLevel(numbers));
            }
        }

        public BeatlineNoteJudgement GetNextJudgement(int level, int streak)
        {
            var total = SkillLevels[level].Values.Sum();
            //Exclude the last skill level value (streak max)
            total -= SkillLevels[level][BeatlineNoteJudgement.COUNT];
            double roll = rnd.Next((int) total);
            System.Diagnostics.Debug.WriteLine(roll + " / " + total);
            var judgement = -1;
            do
            {
                roll = roll - SkillLevels[level][(BeatlineNoteJudgement) judgement + 1];
                judgement++;
            } while (roll > 0);

            //Enforce the max streak rule.
            if ((judgement == 0) && streak >= SkillLevels[level][BeatlineNoteJudgement.COUNT])
            {
                judgement++;
            }

            return ((BeatlineNoteJudgement) judgement);
            
        }

        private Dictionary<BeatlineNoteJudgement, double> CreateSkillLevel(IEnumerable<string> levels)
        {
            var result = new Dictionary<BeatlineNoteJudgement, double>();
            int idx = 0;
            try
            {
                foreach (string value in levels)
                {
                    result.Add((BeatlineNoteJudgement) idx, Double.Parse(value));
                    idx++;
                }
            }
            catch (Exception ex)
            {
                //TODO: Log event
                throw;
            }

            return result;
        }
    }
}