using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nSummerizeTool
{
    
    public class SummeryTool
    {
       private readonly char[] sentenceDelimiters = new char[] { '.' };
        private readonly string[] paragraphDlimiters = new string[] { "\n\n" };
        private readonly char[] wordsDelimiters = new char[] { ' ' };
        private double avgWords;
        
        
        public SummeryTool()
        {
            // nothing to do
        }

        public SummeryTool(char[] sentenceDelimeters)
        {
            this.sentenceDelimiters = sentenceDelimeters;
        }

        public SummeryTool(char[] sentenceDelimeters, string[] paragraphDelimiters)
            : this(sentenceDelimeters)
        {
            this.paragraphDlimiters = paragraphDelimiters;
        }

        string[] splitToSentences(string paragraph)
        {
            return paragraph.Split(sentenceDelimiters);
        }

        string[] splitToParagraphs(string content)
        {
            return content.Split(paragraphDlimiters,StringSplitOptions.RemoveEmptyEntries);
        }

        string[] splitToWods(string sentence)
        {
            return sentence.Split(wordsDelimiters);
        }

        double sentenceIntersection(string[] sentence1, string[] sentence2)
        {
            // so we dont devide by 0
            if (sentence1.Length + sentence2.Length == 0)
                return 0;

            var intersectionWords = sentence1.Intersect(sentence2,StringComparer.OrdinalIgnoreCase);
            //avgWords = (sentence1.Length + sentence2.Length )/ 2;

            // normalize the intersection by the avg num of words
            return intersectionWords.Count() / avgWords;
        }

        private void setAvgWords(string[] sentences)
        {
            avgWords = 0;
            foreach (var sentence in sentences)
            {
                avgWords += sentence.Length;
            }
            avgWords = avgWords / sentences.Length;
        }

        // remove non alpha
        string[] formatSentence(string[] sentence)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");

            return sentence.AsParallel().Select(X => rgx.Replace(X, "")).ToArray();
        }

        public Dictionary<string, double> GetSentencesRank(string Content)
        {
            var res = new Dictionary<string, double>();
            string[] sentences;

            sentences = splitToSentences(Content);
            int len = sentences.Length;
            setAvgWords(sentences);

            double[][] rankMat = new double[len][];
            // set the intersection for each 2 sentences
            for (int i = 0; i < len; i++)
            {
                rankMat[i] = new double[len];
                for (int j = 0; j < len; j++)
                {
                    rankMat[i][j] = sentenceIntersection(splitToWods(sentences[i]), splitToWods(sentences[j]));
                }
            }
            for (int i = 0; i < len; i++)
            {
                res.Add(sentences[i], rankMat[i].Sum());
            }
            return res;
        }

        public string[] ThresholdSummerize(string content)
        {
            var sentancesWithRank = GetSentencesRank(content);

            return SummerizeByThreshold(sentancesWithRank, 0.2);
        }

        public string Summerize(string content)
        {
            var parag = splitToParagraphs(content);
            StringBuilder sb = new StringBuilder();
            foreach (var par in parag )
            {
                sb.Append(GetSentencesRank(par).OrderByDescending(x => x.Value).First().Key + "\n");
            }

            return sb.ToString();
        }

        private string[] SummerizeByThreshold(Dictionary<string, double> sentancesWithRank, double p)
        {
            //throw new NotImplementedException();

            return sentancesWithRank.OrderByDescending(X => X.Value).Select(X => X.Key).Take((int)(sentancesWithRank.Count * p)).ToArray();

        }

        public string mergeStringsToParagraph(string[] content)
        {
            StringBuilder sb = new StringBuilder();
            int counter = 0;
            for (int i= 0 ; i< content.Length ; i++)
            {
                var words = splitToWods(content[i]);
                for (int j = 0; j < words.Length; j++)
                {
                    sb.Append(words[j]);
                    if (++counter % 6 == 0)
                        sb.Append(".");
                    sb.Append(" ");

                }

            }
            return sb.ToString();
        }

    }
}
