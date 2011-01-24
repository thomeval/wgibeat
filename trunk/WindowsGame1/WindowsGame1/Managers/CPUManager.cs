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
            Log.AddMessage("Initializing CPU Manager...",LogLevel.INFO);
            SkillLevels = new List<Dictionary<BeatlineNoteJudgement, double>>();
            SkillNames = new List<string>();
            _rnd = new Random();
        }

        /// <summary>
        /// Loads a list of CPU skill levels from file. This file contains the 'weights' of
        /// each skill level, which correspond with the probability of the CPU player getting
        /// each BeatlineNoteJudgement.
        /// </summary>
        /// <param name="filename">The file to load CPU skill level weights from.</param>
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

        /// <summary>
        /// Determines the next beatline judgement that the CPU obtained, by using
        /// weighted randomness. The skill level provided should have already been
        /// defined and loaded from file.
        /// </summary>
        /// <param name="skillLevel">The name of the skill level the CPU is using.</param>
        /// <param name="streak">The current streak of the CPU. The maximum streak of the CPU
        /// is limited and also defined in the CPU weights file.</param>
        /// <returns>The BeatlineNoteJudgement obtained by the CPU.</returns>
        public BeatlineNoteJudgement GetNextJudgement(string skillLevel, int streak)
        {
            int idx = SkillNames.IndexOf(skillLevel.ToUpper());
            if (idx > -1)
            {
                return GetNextJudgement(idx, streak);
            }

            Log.AddMessage("CPU Skill level '" + skillLevel +"' does not exist.",LogLevel.ERROR);
            return BeatlineNoteJudgement.COUNT;
        }

        /// <summary>
        /// Determines the next beatline judgement that the CPU obtained, by using
        /// weighted randomness. The skill level used is the index provided.
        /// </summary>
        /// <param name="level">The index of the skill level the CPU is using.</param>
        /// <param name="streak">The current streak of the CPU. The maximum streak of the CPU
        /// is limited and also defined in the CPU weights file.</param>
        /// <returns>The BeatlineNoteJudgement obtained by the CPU.</returns>
        public BeatlineNoteJudgement GetNextJudgement(int level, int streak)
        {
            var total = SkillLevels[level].Values.Sum();
            //Exclude the last skill level value (streak max)
            total -= SkillLevels[level][BeatlineNoteJudgement.COUNT];
            double roll = _rnd.Next((int) total);
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

        /// <summary>
        /// Creates a single skill level from an array (or similar) of numbers.
        /// These numbers are the probabilities that the CPU will obtain an
        /// IDEAL, COOL, OK, BAD, MISS, and FAIL, using that skill level, in order.
        /// In addition, the maximum streak allowed should be the seventh number.
        /// </summary>
        /// <param name="levels">An array of numbers (or similar) to use as skill level weights.</param>
        /// <returns>A new skill level, as a Dictionary.</returns>
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
                Log.AddMessage("Problem parsing skill level file: " + ex.Message,LogLevel.ERROR);
                throw;
            }

            return result;
        }
    }
}