using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleButtonCheck
{
    public class Scramble
    {
        public static List<KeyValuePair<Word, int>> Solve(char[] stringOfLetters)
        {
            var letterGrid = new List<List<string>>();

            var thisRow = new List<string>();
            for (int i = 0; i < stringOfLetters.Length; i++)
            {
                var thisLetterString = "";
                var thisLetter = stringOfLetters[i];
                if (thisLetter == 'q')
                {
                    thisLetterString = "qu";
                    i++;
                }
                else
                    thisLetterString = thisLetter.ToString();
                thisRow.Add(thisLetterString);
                if (thisRow.Count == 4)
                {
                    letterGrid.Add(thisRow);
                    thisRow = new List<string>();
                }
            }
            var solver = new WordFinder(new Grid(letterGrid));
            var words = solver.FoundWords.ToList();
            words.Sort((a,b) => a.Value.CompareTo(b.Value));
            var rand = new System.Random();

            return words;
        }

    }

    public class Word
    {
        public List<LetterSquare> Letters;
        public string WordString { get { return string.Join("", Letters.Select(a => a.Letter).ToList()); } }
        public Word()
        {
            Letters = new List<LetterSquare>();
        }
    }

    public class Vec2
    {
        public int X, Y;

        public Vec2(int x, int y)
        {
            X = x;
            Y = y;
        }
        public static bool operator ==(Vec2 v1, Vec2 v2)
        {
            return v1.X == v2.X && v1.Y == v2.Y;
        }
        public static bool operator !=(Vec2 v1, Vec2 v2)
        {
            return v1.X != v2.X || v1.Y != v2.Y;
        }
        public static Vec2 operator +(Vec2 v1, Vec2 v2)
        {
            return
            (
               new Vec2
               (
                  v1.X + v2.X,
                  v1.Y + v2.Y
               )
            );
        }
        public static Vec2 operator -(Vec2 v1, Vec2 v2)
        {
            return
            (
               new Vec2
               (
                  v1.X - v2.X,
                  v1.Y - v2.Y
               )
            );
        }
    }

    public class LetterSquare
    {
        public string Letter { get; private set; }
        public int PointValue { get; private set; }
        public int WordMultiplier { get; private set; }
        public bool Used;
        public Vec2 Pos { get; private set; }

        private static Dictionary<string, int> pointValues = new Dictionary<string, int>
    {
       {"a",1},
       {"b",4},
       {"c",1},
       {"d",2},
       {"e",1},
       {"f",4},
       {"g",3},
       {"h",3},
       {"i",1},
       {"j",1},
       {"k",5},
       {"l",2},
       {"m",4},
       {"n",2},
       {"o",1},
       {"p",4},
       {"qu",10},
       {"r",1},
       {"s",1},
       {"t",1},
       {"u",1},
       {"v",5},
       {"w",4},
       {"x",1},
       {"y",3},
       {"z",10}
    };

        public LetterSquare(string letter, int letterMult, int wordMult, Vec2 pos)
        {
            Letter = letter;
            PointValue = pointValues[letter] * letterMult;
            WordMultiplier = wordMult;
            Pos = pos;
        }
    }

    class Grid
    {
        public List<LetterSquare> AllLetters;
        public List<LetterSquare> AdjacentLetters(LetterSquare thisLetter)
        {
            var adjacentLetters = new List<LetterSquare>();
            foreach (var letterSquare in AllLetters)
            {
                var diff = letterSquare.Pos - thisLetter.Pos;
                if (Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y) < 2 && letterSquare.Pos != thisLetter.Pos)
                {
                    adjacentLetters.Add(letterSquare);
                }
            }
            return adjacentLetters;
        }
        public Grid(List<List<string>> rowsOfLetters)
        {
            AllLetters = new List<LetterSquare>();
            int x = 0;
            int y = 0;
            foreach (var row in rowsOfLetters)
            {
                foreach (var letter in row)
                {
                    AllLetters.Add(new LetterSquare(letter, 1, 1, new Vec2(x, y)));

                    x++;
                    if (x > 3)
                        x = 0;
                }

                y++;
                if (y > 3)
                    y = 0;
            }
        }

        public LetterSquare this[int x, int y]
        {
            get { return AllLetters.Single(a => a.Pos.X == x && a.Pos.Y == y); }
        }
        public LetterSquare this[Vec2 pos]
        {
            get { return AllLetters.Single(a => a.Pos == pos); }
        }
    }

    class WordFinder
    {
        public Dictionary<Word, int> FoundWords;
        private static List<String> DictionaryWords;
        private Grid grid;
        public WordFinder(Grid gridtoSolve)
        {
            DictionaryWords = System.IO.File.ReadAllLines(@"c:\temp\sortedDict").ToList();
            grid = gridtoSolve;
            FoundWords = new Dictionary<Word, int>();
            FindAllWords();
        }

        private void FindAllWords()
        {
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    FindWordsStartingHere(new Vec2(x, y), new Word(), -1);
                }
            }
        }

        private void FindWordsStartingHere(Vec2 pos, Word preceedingLetters, int dictStartIndex)
        {
            grid[pos].Used = true;
            var potentialWord = new Word();
            potentialWord.Letters = new List<LetterSquare>(preceedingLetters.Letters);
            potentialWord.Letters.Add(grid[pos]);
            var wordResult = IsWord(potentialWord, ref dictStartIndex);
            if (wordResult.Item1 && !FoundWords.Keys.Select(a=> a.WordString).Contains(potentialWord.WordString))
            {
                FoundWords.Add(potentialWord, GetPointValue(grid.AllLetters.Where(a => a.Used).ToList()));
            }
            if (!wordResult.Item2)
            {
                grid[pos].Used = false;
                return;
            }

            foreach (var letterSquare in grid.AdjacentLetters(grid[pos]).Where(a => !a.Used))
            {
                FindWordsStartingHere(letterSquare.Pos, potentialWord, dictStartIndex);
            }
            grid[pos].Used = false;
        }

        //We keep track of the startindex so we should only ever have to look a few spots forward
        private Tuple<bool, bool> IsWord(Word word, ref int startIndex)
        {
            var potentialWord = word.WordString.ToUpper();
            var isWord = false;
            var potentiallyWord = false;
            for (int i = startIndex == -1 ? 0 : startIndex; i < DictionaryWords.Count; i++)
            {
                //we need to fast forward to the current spot in the dictionary
                if (potentialWord.CompareTo(DictionaryWords[i]) > 0)
                    continue;

                if (DictionaryWords[i].StartsWith(potentialWord))
                {
                    potentiallyWord = true;
                    startIndex = i;
                    if (potentialWord == DictionaryWords[i])
                    {
                        isWord = true;
                        startIndex = i + 1;
                    }
                }
                break;
            }

            return new Tuple<bool, bool>(isWord, potentiallyWord);
        }

        private int GetPointValue(List<LetterSquare> usedSquares)
        {
            var value = usedSquares.Sum(a => a.PointValue);
            var startVal = 3;
            for (int i = usedSquares.Count; i > 4; --i)
            {
                value += startVal;
                startVal++;
            }
            foreach (var square in usedSquares)
            {
                value *= square.WordMultiplier;
            }
            return value;
        }

    }
}
