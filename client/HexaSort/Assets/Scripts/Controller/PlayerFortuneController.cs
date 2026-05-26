using System.Collections.Generic;
using Core;
using UnityEngine;

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
        }

        private void CountFortune()
        {
            _freeCellsRatio = (double)_gridMgr.GetFreeCells() / _gridMgr.GetTotalCells();
            _freeCellsRatio = Mathf.Pow((float)_freeCellsRatio, 1.5f);

            _fieldEntropyRatio = _gridMgr.GetEntropyCells() / _gridMgr.GetTotalCells();
            _deadCellsRatio = (double)_gridMgr.GetDeadCells() / _gridMgr.GetTotalCells();
            _closeDoneStacksRatio = _gridMgr.GetCloseDoneCells() / _gridMgr.GetTotalCells();

            _currentFortune = _freeCellsRatio * 0.45 +
                              (1f - _fieldEntropyRatio) * 0.15 +
                              (1f - _deadCellsRatio) * 0.15 +
                              _closeDoneStacksRatio * 0.25;

            // Derivations from standart help
            _temporaryPressure *= 0.96f;

            if (_fieldEntropyRatio < 0.50f &&
                _freeCellsRatio > 0.35f)
            {
                _temporaryPressure += 0.015f;
            }

            _temporaryPressure =
                Mathf.Clamp01(_temporaryPressure);
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

            foreach (int symbol in pattern.Pattern)
            {
                if (!colorMap.ContainsKey(symbol))
                {
                    colorMap[symbol] =
                        PickColorForSymbol();
                }

                stack.Push(new Hex(colorMap[symbol]));
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
            while (newColor == oldColor)
            {
                newColor = PickColorForSymbol();
            }

            arr[corruptIndex] = new Hex(newColor);
            stack = new Stack<Hex>(arr);

            return new Cell(stack);
        }

        private MovePattern SelectPattern()
        {
            float desiredDifficulty =
                Mathf.Clamp01(
                    1f - (float)_currentFortune +
                    _temporaryPressure);

            float totalWeight = 0f;

            float[] weights = new float[MovePatternsLib.Patterns.Length];

            for (int i = 0; i < MovePatternsLib.Patterns.Length; i++)
            {
                MovePattern p = MovePatternsLib.Patterns[i];

                float distance = Mathf.Abs(p.Difficulty - desiredDifficulty);

                float weight = Mathf.Exp(-distance * 5f);

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

            float pressure =
                1f - (float)_currentFortune;
            float r = Random.value;

            if (pressure > 0.8f)
            {
                if (r < 0.85f)
                    intent = SpawnIntent.Help;
                else
                    intent = SpawnIntent.Neutral;
            }
            else
            {
                if (r < 0.3f)
                    intent = SpawnIntent.Neutral;
                else if (r < 0.85f)
                    intent = SpawnIntent.Pressure;
                else
                    intent = SpawnIntent.Help;
            }

            return intent;
        }

        private int PickColorForSymbol()
        {
            HashSet<int> availableColors = _gridMgr.GetAvailableColors();

            List<ColorStats> stats =
                _gridMgr.GetColorStats();

            SpawnIntent intent =
                DecideIntent();

            float totalWeight = 0f;

            List<(int color, float weight)> weights = new();

            foreach (ColorStats s in stats)
            {
                if (!availableColors.Contains(s.Color))
                {
                    weights.Add((s.Color, 0.5f));

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
                        weight += s.TopCount * 1f;

                        break;

                    case SpawnIntent.Pressure:
                        // Give rare colors
                        weight += Mathf.Max(
                            0,
                            10 - s.TotalCount);

                        // Punish dominant colors
                        weight -= s.TopCount * 1.5f;

                        break;
                }

                weight = Mathf.Max(0.1f, weight);

                weights.Add((s.Color, weight));

                totalWeight += weight;
            }

            float random =
                Random.value * totalWeight;

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
