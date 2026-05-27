using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Controller
{
    public class ColorStats
    {
        public int Color;

        public int DeadStacks;

        public int NearCompleteStacks;

        public int NeighborPotential;

        public int TopCount;

        public int TotalCount;
    }

    internal enum SpawnIntent
    {
        Help,
        Neutral,
        Pressure
    }

    public class MovePattern
    {
        // 0..1
        public readonly float Difficulty;
        public readonly int[] Pattern;

        public MovePattern(float difficulty, params int[] pattern)
        {
            Pattern = pattern;
            Difficulty = difficulty;
        }
    }

    public class PlayerFortuneController
    {
        private readonly GridManager _gridMgr;

        // Rations for fortune
        private double _closeDoneStacksRatio;

        private double _currentFortune;

        private double _deadCellsRatio;
        private double _fieldEntropyRatio;
        private double _freeCellsRatio;

        private float _temporaryPressure;

        public PlayerFortuneController(GridManager gridMgr)
        {
            _gridMgr = gridMgr;
            _temporaryPressure = 0.5f;
        }

        private void CountFortune()
        {
            _freeCellsRatio = (double)_gridMgr.GetFreeCells() / _gridMgr.GetTotalCells();
            _freeCellsRatio = Mathf.Pow((float)_freeCellsRatio, 1.5f);

            _fieldEntropyRatio = _gridMgr.GetEntropyCells() / _gridMgr.GetTotalCells();
            _deadCellsRatio = (double)_gridMgr.GetDeadCells() / _gridMgr.GetTotalCells();
            _closeDoneStacksRatio = _gridMgr.GetCloseDoneCells() / _gridMgr.GetTotalCells();

            _currentFortune = _freeCellsRatio * 0.55 +
                              (1f - _fieldEntropyRatio) * 0.15 +
                              (1f - _deadCellsRatio) * 0.15 +
                              _closeDoneStacksRatio * 0.15;

            Debug.Log("Fortune: " + _currentFortune);

            // Derivations from standart help
            _temporaryPressure *= 0.96f;

            if (_fieldEntropyRatio < 0.50f &&
                _freeCellsRatio > 0.35f)
            {
                _temporaryPressure += 0.015f;
            }

            _temporaryPressure = Mathf.Clamp01(_temporaryPressure);
        }

        public Cell[] GetNewMoves(int count)
        {
            CountFortune();

            var newMoves = new Cell[count];
            for (int i = 0; i < count; i++)
            {
                MovePattern pat = SelectPattern();
                newMoves[i] = CreateCellFromPattern(pat);
            }

            return newMoves;
        }

        private Cell CreateCellFromPattern(MovePattern pattern)
        {
            Dictionary<int, int> colorMap = new();
            Stack<Hex> stack = new();

            bool isFirst = true;
            foreach (int symbol in pattern.Pattern)
            {
                if (!colorMap.ContainsKey(symbol))
                {
                    colorMap[symbol] = PickColorForSymbol(isFirst);
                }

                stack.Push(new Hex(colorMap[symbol]));
                isFirst = false;
            }

            float corruptionChance =
                Mathf.Lerp(0.15f, 0.45f,
                    (float)_currentFortune);

            if (!(Random.value < corruptionChance) ||
                stack.Count < 4) return new Cell(stack);

            Hex[] arr = stack.ToArray();

            int corruptIndex =
                Random.Range(1, arr.Length - 1);
            int oldColor = arr[corruptIndex].Type;
            int newColor = oldColor;
            if (newColor == oldColor)
            {
                newColor = PickColorForSymbol(false);
            }

            arr[corruptIndex] = new Hex(newColor);
            stack = new Stack<Hex>(arr);

            return new Cell(stack);
        }

        private MovePattern SelectPattern()
        {
            float desiredDifficulty = Mathf.Clamp01((float)_currentFortune + _temporaryPressure);

            float totalWeight = 0f;

            float[] weights = new float[MovePatternsLib.Patterns.Length];

            for (int i = 0; i < MovePatternsLib.Patterns.Length; i++)
            {
                MovePattern p = MovePatternsLib.Patterns[i];

                float distance = Mathf.Abs(p.Difficulty - desiredDifficulty);

                float weight = Mathf.Exp(-distance * 4f);

                weights[i] = weight;

                totalWeight += weight;
            }

            float random =
                Random.value * totalWeight;

            for (int i = 0; i < weights.Length; i++)
            {
                random -= weights[i];

                if (random <= 0f)
                    return MovePatternsLib.Patterns[i];
            }

            return MovePatternsLib.Patterns[^1];
        }

        private SpawnIntent DecideIntent()
        {
            SpawnIntent intent;
            float desiredPressure = (float)_currentFortune;
            float r = Random.value;

            if (desiredPressure > 0.75f)
            {
                if (r < 0.35f)
                    intent = SpawnIntent.Help;
                else
                    intent = SpawnIntent.Neutral;
            }
            else
            {
                if (r < 0.1f)
                    intent = SpawnIntent.Help;
                else if (r < 0.25f)
                    intent = SpawnIntent.Neutral;
                else
                    intent = SpawnIntent.Pressure;
            }

            return intent;
        }

        private int PickColorForSymbol(bool isTopInPattern)
        {
            HashSet<int> availableColors = _gridMgr.GetAvailableColors();
            List<ColorStats> stats = _gridMgr.GetColorStats();
            SpawnIntent intent = DecideIntent();

            float totalWeight = 0f;
            List<(int color, float weight)> weights = new();

            foreach (ColorStats s in stats)
            {
                if (!availableColors.Contains(s.Color))
                {
                    continue;
                }

                float weight = 1f;

                switch (intent)
                {
                    case SpawnIntent.Help:
                        weight += s.TopCount * 2.5f;
                        weight += s.NeighborPotential * 1.5f;
                        weight += s.NearCompleteStacks * 4f;
                        weight -= s.DeadStacks * 2f;

                        break;

                    case SpawnIntent.Neutral:
                        // weight += s.TopCount * 1f;

                        break;

                    case SpawnIntent.Pressure:
                        // Give rare colors
                        weight += Mathf.Max(
                            0,
                            4 - s.TotalCount);

                        // Punish dominant colors
                        weight -= s.TopCount * 1.5f;

                        break;
                }

                weight = Mathf.Max(0.1f, weight);

                weights.Add((s.Color, weight));

                totalWeight += weight;
            }

            float averageWeight = totalWeight / stats.Count;
            foreach (ColorStats s in stats)
            {
                if (!availableColors.Contains(s.Color))
                {
                    float w = averageWeight + 0.5f - Random.value;
                    if (isTopInPattern && _currentFortune > 0.5f)
                        w += 3;
                    weights.Add((s.Color, w));
                    totalWeight += w;
                }
            }

            float random = Random.value * totalWeight;

            weights = weights.OrderBy(x => x.weight).ToList();
            foreach ((int color, float weight) entry in weights)
            {
                random -= entry.weight;

                if (random <= 0f)
                    return entry.color;
            }

            return weights[^1].color;
        }
    }
}
