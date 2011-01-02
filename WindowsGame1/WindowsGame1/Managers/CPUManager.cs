using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using WGiBeat.Notes;

namespace WGiBeat.Managers
{
    public class CPUManager : Manager
    {
        public readonly List<Dictionary<BeatlineNoteJudgement, double>> SkillLevels;
        public readonly List<string> SkillNames;
        private readonly Random _rnd;
        
        public CPUManager(LogManager log)
        {
            Log = log;
            Log.AddMessage("INFO: Initializing CPU Manager...");
            SkillLevels = new List<Dictionary<BeatlineNoteJudgement, double>>();
            SkillNames = new List<string>();
            _rnd = new Random();
        }

        public void LoadWeights(string filename)
        {
            SkillLevels.Clear();

            var lines = File.ReadAllLines(filename);
            string[] splitters = {"[", "]", ","};
            foreach (string line in lines)
            {
                if ((line.Length == 0) || (line[0] == '#') || (line.IndexOf('[') == -1) || (line.IndexOf(']') == -1) || (line.IndexOf('=') == -1))
                {
                    continue;
                }
                var name = line.Substring(0, line.IndexOf("="));

                var numbers = line.Substring(line.IndexOf("=")+1).Split(splitters, StringSplitOptions.RemoveEmptyEntries);

                if (numbers.Count() < 7)
                {
                    continue;
                }
                SkillNames.Add(name.ToUpper());
                SkillLevels.Add(CreateSkillLevel(numbers));
            }
        }

        public BeatlineNoteJudgement GetNextJudgement(string skillLevel, int streak)
        {
            int idx = SkillNames.IndexOf(skillLevel.ToUpper());
            if (idx > -1)
            {
                return GetNextJudgement(idx, streak);
            }

            Log.AddMessage("ERROR: CPU Skill level '" + skillLevel +"' does not exist.");
            return BeatlineNoteJudgement.COUNT;
        }
        public BeatlineNoteJudgement GetNextJudgement(int level, int streak)
        {
            var total = SkillLevels[level].Values.Sum();
            //Exclude the last skill level value (streak max)
            total -= SkillLevels[level][BeatlineNoteJudgement.COUNT];
            double roll = _rnd.Next((int) total);
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
                Log.AddMessage("ERROR: Problem parsing skill level file: " + ex.Message);
                throw;
            }

            return result;
        }
    }
}